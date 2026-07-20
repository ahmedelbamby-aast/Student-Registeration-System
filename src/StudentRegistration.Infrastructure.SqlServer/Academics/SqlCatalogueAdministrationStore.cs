using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;
using CatalogueProgram = StudentRegistration.Academics.Domain.Program;

namespace StudentRegistration.Infrastructure.SqlServer.Academics;

public sealed class SqlCatalogueAdministrationStore(
    StudentRegistrationDbContext dbContext,
    CataloguePublicationService publicationService,
    TimeProvider timeProvider) : ICatalogueAdministrationStore
{
    private static readonly JsonSerializerOptions ContentJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<Page<ProgramAdminDto>> ListProgramsAsync(
        CatalogueProgramQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var source = dbContext.Set<CatalogueProgram>().AsNoTracking();
        if (query.Active is { } active)
        {
            source = source.Where(item => item.IsActive == active);
        }
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var pattern = $"%{EscapeLike(query.Query.Trim())}%";
            source = source.Where(item =>
                EF.Functions.Like(item.Code, pattern, "\\")
                || EF.Functions.Like(item.DisplayName, pattern, "\\"));
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        source = query.Sort switch
        {
            "code-desc,id" => source.OrderByDescending(item => item.Code).ThenBy(item => item.Id),
            "displayName,id" => source.OrderBy(item => item.DisplayName).ThenBy(item => item.Id),
            "displayName-desc,id" => source.OrderByDescending(item => item.DisplayName).ThenBy(item => item.Id),
            _ => source.OrderBy(item => item.Code).ThenBy(item => item.Id),
        };
        var rows = await source
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return new(
            rows.Select(ToDto).ToArray(),
            query.Page,
            query.PageSize,
            total,
            query.Sort ?? "code,id");
    }

    public async Task<Page<CatalogueVersionSummaryDto>> ListVersionsAsync(
        CatalogueVersionQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var source = dbContext.Set<CatalogueVersion>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Scope))
        {
            var scope = NormalizeCode(query.Scope);
            source = source.Where(item => item.ScopeCode == scope);
        }
        if (!string.IsNullOrWhiteSpace(query.State))
        {
            var state = ParseVersionState(query.State);
            source = source.Where(item => item.State == state);
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        source = query.Sort switch
        {
            "version,id" => source.OrderBy(item => item.VersionCode).ThenBy(item => item.Id),
            "version-desc,id" => source.OrderByDescending(item => item.VersionCode).ThenBy(item => item.Id),
            "scope,id" => source.OrderBy(item => item.ScopeCode).ThenBy(item => item.Id),
            "state,id" => source.OrderBy(item => item.State).ThenBy(item => item.Id),
            "publishedAtUtc,id" => source.OrderBy(item => item.PublishedAtUtc).ThenBy(item => item.Id),
            _ => source.OrderByDescending(item => item.PublishedAtUtc).ThenBy(item => item.Id),
        };
        var rows = await source
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return new(
            rows.Select(ToDto).ToArray(),
            query.Page,
            query.PageSize,
            total,
            query.Sort ?? "publishedAtUtc-desc,id");
    }

    public async Task<CatalogueDraftDto?> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        var draft = await dbContext.Set<CatalogueDraft>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == draftId, cancellationToken)
            .ConfigureAwait(false);
        return draft is null ? null : ToDto(draft, ReadContent(draft));
    }

    public async Task<CatalogueDraftDto> CreateDraftAsync(
        CreateCatalogueDraftRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var version = await dbContext.Set<CatalogueVersion>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.BasedOnVersionId,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw NotFound("CATALOGUE_VERSION_NOT_FOUND", "The base catalogue version was not found.");
        var existingDraft = await dbContext.Set<CatalogueDraft>()
            .AsNoTracking()
            .Where(item => item.BasedOnVersionId == version.Id &&
                (item.State == CatalogueDraftState.Editing ||
                 item.State == CatalogueDraftState.Validated))
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (existingDraft is not null)
        {
            return ToDto(existingDraft, ReadContent(existingDraft));
        }
        var sourceDraft = await dbContext.Set<CatalogueDraft>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == version.SourceDraftId, cancellationToken)
            .ConfigureAwait(false);
        var content = ReadContent(sourceDraft);
        var validation = publicationService.Validate(content);
        var draft = new CatalogueDraft(
            Guid.NewGuid(),
            version.ScopeCode,
            version.Id,
            validation.CanonicalContentHash,
            WriteContent(content),
            string.Empty,
            CatalogueDraftState.Editing);
        dbContext.Add(draft);
        await SaveAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Entry(draft).ReloadAsync(cancellationToken).ConfigureAwait(false);
        return ToDto(draft, content);
    }

    public async Task<CatalogueDraftDto> UpdateDraftAsync(
        Guid draftId,
        CatalogueDraftMutationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var draft = await dbContext.Set<CatalogueDraft>()
            .SingleOrDefaultAsync(item => item.Id == draftId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw NotFound("DRAFT_NOT_FOUND", "The catalogue draft was not found.");
        RequireVersion(draft.Version, request.ExpectedDraftRowVersion);
        if (draft.State is CatalogueDraftState.Published or CatalogueDraftState.Abandoned)
        {
            throw Conflict("DRAFT_NOT_EDITABLE", "The catalogue draft is closed.");
        }

        var content = ApplyOperations(ReadContent(draft), request.Operations);
        var validation = publicationService.Validate(content);
        draft.ReplaceContent(validation.CanonicalContentHash, WriteContent(content));
        await SaveAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Entry(draft).ReloadAsync(cancellationToken).ConfigureAwait(false);
        return ToDto(draft, content);
    }

    public async Task<ImportBatchDto> CreateImportAsync(
        CreateImportRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var draft = await dbContext.Set<CatalogueDraft>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.DraftId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw NotFound("DRAFT_NOT_FOUND", "The catalogue draft was not found.");
        if (draft.State is CatalogueDraftState.Published or CatalogueDraftState.Abandoned)
        {
            throw Conflict("DRAFT_NOT_EDITABLE", "The catalogue draft is closed.");
        }

        var import = new ImportBatch(
            Guid.NewGuid(),
            draft.Id,
            request.Source,
            DateTime.SpecifyKind(request.AccessedOn.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
            request.ContentHash,
            request.SyntheticFields?.Count ?? 0,
            ImportBatchState.Uploaded);
        dbContext.Add(import);
        await SaveAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Entry(import).ReloadAsync(cancellationToken).ConfigureAwait(false);
        return ToDto(import);
    }

    public async Task<ImportBatchDto?> GetImportAsync(
        Guid importId,
        CancellationToken cancellationToken)
    {
        var import = await dbContext.Set<ImportBatch>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == importId, cancellationToken)
            .ConfigureAwait(false);
        return import is null ? null : ToDto(import);
    }

    public async Task<CatalogueValidationResult> ValidateImportAsync(
        CatalogueImportValidationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var import = await dbContext.Set<ImportBatch>()
            .SingleOrDefaultAsync(item => item.Id == command.ImportId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw NotFound("IMPORT_NOT_FOUND", "The catalogue import was not found.");
        var draft = await dbContext.Set<CatalogueDraft>()
            .SingleAsync(item => item.Id == import.CatalogueDraftId, cancellationToken)
            .ConfigureAwait(false);
        RequireVersion(import.Version, command.ExpectedImportRowVersion);
        RequireVersion(draft.Version, command.ExpectedDraftRowVersion);
        if (import.State is not (ImportBatchState.Uploaded or ImportBatchState.Validating))
        {
            throw Conflict("IMPORT_NOT_VALIDATABLE", "The catalogue import cannot be validated in its current state.");
        }

        var result = publicationService.ApplyValidation(draft, import, ReadContent(draft));
        await SaveAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Entry(draft).ReloadAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Entry(import).ReloadAsync(cancellationToken).ConfigureAwait(false);
        var draftVersion = Token(draft.Version);
        return new(
            result.IsValid,
            result.Errors.Select(error => new CatalogueValidationErrorDto(
                error.Row,
                error.Field,
                error.Code,
                error.Message)).ToArray(),
            result.IsValid ? PreviewToken(import.Id, draft, import, command.ActorReference) : null,
            draftVersion,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["catalogue-draft"] = draftVersion,
                ["catalogue-import"] = Token(import.Version),
            });
    }

    public async Task<CatalogueVersionSummaryDto> PublishImportAsync(
        CatalogueImportPublishCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var import = await dbContext.Set<ImportBatch>()
            .SingleOrDefaultAsync(item => item.Id == command.ImportId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw NotFound("IMPORT_NOT_FOUND", "The catalogue import was not found.");
        var draft = await dbContext.Set<CatalogueDraft>()
            .SingleAsync(item => item.Id == import.CatalogueDraftId, cancellationToken)
            .ConfigureAwait(false);
        RequireVersion(draft.Version, command.ExpectedDraftRowVersion, "STALE_PREVIEW");
        if (import.State is not ImportBatchState.Validated
            || draft.State is not CatalogueDraftState.Validated
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(PreviewToken(import.Id, draft, import, command.ActorReference)),
                Encoding.UTF8.GetBytes(command.PreviewToken)))
        {
            throw Conflict("STALE_PREVIEW", "The catalogue preview is no longer current.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var current = await dbContext.Set<CatalogueVersion>()
            .Where(item => item.ScopeCode == draft.ScopeCode
                && item.State == CatalogueVersionState.Published)
            .OrderByDescending(item => item.PublishedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var publication = publicationService.PreparePublication(
            draft,
            import,
            ReadContent(draft),
            $"DEMO-{now:yyyyMMddHHmmssfff}",
            current?.Id,
            command.ActorReference,
            now,
            now);
        current?.MarkSuperseded();
        import.MarkPublishing();
        draft.MarkPublished();
        dbContext.Add(publication.Version);
        dbContext.AddRange(publication.Programs.Select(program => new CatalogueProgram(
            program.Id,
            program.CatalogueVersionId,
            program.Code,
            program.DisplayName,
            program.IsActive,
            Clone(program.Provenance))));
        dbContext.AddRange(publication.Courses.Select(course => new Course(
            course.Id,
            course.CatalogueVersionId,
            course.Code,
            course.Title,
            course.Credits,
            course.IsActive,
            Clone(course.Provenance))));
        dbContext.AddRange(publication.Curriculum.Select(course => new CurriculumCourse(
            course.CatalogueVersionId,
            course.ProgramId,
            course.CourseId,
            course.Level,
            course.RecommendedTerm,
            course.IsRequired,
            course.CohortScope,
            Clone(course.Provenance))));
        dbContext.AddRange(publication.Prerequisites.Select(prerequisite => new CoursePrerequisite(
            prerequisite.CatalogueVersionId,
            prerequisite.CourseId,
            prerequisite.RequiredCourseId,
            prerequisite.MinimumGrade,
            Clone(prerequisite.Provenance))));
        import.MarkPublished(publication.Version.Id);
        await SaveAsync(cancellationToken).ConfigureAwait(false);
        return ToDto(publication.Version);
    }

    public async Task<StudentRoadmapDto?> GetStudentRoadmapAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Set<Student>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ApplicationUserId == applicationUserId && item.IsActive,
                cancellationToken)
            .ConfigureAwait(false);
        if (student is null)
        {
            return null;
        }

        var version = await dbContext.Set<CatalogueVersion>()
            .AsNoTracking()
            .Where(item => item.State == CatalogueVersionState.Published
                && (item.ScopeCode == student.ProgramCode
                    || item.ScopeCode.StartsWith(student.ProgramCode + "-")))
            .OrderByDescending(item => item.PublishedAtUtc)
            .ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (version is null)
        {
            return null;
        }

        var rows = await (
                from roadmap in dbContext.Set<CurriculumCourse>().AsNoTracking()
                join course in dbContext.Set<Course>().AsNoTracking()
                    on new { roadmap.CatalogueVersionId, roadmap.CourseId }
                    equals new { course.CatalogueVersionId, CourseId = course.Id }
                where roadmap.CatalogueVersionId == version.Id
                    && roadmap.RecommendedTerm != null
                    && (roadmap.CohortScope == null || roadmap.CohortScope == student.Cohort)
                select new
                {
                    course.Id,
                    course.Code,
                    course.Title,
                    course.Credits,
                    roadmap.Level,
                    RecommendedTerm = roadmap.RecommendedTerm!.Value,
                    roadmap.IsRequired,
                    roadmap.CohortScope,
                })
            .OrderBy(item => item.RecommendedTerm)
            .ThenBy(item => item.Code)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var courseIds = rows.Select(item => item.Id).ToArray();
        var courseCodes = rows.ToDictionary(item => item.Id, item => item.Code);
        var edges = await dbContext.Set<CoursePrerequisite>()
            .AsNoTracking()
            .Where(item => item.CatalogueVersionId == version.Id
                && courseIds.Contains(item.CourseId))
            .Select(item => new { item.CourseId, item.RequiredCourseId })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var attempts = await dbContext.Set<TranscriptAttempt>()
            .AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var superseded = attempts
            .Where(item => item.SupersedesAttemptId.HasValue)
            .Select(item => item.SupersedesAttemptId!.Value)
            .ToHashSet();
        var leaves = attempts.Where(item => !superseded.Contains(item.Id)).ToArray();
        var completed = leaves
            .Where(item => item.Status == TranscriptAttemptStatus.Passed)
            .Select(item => NormalizeCode(item.CourseCode))
            .ToHashSet(StringComparer.Ordinal);
        var inProgress = leaves
            .Where(item => item.Status == TranscriptAttemptStatus.InProgress)
            .Select(item => NormalizeCode(item.CourseCode))
            .ToHashSet(StringComparer.Ordinal);
        var registered = await (
                from enrollment in dbContext.Set<Enrollment>().AsNoTracking()
                join offering in dbContext.Set<CourseOffering>().AsNoTracking()
                    on enrollment.OfferingId equals offering.Id
                join course in dbContext.Set<Course>().AsNoTracking()
                    on offering.CourseId equals course.Id
                where enrollment.StudentId == student.Id
                    && enrollment.State == EnrollmentState.Active
                    && course.CatalogueVersionId == version.Id
                select course.Code)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);

        var terms = rows.GroupBy(item => new { item.RecommendedTerm, item.Level })
            .Select(group => new RoadmapTermDto(
                group.Key.RecommendedTerm,
                group.Key.Level,
                group.Select(item =>
                {
                    var prerequisites = edges
                        .Where(edge => edge.CourseId == item.Id)
                        .Select(edge => courseCodes.GetValueOrDefault(edge.RequiredCourseId))
                        .Where(code => code is not null)
                        .Cast<string>()
                        .Order(StringComparer.Ordinal)
                        .ToArray();
                    var missing = prerequisites
                        .Where(code => !completed.Contains(code))
                        .ToArray();
                    var code = NormalizeCode(item.Code);
                    var status = registered.Contains(code)
                        ? "registered"
                        : completed.Contains(code)
                        ? "completed"
                        : inProgress.Contains(code)
                            ? "in-progress"
                            : missing.Length == 0
                                ? "available"
                                : "locked";
                    return new RoadmapSubjectDto(
                        item.Id,
                        code,
                        item.Title,
                        item.Credits,
                        item.IsRequired,
                        item.CohortScope,
                        status,
                        item.RecommendedTerm == 1,
                        prerequisites,
                        missing);
                }).ToArray()))
            .OrderBy(term => term.RecommendedTerm)
            .ThenBy(term => term.Level)
            .ToArray();
        return new(student.ProgramCode, student.Cohort, version.VersionCode, terms);
    }

    private static CatalogueDraftContent ApplyOperations(
        CatalogueDraftContent source,
        IReadOnlyList<CatalogueDraftOperation> operations)
    {
        var courses = source.Courses.ToDictionary(
            item => NormalizeCode(item.Code),
            item => item,
            StringComparer.Ordinal);
        foreach (var operation in operations)
        {
            var kind = operation.Kind.Trim().ToLowerInvariant();
            switch (kind)
            {
                case "upsert-program":
                    if (operation.Program is null
                        || NormalizeCode(operation.Program.Code) != NormalizeCode(source.ScopeCode))
                    {
                        throw Validation("VALIDATION_ERROR", "The program operation does not match the draft scope.");
                    }
                    break;
                case "upsert-course":
                    if (operation.Course is null)
                    {
                        throw Validation("VALIDATION_ERROR", "The course operation is incomplete.");
                    }
                    var code = NormalizeCode(operation.Course.Code);
                    var existing = courses.GetValueOrDefault(code);
                    courses[code] = new CatalogueCourseDefinition(
                        code,
                        operation.Course.Title,
                        operation.Course.Credits,
                        operation.Course.Active,
                        existing?.Sequence ?? courses.Count + 1,
                        existing?.PrerequisiteCodes ?? [],
                        existing?.MinimumGpa,
                        existing?.MinimumEarnedCredits,
                        FromDto(operation.Course.Provenance),
                        existing?.Level,
                        existing?.IsRequired ?? true,
                        existing?.CohortScope);
                    break;
                case "upsert-curriculum-course":
                    if (operation.CurriculumCourse is null
                        || !courses.TryGetValue(
                            NormalizeCode(operation.CurriculumCourse.CourseCode),
                            out var curriculumCourse))
                    {
                        throw Validation("VALIDATION_ERROR", "The curriculum course does not reference a draft course.");
                    }
                    courses[curriculumCourse.Code] = curriculumCourse with
                    {
                        Sequence = operation.CurriculumCourse.TermSequence
                            ?? Math.Max(1, operation.CurriculumCourse.Level * 2 - 1),
                        Level = operation.CurriculumCourse.Level,
                        IsRequired = operation.CurriculumCourse.Required,
                        CohortScope = string.IsNullOrWhiteSpace(operation.CurriculumCourse.CohortScope)
                            ? null
                            : operation.CurriculumCourse.CohortScope.Trim(),
                        Provenance = FromDto(operation.CurriculumCourse.Provenance),
                    };
                    break;
                case "remove-curriculum-course":
                    if (string.IsNullOrWhiteSpace(operation.CourseCode))
                    {
                        throw Validation("VALIDATION_ERROR", "A course code is required.");
                    }
                    courses.Remove(NormalizeCode(operation.CourseCode));
                    break;
                case "set-prerequisites":
                    if (string.IsNullOrWhiteSpace(operation.CourseCode)
                        || !courses.TryGetValue(NormalizeCode(operation.CourseCode), out var course))
                    {
                        throw Validation("VALIDATION_ERROR", "The prerequisite operation does not reference a draft course.");
                    }
                    courses[course.Code] = course with
                    {
                        PrerequisiteCodes = (operation.RequiredCourseCodes ?? [])
                            .Select(NormalizeCode)
                            .Distinct(StringComparer.Ordinal)
                            .Order(StringComparer.Ordinal)
                            .ToArray(),
                        Provenance = operation.Provenance is null
                            ? course.Provenance
                            : FromDto(operation.Provenance),
                    };
                    break;
                default:
                    throw Validation("VALIDATION_ERROR", "The catalogue operation kind is not supported.");
            }
        }

        return new(source.ScopeCode, courses.Values.OrderBy(item => item.Sequence).ThenBy(item => item.Code).ToArray());
    }

    private static CatalogueDraftContent ReadContent(CatalogueDraft draft)
    {
        try
        {
            var stored = JsonSerializer.Deserialize<StoredCatalogueContent>(
                draft.ContentJson,
                ContentJsonOptions);
            if (stored is null || stored.Courses is null)
            {
                throw new JsonException();
            }

            return new(
                stored.ScopeCode,
                stored.Courses.Select(course => new CatalogueCourseDefinition(
                    course.Code,
                    course.Title,
                    course.Credits,
                    course.IsActive,
                    course.Sequence,
                    course.PrerequisiteCodes ?? [],
                    course.MinimumGpa,
                    course.MinimumEarnedCredits,
                    new CatalogueFieldProvenance(
                        course.Provenance.SourceReference,
                        course.Provenance.AccessedOn,
                        course.Provenance.SourceKind,
                        course.Provenance.SyntheticFields ?? []),
                    course.Level,
                    course.IsRequired,
                    course.CohortScope)).ToArray());
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new CatalogueStoreException(
                CatalogueStoreFailure.Validation,
                "CATALOGUE_CONTENT_INVALID",
                "The stored catalogue draft content is invalid.");
        }
    }

    private static string WriteContent(CatalogueDraftContent content) =>
        JsonSerializer.Serialize(new StoredCatalogueContent(
            content.ScopeCode,
            content.Courses.Select(course => new StoredCatalogueCourse(
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
                new StoredCatalogueProvenance(
                    course.Provenance.SourceReference,
                    course.Provenance.AccessedOn,
                    course.Provenance.SourceKind,
                    course.Provenance.SyntheticFields))).ToArray()));

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw Conflict("STALE_VERSION", "The catalogue data changed.");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw Conflict("PUBLICATION_CONFLICT", "The catalogue scope or version already exists.");
        }
    }

    private static CatalogueDraftDto ToDto(CatalogueDraft draft, CatalogueDraftContent content)
    {
        var programId = StableId(draft.Id, $"program:{content.ScopeCode}");
        var provenance = content.Courses.FirstOrDefault()?.Provenance
            ?? new CatalogueFieldProvenance("synthetic-demo", new DateOnly(2026, 7, 20), CatalogueSourceKind.SyntheticDemo, []);
        var programs = new[]
        {
            new ProgramAdminDto(programId, content.ScopeCode, content.ScopeCode, true, ToDto(provenance)),
        };
        var courses = content.Courses.Select(item => new CourseAdminDto(
            StableId(draft.Id, $"course:{NormalizeCode(item.Code)}"),
            NormalizeCode(item.Code),
            item.Title,
            item.Credits,
            item.IsActive,
            ToDto(item.Provenance),
            Token(draft.Version))).ToArray();
        var curriculum = content.Courses.Select(item => new CurriculumCourseAdminDto(
            content.ScopeCode,
            NormalizeCode(item.Code),
            item.Level ?? Math.Max(1, (item.Sequence + 1) / 2),
            item.Sequence,
            item.IsRequired,
            item.CohortScope,
            ToDto(item.Provenance))).ToArray();
        return new(
            draft.Id,
            draft.ScopeCode,
            draft.BasedOnVersionId,
            State(draft.State),
            draft.CanonicalContentHash,
            Token(draft.Version),
            programs,
            courses,
            curriculum);
    }

    private static ProgramAdminDto ToDto(CatalogueProgram program) => new(
        program.Id,
        program.Code,
        program.DisplayName,
        program.IsActive,
        ToDto(program.Provenance));

    private static CatalogueVersionSummaryDto ToDto(CatalogueVersion version) => new(
        version.Id,
        version.ScopeCode,
        version.VersionCode,
        State(version.State),
        version.SourceReference,
        version.PublishedAtUtc);

    private static ImportBatchDto ToDto(ImportBatch import)
    {
        var errors = import.Errors.Take(100).Select(error => new ImportRowErrorDto(
            error.SourceRow,
            error.Field,
            error.Code,
            error.Message)).ToArray();
        return new(
            import.Id,
            import.CatalogueDraftId,
            State(import.State),
            import.SourceReference,
            DateOnly.FromDateTime(import.AccessedAtUtc),
            import.ContentHash,
            import.SyntheticFieldCount,
            Token(import.Version),
            errors,
            import.Errors.Count,
            import.Errors.Count > errors.Length,
            import.PublishedVersionId);
    }

    private static CatalogueFieldProvenanceDto ToDto(CatalogueFieldProvenance provenance) => new(
        provenance.SourceReference,
        provenance.AccessedOn,
        provenance.SourceKind == CatalogueSourceKind.OfficialSource ? "official-source" : "synthetic-demo",
        provenance.SyntheticFields);

    private static CatalogueFieldProvenance FromDto(CatalogueFieldProvenanceDto provenance) => new(
        provenance.SourceReference,
        provenance.AccessedOn,
        provenance.SourceKind.Trim().ToLowerInvariant() switch
        {
            "official-source" => CatalogueSourceKind.OfficialSource,
            "synthetic-demo" => CatalogueSourceKind.SyntheticDemo,
            _ => throw Validation("PROVENANCE_INVALID", "The catalogue provenance kind is invalid."),
        },
        provenance.SyntheticFields);

    private static CatalogueFieldProvenance Clone(CatalogueFieldProvenance provenance) => new(
        provenance.SourceReference,
        provenance.AccessedOn,
        provenance.SourceKind,
        provenance.SyntheticFields);

    private static void RequireVersion(byte[] current, string expected, string code = "STALE_VERSION")
    {
        byte[] supplied;
        try
        {
            supplied = Convert.FromBase64String(expected);
        }
        catch (FormatException)
        {
            throw Conflict(code, "The catalogue version is stale.");
        }
        if (!current.AsSpan().SequenceEqual(supplied))
        {
            throw Conflict(code, "The catalogue version is stale.");
        }
    }

    private static string PreviewToken(
        Guid importId,
        CatalogueDraft draft,
        ImportBatch import,
        string actor) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
            '|',
            importId.ToString("D"),
            draft.ScopeCode,
            draft.CanonicalContentHash,
            Token(draft.Version),
            Token(import.Version),
            actor.Trim()))));

    private static Guid StableId(Guid scope, string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{scope:D}:{value}"));
        return new Guid(bytes.AsSpan(0, 16));
    }

    private static string Token(byte[] value) => Convert.ToBase64String(value);

    private static string NormalizeCode(string value) => value.Trim().Normalize().ToUpperInvariant();

    private static string EscapeLike(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal);

    private static CatalogueVersionState ParseVersionState(string value) => value.Trim().ToLowerInvariant() switch
    {
        "published" => CatalogueVersionState.Published,
        "superseded" => CatalogueVersionState.Superseded,
        _ => throw Validation("VALIDATION_ERROR", "The catalogue version state is invalid."),
    };

    private static string State(CatalogueDraftState value) => value.ToString().ToLowerInvariant();

    private static string State(CatalogueVersionState value) => value.ToString().ToLowerInvariant();

    private static string State(ImportBatchState value) => value.ToString().ToLowerInvariant();

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static CatalogueStoreException Validation(string code, string message) =>
        new(CatalogueStoreFailure.Validation, code, message);

    private static CatalogueStoreException NotFound(string code, string message) =>
        new(CatalogueStoreFailure.NotFound, code, message);

    private static CatalogueStoreException Conflict(string code, string message) =>
        new(CatalogueStoreFailure.Conflict, code, message);

    private sealed record StoredCatalogueContent(
        string ScopeCode,
        IReadOnlyList<StoredCatalogueCourse> Courses);

    private sealed record StoredCatalogueCourse(
        string Code,
        string Title,
        decimal Credits,
        bool IsActive,
        int Sequence,
        IReadOnlyList<string>? PrerequisiteCodes,
        decimal? MinimumGpa,
        decimal? MinimumEarnedCredits,
        int? Level,
        bool IsRequired,
        string? CohortScope,
        StoredCatalogueProvenance Provenance);

    private sealed record StoredCatalogueProvenance(
        string SourceReference,
        DateOnly AccessedOn,
        CatalogueSourceKind SourceKind,
        IReadOnlyList<string>? SyntheticFields);
}
