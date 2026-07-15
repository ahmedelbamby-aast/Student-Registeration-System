using StudentRegistration.Contracts;

namespace StudentRegistration.Contracts.Academics;

public sealed record TranscriptSummaryDto
{
    public TranscriptSummaryDto(
        decimal attemptedCredits,
        decimal earnedCredits,
        int attemptCount)
    {
        AttemptedCredits = AcademicContractGuard.NonNegative(
            attemptedCredits,
            nameof(attemptedCredits));
        EarnedCredits = AcademicContractGuard.NonNegative(
            earnedCredits,
            nameof(earnedCredits));
        if (earnedCredits > attemptedCredits)
        {
            throw new ArgumentOutOfRangeException(
                nameof(earnedCredits),
                earnedCredits,
                "Earned credits cannot exceed attempted credits.");
        }

        if (attemptCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attemptCount),
                attemptCount,
                "Attempt count cannot be negative.");
        }

        AttemptCount = attemptCount;
    }

    public decimal AttemptedCredits { get; }

    public decimal EarnedCredits { get; }

    public int AttemptCount { get; }
}

public sealed record TranscriptAttemptDto
{
    private static readonly HashSet<string> AllowedStatuses = new(
        ["in-progress", "passed", "failed", "withdrawn"],
        StringComparer.Ordinal);

    public TranscriptAttemptDto(
        string attemptId,
        string? supersedesAttemptId,
        string courseCode,
        string termCode,
        decimal credits,
        string? grade,
        string status,
        string provenance)
    {
        AttemptId = AcademicContractGuard.Required(attemptId, nameof(attemptId), 100);
        SupersedesAttemptId = AcademicContractGuard.Optional(
            supersedesAttemptId,
            nameof(supersedesAttemptId),
            100);
        CourseCode = AcademicContractGuard.Required(courseCode, nameof(courseCode), 50);
        TermCode = AcademicContractGuard.Required(termCode, nameof(termCode), 50);
        if (credits <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credits),
                credits,
                "Attempt credits must be positive.");
        }

        Credits = credits;
        Grade = AcademicContractGuard.Optional(grade, nameof(grade), 50);
        Status = AcademicContractGuard.Required(status, nameof(status), 50);
        if (!AllowedStatuses.Contains(Status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "A declared transcript-attempt status is required.");
        }

        Provenance = AcademicContractGuard.Required(
            provenance,
            nameof(provenance),
            200);
    }

    public string AttemptId { get; }

    public string? SupersedesAttemptId { get; }

    public string CourseCode { get; }

    public string TermCode { get; }

    public decimal Credits { get; }

    public string? Grade { get; }

    public string Status { get; }

    public string Provenance { get; }
}

public sealed record AcademicProvenanceDto
{
    public AcademicProvenanceDto(
        string source,
        string reference,
        DateTime importedAtUtc)
    {
        Source = AcademicContractGuard.Required(source, nameof(source), 200);
        Reference = AcademicContractGuard.Required(reference, nameof(reference), 200);
        ImportedAtUtc = AcademicContractGuard.Utc(importedAtUtc, nameof(importedAtUtc));
    }

    public string Source { get; }

    public string Reference { get; }

    public DateTime ImportedAtUtc { get; }
}

public record AcademicHoldDto
{
    public AcademicHoldDto(
        string termId,
        string code,
        string message,
        bool blocksRegistration,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string source)
    {
        TermId = AcademicContractGuard.Required(termId, nameof(termId), 100);
        Code = AcademicContractGuard.Required(code, nameof(code), 100);
        Message = AcademicContractGuard.Required(message, nameof(message), 500);
        BlocksRegistration = blocksRegistration;
        EffectiveFromUtc = AcademicContractGuard.Utc(
            effectiveFromUtc,
            nameof(effectiveFromUtc));
        EffectiveToUtc = AcademicContractGuard.OptionalUtc(
            effectiveToUtc,
            nameof(effectiveToUtc));
        if (EffectiveToUtc is not null && EffectiveToUtc <= EffectiveFromUtc)
        {
            throw new ArgumentException(
                "The hold end instant must be after its start instant.",
                nameof(effectiveToUtc));
        }

        Source = AcademicContractGuard.Required(source, nameof(source), 200);
    }

    public string TermId { get; }

    public string Code { get; }

    public string Message { get; }

    public bool BlocksRegistration { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public string Source { get; }
}

public sealed record AdminAcademicHoldDto : AcademicHoldDto
{
    public AdminAcademicHoldDto(
        string termId,
        string code,
        string message,
        bool blocksRegistration,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string source,
        string holdId,
        string sourceReference)
        : base(
            termId,
            code,
            message,
            blocksRegistration,
            effectiveFromUtc,
            effectiveToUtc,
            source)
    {
        HoldId = AcademicContractGuard.Required(holdId, nameof(holdId), 100);
        SourceReference = AcademicContractGuard.Required(
            sourceReference,
            nameof(sourceReference),
            200);
    }

    public string HoldId { get; }

    public string SourceReference { get; }
}

public sealed record StudentAcademicContextDto
{
    public StudentAcademicContextDto(
        string universityId,
        string programCode,
        string cohort,
        decimal currentGpa,
        decimal earnedCredits,
        string standing,
        TranscriptSummaryDto transcriptSummary,
        Page<TranscriptAttemptDto> transcriptAttempts,
        IReadOnlyList<AcademicHoldDto> activeHolds,
        string dataVersion,
        DateTime dataAsOfUtc,
        Page<AcademicProvenanceDto> provenance)
    {
        UniversityId = AcademicContractGuard.Required(
            universityId,
            nameof(universityId),
            50);
        ProgramCode = AcademicContractGuard.Required(
            programCode,
            nameof(programCode),
            50);
        Cohort = AcademicContractGuard.Required(cohort, nameof(cohort), 50);
        CurrentGpa = AcademicContractGuard.Gpa(currentGpa, nameof(currentGpa));
        EarnedCredits = AcademicContractGuard.NonNegative(
            earnedCredits,
            nameof(earnedCredits));
        Standing = AcademicContractGuard.Required(standing, nameof(standing), 100);
        TranscriptSummary = transcriptSummary ??
            throw new ArgumentNullException(nameof(transcriptSummary));
        TranscriptAttempts = transcriptAttempts ??
            throw new ArgumentNullException(nameof(transcriptAttempts));
        ActiveHolds = AcademicContractGuard.CompleteHolds(
            activeHolds,
            nameof(activeHolds));
        DataVersion = AcademicContractGuard.Required(
            dataVersion,
            nameof(dataVersion),
            200);
        DataAsOfUtc = AcademicContractGuard.Utc(dataAsOfUtc, nameof(dataAsOfUtc));
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
    }

    public string UniversityId { get; }

    public string ProgramCode { get; }

    public string Cohort { get; }

    public decimal CurrentGpa { get; }

    public decimal EarnedCredits { get; }

    public string Standing { get; }

    public TranscriptSummaryDto TranscriptSummary { get; }

    public Page<TranscriptAttemptDto> TranscriptAttempts { get; }

    public IReadOnlyList<AcademicHoldDto> ActiveHolds { get; }

    public string DataVersion { get; }

    public DateTime DataAsOfUtc { get; }

    public Page<AcademicProvenanceDto> Provenance { get; }
}

public sealed record AdminStudentAcademicContextDto
{
    public AdminStudentAcademicContextDto(
        string studentId,
        string termId,
        string universityId,
        string programCode,
        string cohort,
        decimal currentGpa,
        decimal earnedCredits,
        string standing,
        TranscriptSummaryDto transcriptSummary,
        Page<TranscriptAttemptDto> transcriptAttempts,
        IReadOnlyList<AdminAcademicHoldDto> activeHolds,
        string dataVersion,
        DateTime dataAsOfUtc,
        Page<AcademicProvenanceDto> provenance,
        string studentRowVersion,
        string studentTermStateRowVersion)
    {
        StudentId = AcademicContractGuard.Required(studentId, nameof(studentId), 100);
        TermId = AcademicContractGuard.Required(termId, nameof(termId), 100);
        UniversityId = AcademicContractGuard.Required(
            universityId,
            nameof(universityId),
            50);
        ProgramCode = AcademicContractGuard.Required(
            programCode,
            nameof(programCode),
            50);
        Cohort = AcademicContractGuard.Required(cohort, nameof(cohort), 50);
        CurrentGpa = AcademicContractGuard.Gpa(currentGpa, nameof(currentGpa));
        EarnedCredits = AcademicContractGuard.NonNegative(
            earnedCredits,
            nameof(earnedCredits));
        Standing = AcademicContractGuard.Required(standing, nameof(standing), 100);
        TranscriptSummary = transcriptSummary ??
            throw new ArgumentNullException(nameof(transcriptSummary));
        TranscriptAttempts = transcriptAttempts ??
            throw new ArgumentNullException(nameof(transcriptAttempts));
        ActiveHolds = AcademicContractGuard.CompleteAdminHolds(
            activeHolds,
            nameof(activeHolds));
        DataVersion = AcademicContractGuard.Required(
            dataVersion,
            nameof(dataVersion),
            200);
        DataAsOfUtc = AcademicContractGuard.Utc(dataAsOfUtc, nameof(dataAsOfUtc));
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
        StudentRowVersion = AcademicContractGuard.Required(
            studentRowVersion,
            nameof(studentRowVersion),
            128);
        StudentTermStateRowVersion = AcademicContractGuard.Required(
            studentTermStateRowVersion,
            nameof(studentTermStateRowVersion),
            128);
    }

    public string StudentId { get; }

    public string TermId { get; }

    public string UniversityId { get; }

    public string ProgramCode { get; }

    public string Cohort { get; }

    public decimal CurrentGpa { get; }

    public decimal EarnedCredits { get; }

    public string Standing { get; }

    public TranscriptSummaryDto TranscriptSummary { get; }

    public Page<TranscriptAttemptDto> TranscriptAttempts { get; }

    public IReadOnlyList<AdminAcademicHoldDto> ActiveHolds { get; }

    public string DataVersion { get; }

    public DateTime DataAsOfUtc { get; }

    public Page<AcademicProvenanceDto> Provenance { get; }

    public string StudentRowVersion { get; }

    public string StudentTermStateRowVersion { get; }
}

internal static class AcademicContractGuard
{
    private const int MaximumActiveHolds = 100;

    public static string Required(string? value, string parameterName, int maximumLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
        {
            throw new ArgumentException(
                $"A non-empty value no longer than {maximumLength} characters is required.",
                parameterName);
        }

        return normalized;
    }

    public static string? Optional(string? value, string parameterName, int maximumLength) =>
        value is null ? null : Required(value, parameterName, maximumLength);

    public static decimal NonNegative(decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value cannot be negative.");
        }

        return value;
    }

    public static decimal Gpa(decimal value, string parameterName)
    {
        if (value is < 0 or > 4)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "GPA must be between zero and four.");
        }

        return value;
    }

    public static DateTime Utc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC timestamp is required.", parameterName);
        }

        return value;
    }

    public static DateTime? OptionalUtc(DateTime? value, string parameterName) =>
        value is null ? null : Utc(value.Value, parameterName);

    public static IReadOnlyList<AcademicHoldDto> CompleteHolds(
        IReadOnlyList<AcademicHoldDto> holds,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(holds, parameterName);
        if (holds.Count > MaximumActiveHolds || holds.Any(hold => hold is null))
        {
            throw new ArgumentException(
                $"The complete active-hold set may contain at most {MaximumActiveHolds} items.",
                parameterName);
        }

        return Array.AsReadOnly(holds.ToArray());
    }

    public static IReadOnlyList<AdminAcademicHoldDto> CompleteAdminHolds(
        IReadOnlyList<AdminAcademicHoldDto> holds,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(holds, parameterName);
        if (holds.Count > MaximumActiveHolds || holds.Any(hold => hold is null))
        {
            throw new ArgumentException(
                $"The complete active-hold set may contain at most {MaximumActiveHolds} items.",
                parameterName);
        }

        return Array.AsReadOnly(holds.ToArray());
    }
}
