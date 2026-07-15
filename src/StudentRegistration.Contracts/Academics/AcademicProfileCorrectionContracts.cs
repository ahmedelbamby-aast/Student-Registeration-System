using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts.Academics;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SetGpaOperation), typeDiscriminator: "set-gpa")]
[JsonDerivedType(typeof(SetEarnedCreditsOperation), typeDiscriminator: "set-earned-credits")]
[JsonDerivedType(typeof(SetStandingOperation), typeDiscriminator: "set-standing")]
[JsonDerivedType(
    typeof(UpsertTranscriptAttemptOperation),
    typeDiscriminator: "upsert-transcript-attempt")]
[JsonDerivedType(typeof(UpsertHoldOperation), typeDiscriminator: "upsert-hold")]
[JsonDerivedType(typeof(RemoveHoldOperation), typeDiscriminator: "remove-hold")]
public abstract record AcademicProfileCorrectionOperation
{
    protected AcademicProfileCorrectionOperation(string sourceReference)
    {
        SourceReference = AcademicContractGuard.Required(
            sourceReference,
            nameof(sourceReference),
            200);
    }

    public string SourceReference { get; }
}

public sealed record SetGpaOperation : AcademicProfileCorrectionOperation
{
    public SetGpaOperation(decimal currentGpa, string sourceReference)
        : base(sourceReference)
    {
        CurrentGpa = AcademicContractGuard.Gpa(currentGpa, nameof(currentGpa));
    }

    public decimal CurrentGpa { get; }
}

public sealed record SetEarnedCreditsOperation : AcademicProfileCorrectionOperation
{
    public SetEarnedCreditsOperation(decimal earnedCredits, string sourceReference)
        : base(sourceReference)
    {
        EarnedCredits = AcademicContractGuard.NonNegative(
            earnedCredits,
            nameof(earnedCredits));
    }

    public decimal EarnedCredits { get; }
}

public sealed record SetStandingOperation : AcademicProfileCorrectionOperation
{
    public SetStandingOperation(string standingCode, string sourceReference)
        : base(sourceReference)
    {
        StandingCode = AcademicContractGuard.Required(
            standingCode,
            nameof(standingCode),
            100);
    }

    public string StandingCode { get; }
}

public sealed record UpsertTranscriptAttemptOperation : AcademicProfileCorrectionOperation
{
    private static readonly HashSet<string> AllowedStatuses = new(
        ["in-progress", "passed", "failed", "withdrawn"],
        StringComparer.Ordinal);

    public UpsertTranscriptAttemptOperation(
        string? supersedesAttemptId,
        string courseCode,
        string termCode,
        decimal credits,
        string? grade,
        string status,
        string sourceReference)
        : base(sourceReference)
    {
        SupersedesAttemptId = AcademicContractGuard.Optional(
            supersedesAttemptId,
            nameof(supersedesAttemptId),
            100);
        CourseCode = AcademicContractGuard.Required(
            courseCode,
            nameof(courseCode),
            50);
        TermCode = AcademicContractGuard.Required(termCode, nameof(termCode), 50);
        if (credits <= 0m)
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
    }

    public string? SupersedesAttemptId { get; }

    public string CourseCode { get; }

    public string TermCode { get; }

    public decimal Credits { get; }

    public string? Grade { get; }

    public string Status { get; }
}

public sealed record UpsertHoldOperation : AcademicProfileCorrectionOperation
{
    public UpsertHoldOperation(
        string? holdId,
        string code,
        string message,
        bool blocksRegistration,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string sourceReference)
        : base(sourceReference)
    {
        HoldId = AcademicContractGuard.Optional(holdId, nameof(holdId), 100);
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
    }

    public string? HoldId { get; }

    public string Code { get; }

    public string Message { get; }

    public bool BlocksRegistration { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }
}

public sealed record RemoveHoldOperation : AcademicProfileCorrectionOperation
{
    public RemoveHoldOperation(string holdId, string sourceReference)
        : base(sourceReference)
    {
        HoldId = AcademicContractGuard.Required(holdId, nameof(holdId), 100);
    }

    public string HoldId { get; }
}

public sealed record AcademicProfileCorrectionRequest
{
    public AcademicProfileCorrectionRequest(
        string termId,
        string expectedStudentRowVersion,
        string expectedStudentTermStateRowVersion,
        string reason,
        string source,
        IReadOnlyList<AcademicProfileCorrectionOperation> operations)
    {
        TermId = AcademicContractGuard.Required(termId, nameof(termId), 100);
        ExpectedStudentRowVersion = AcademicContractGuard.Required(
            expectedStudentRowVersion,
            nameof(expectedStudentRowVersion),
            128);
        ExpectedStudentTermStateRowVersion = AcademicContractGuard.Required(
            expectedStudentTermStateRowVersion,
            nameof(expectedStudentTermStateRowVersion),
            128);
        Reason = AcademicContractGuard.Required(reason, nameof(reason), 500);
        if (Reason.Length < 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "Reason must contain at least 10 characters.");
        }

        Source = AcademicContractGuard.Required(source, nameof(source), 200);
        ArgumentNullException.ThrowIfNull(operations);
        if (operations.Count is < 1 or > 20 || operations.Any(operation => operation is null))
        {
            throw new ArgumentException(
                "A profile correction requires between 1 and 20 operations.",
                nameof(operations));
        }

        Operations = Array.AsReadOnly(operations.ToArray());
    }

    public string TermId { get; }

    public string ExpectedStudentRowVersion { get; }

    public string ExpectedStudentTermStateRowVersion { get; }

    public string Reason { get; }

    public string Source { get; }

    public IReadOnlyList<AcademicProfileCorrectionOperation> Operations { get; }
}
