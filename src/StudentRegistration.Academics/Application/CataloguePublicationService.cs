using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.Academics.Application;

public sealed record CatalogueCourseDefinition(
    string Code,
    string Title,
    decimal Credits,
    bool IsActive,
    int Sequence,
    IReadOnlyList<string> PrerequisiteCodes,
    decimal? MinimumGpa,
    decimal? MinimumEarnedCredits,
    CatalogueFieldProvenance Provenance,
    int? Level = null,
    bool IsRequired = true,
    string? CohortScope = null);

public sealed record CatalogueDraftContent(
    string ScopeCode,
    IReadOnlyList<CatalogueCourseDefinition> Courses);

public sealed record CatalogueValidationError(
    int? Row,
    string? Field,
    string Code,
    string Message,
    IReadOnlyList<string> ResourceCodes);

public sealed record CatalogueGraphValidationResult(
    bool IsValid,
    string CanonicalContentHash,
    IReadOnlyList<CatalogueValidationError> Errors);

public sealed record PreparedCataloguePublication(
    CatalogueVersion Version,
    IReadOnlyList<StudentRegistration.Academics.Domain.Program> Programs,
    IReadOnlyList<Course> Courses,
    IReadOnlyList<CurriculumCourse> Curriculum,
    IReadOnlyList<CoursePrerequisite> Prerequisites);

public sealed class CataloguePublicationService
{
    private const string SourceReference =
        "https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655";
    private static readonly DateOnly AccessedOn = new(2026, 7, 13);

    public static CatalogueDraftContent CreateDemoCurriculum() =>
        new(
            "AI-DS",
            [
                Course("BA101", "Calculus I", 1),
                Course("BA113", "Physics", 1),
                Course("GN111", "Introduction to Computing", 1),
                Course("GN112", "Introduction to Problem Solving and Programming", 1),
                Course("BA102", "Calculus II", 2, ["BA101"]),
                Course("GN121", "Data Structures", 2, ["GN112"]),
                Course("GN123", "Digital Logic Design", 2, ["GN111"]),
                Course("BA203", "Probability and Statistics", 3, ["BA102"]),
                Course("GN211", "Computing Algorithms", 3, ["GN121"]),
                Course("IN211", "Fundamentals of Artificial Intelligence", 3, ["GN111", "GN112"]),
                Course("DS221", "Fundamentals of Data Science", 4, ["GN111", "GN112"]),
                Course("GN223", "Software Engineering", 4, ["GN121"]),
                Course("IN221", "Machine Learning", 4, ["IN211", "BA203"]),
                Course("IN311", "Deep Learning", 5, ["IN221"]),
                Course("DS312", "Programming for Data Science", 5, ["GN121"]),
                Course("DS322", "Statistics for Data Science", 6, ["BA203"]),
                Course("DS324", "Computational Linguistics", 6, ["IN311"]),
                Course("DS413", "Project I", 7, ["DS322", "IN311"], minimumGpa: 2.0m, minimumEarnedCredits: 96m),
                Course("DS421", "Project II", 8, ["DS413"]),
            ]);

    public CatalogueGraphValidationResult Validate(CatalogueDraftContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var scope = RequiredCode(content.ScopeCode, nameof(content.ScopeCode));
        var courses = content.Courses
            ?? throw new ArgumentNullException(nameof(content.Courses));
        var normalized = courses
            .Select((course, index) => Normalize(course, index + 1))
            .ToArray();
        var errors = new List<CatalogueValidationError>();

        foreach (var duplicate in normalized
                     .GroupBy(course => course.Code, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            errors.Add(Error(
                duplicate.Min(course => course.Row),
                "code",
                "DUPLICATE_COURSE_CODE",
                $"Course code {duplicate.Key} is duplicated.",
                duplicate.Key));
        }

        foreach (var course in normalized)
        {
            if (course.Credits != 3m)
            {
                errors.Add(Error(
                    course.Row,
                    "credits",
                    "INVALID_CREDITS",
                    $"Course {course.Code} must have exactly three credits.",
                    course.Code));
            }

            if (course.Sequence < 1)
            {
                errors.Add(Error(
                    course.Row,
                    "sequence",
                    "INVALID_SEQUENCE",
                    $"Course {course.Code} has an invalid sequence.",
                    course.Code));
            }

            if (course.Sequence == 1 && course.PrerequisiteCodes.Count > 0)
            {
                errors.Add(Error(
                    course.Row,
                    "prerequisiteCodes",
                    "FIRST_TERM_PREREQUISITE_INVALID",
                    $"First-term course {course.Code} cannot have a prerequisite.",
                    course.Code));
            }

            if (course.Sequence > 1 && course.PrerequisiteCodes.Count == 0)
            {
                errors.Add(Error(
                    course.Row,
                    "prerequisiteCodes",
                    "LATER_TERM_PREREQUISITE_REQUIRED",
                    $"Later-term course {course.Code} requires a prerequisite.",
                    course.Code));
            }

            if (course.Provenance.SourceKind is CatalogueSourceKind.OfficialSource
                && !course.Provenance.SyntheticFields.Contains(
                    "Credits",
                    StringComparer.OrdinalIgnoreCase))
            {
                errors.Add(Error(
                    course.Row,
                    "provenance.syntheticFields",
                    "PROVENANCE_INVALID",
                    $"Official-source course {course.Code} must classify synthetic credits.",
                    course.Code));
            }
        }

        var codes = normalized.Select(course => course.Code).ToHashSet(StringComparer.Ordinal);
        foreach (var course in normalized)
        {
            foreach (var prerequisite in course.PrerequisiteCodes)
            {
                if (!codes.Contains(prerequisite))
                {
                    errors.Add(Error(
                        course.Row,
                        "prerequisiteCodes",
                        "MISSING_REFERENCE",
                        $"Course {course.Code} references missing prerequisite {prerequisite}.",
                        course.Code,
                        prerequisite));
                }
            }
        }

        var cycle = FindCycle(normalized);
        if (cycle.Count > 0)
        {
            errors.Add(new(
                Row: null,
                Field: "prerequisiteCodes",
                Code: "PREREQUISITE_CYCLE",
                Message: $"Prerequisite cycle detected: {string.Join(" -> ", cycle)}.",
                ResourceCodes: cycle));
        }

        var canonical = CanonicalJson(scope, normalized);
        return new(
            errors.Count == 0,
            $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))}",
            errors);
    }

    public CatalogueGraphValidationResult ApplyValidation(
        CatalogueDraft draft,
        ImportBatch import,
        CatalogueDraftContent content)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(import);
        if (import.CatalogueDraftId != draft.Id)
        {
            throw new ArgumentException("The import must target the supplied draft.", nameof(import));
        }

        var result = Validate(content);
        var canonical = CanonicalJson(
            RequiredCode(content.ScopeCode, nameof(content.ScopeCode)),
            content.Courses.Select((course, index) => Normalize(course, index + 1)).ToArray());
        draft.ReplaceContent(result.CanonicalContentHash, canonical);
        if (!result.IsValid)
        {
            import.MarkInvalid(result.Errors.Select(error =>
                new ImportRowError(error.Row, error.Field, error.Code, error.Message)));
            return result;
        }

        draft.MarkValidated(JsonSerializer.Serialize(new
        {
            valid = true,
            errorCount = 0,
            result.CanonicalContentHash,
        }));
        import.MarkValidated();
        return result;
    }

    public PreparedCataloguePublication PreparePublication(
        CatalogueDraft draft,
        ImportBatch import,
        CatalogueDraftContent content,
        string versionCode,
        Guid? supersedesId,
        string publishedBy,
        DateTime effectiveFromUtc,
        DateTime publishedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(import);
        if (draft.State is not CatalogueDraftState.Validated
            || import.State is not ImportBatchState.Validated)
        {
            throw new InvalidOperationException(
                "A validated draft and import are required before publication.");
        }

        var validation = Validate(content);
        if (!validation.IsValid
            || !string.Equals(
                validation.CanonicalContentHash,
                draft.CanonicalContentHash,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The validated catalogue content has changed.");
        }

        var version = new CatalogueVersion(
            Guid.NewGuid(),
            draft.Id,
            supersedesId,
            draft.ScopeCode,
            versionCode,
            import.SourceReference,
            effectiveFromUtc,
            publishedAtUtc,
            publishedBy,
            CatalogueVersionState.Published);
        var program = new StudentRegistration.Academics.Domain.Program(
            Guid.NewGuid(),
            version.Id,
            draft.ScopeCode,
            "Data Science",
            true,
            new CatalogueFieldProvenance(
                SourceReference,
                AccessedOn,
                CatalogueSourceKind.OfficialSource,
                ["IsActive"]));
        var courseIds = content.Courses.ToDictionary(
            course => RequiredCode(course.Code, nameof(course.Code)),
            _ => Guid.NewGuid(),
            StringComparer.Ordinal);
        var courses = content.Courses.Select(course =>
        {
            var code = RequiredCode(course.Code, nameof(course.Code));
            return new Course(
                courseIds[code],
                version.Id,
                code,
                course.Title,
                course.Credits,
                course.IsActive,
                Clone(course.Provenance));
        }).ToArray();
        var curriculum = content.Courses.Select(course =>
            new CurriculumCourse(
                version.Id,
                program.Id,
                courseIds[RequiredCode(course.Code, nameof(course.Code))],
                course.Level ?? Math.Max(1, (course.Sequence + 1) / 2),
                course.Sequence,
                course.IsRequired,
                course.CohortScope,
                Clone(course.Provenance))).ToArray();
        var prerequisites = content.Courses
            .SelectMany(course =>
            {
                var code = RequiredCode(course.Code, nameof(course.Code));
                return course.PrerequisiteCodes.Select(required =>
                    new CoursePrerequisite(
                        version.Id,
                        courseIds[code],
                        courseIds[RequiredCode(required, nameof(course.PrerequisiteCodes))],
                        minimumGrade: null,
                        Clone(course.Provenance)));
            })
            .ToArray();

        CoursePrerequisite.ValidateGraph(courseIds.Values, prerequisites);
        return new(version, [program], courses, curriculum, prerequisites);
    }

    private static CatalogueCourseDefinition Course(
        string code,
        string title,
        int sequence,
        IReadOnlyList<string>? prerequisites = null,
        decimal? minimumGpa = null,
        decimal? minimumEarnedCredits = null) =>
        new(
            code,
            title,
            3m,
            true,
            sequence,
            prerequisites ?? [],
            minimumGpa,
            minimumEarnedCredits,
            new CatalogueFieldProvenance(
                SourceReference,
                AccessedOn,
                CatalogueSourceKind.OfficialSource,
                ["Credits", "IsActive"]));

    private static CatalogueFieldProvenance Clone(
        CatalogueFieldProvenance provenance) =>
        new(
            provenance.SourceReference,
            provenance.AccessedOn,
            provenance.SourceKind,
            provenance.SyntheticFields.ToArray());

    private static NormalizedCourse Normalize(
        CatalogueCourseDefinition course,
        int row)
    {
        ArgumentNullException.ThrowIfNull(course);
        return new(
            row,
            RequiredCode(course.Code, nameof(course.Code)),
            Required(course.Title, nameof(course.Title)),
            course.Credits,
            course.IsActive,
            course.Sequence,
            (course.PrerequisiteCodes ?? [])
                .Select(code => RequiredCode(code, nameof(course.PrerequisiteCodes)))
                .Order(StringComparer.Ordinal)
                .ToArray(),
            course.MinimumGpa,
            course.MinimumEarnedCredits,
            course.Level,
            course.IsRequired,
            string.IsNullOrWhiteSpace(course.CohortScope) ? null : course.CohortScope.Trim(),
            course.Provenance ?? throw new ArgumentNullException(nameof(course.Provenance)));
    }

    private static IReadOnlyList<string> FindCycle(IReadOnlyList<NormalizedCourse> courses)
    {
        var adjacency = courses
            .GroupBy(course => course.Code, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(course => course.Row).First().PrerequisiteCodes,
                StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();

        foreach (var code in adjacency.Keys.Order(StringComparer.Ordinal))
        {
            var cycle = Visit(code);
            if (cycle.Count > 0)
            {
                return cycle;
            }
        }

        return [];

        IReadOnlyList<string> Visit(string code)
        {
            if (visited.Contains(code) || !adjacency.ContainsKey(code))
            {
                return [];
            }

            if (!visiting.Add(code))
            {
                var start = path.IndexOf(code);
                return path.Skip(Math.Max(start, 0)).Append(code).ToArray();
            }

            path.Add(code);
            foreach (var required in adjacency[code])
            {
                var cycle = Visit(required);
                if (cycle.Count > 0)
                {
                    return cycle;
                }
            }

            path.RemoveAt(path.Count - 1);
            visiting.Remove(code);
            visited.Add(code);
            return [];
        }
    }

    private static string CanonicalJson(
        string scopeCode,
        IReadOnlyList<NormalizedCourse> courses) =>
        JsonSerializer.Serialize(new
        {
            scopeCode,
            courses = courses
                .OrderBy(course => course.Code, StringComparer.Ordinal)
                .Select(course => new
                {
                    course.Code,
                    course.Title,
                    course.Credits,
                    course.IsActive,
                    course.Sequence,
                    course.PrerequisiteCodes,
                    course.MinimumGpa,
                    course.MinimumEarnedCredits,
                    course.Level,
                    course.IsRequired,
                    course.CohortScope,
                    provenance = new
                    {
                        course.Provenance.SourceReference,
                        course.Provenance.AccessedOn,
                        course.Provenance.SourceKind,
                        course.Provenance.SyntheticFields,
                    },
                }),
        });

    private static CatalogueValidationError Error(
        int? row,
        string field,
        string code,
        string message,
        params string[] resourceCodes) =>
        new(row, field, code, message, resourceCodes);

    private static string Required(string? value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A non-empty value is required.", parameterName)
            : value.Trim();

    private static string RequiredCode(string? value, string parameterName) =>
        Required(value, parameterName).Normalize().ToUpperInvariant();

    private sealed record NormalizedCourse(
        int Row,
        string Code,
        string Title,
        decimal Credits,
        bool IsActive,
        int Sequence,
        IReadOnlyList<string> PrerequisiteCodes,
        decimal? MinimumGpa,
        decimal? MinimumEarnedCredits,
        int? Level,
        bool IsRequired,
        string? CohortScope,
        CatalogueFieldProvenance Provenance);
}
