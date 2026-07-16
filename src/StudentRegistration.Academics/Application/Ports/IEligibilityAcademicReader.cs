namespace StudentRegistration.Academics.Application.Ports;

public interface IEligibilityAcademicReader
{
    Task<EligibilityAcademicSnapshot?> ReadAsync(
        Guid applicationUserId,
        Guid termId,
        IReadOnlyCollection<Guid> courseIds,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default);
}

public sealed record EligibilityAcademicSnapshot(
    Guid StudentId,
    Guid TermId,
    string ProgramCode,
    string Cohort,
    decimal Gpa,
    decimal EarnedCredits,
    string Standing,
    byte[] AcademicContextVersion,
    EligibilityRegistrationWindowSnapshot RegistrationWindow,
    IReadOnlyList<EligibilityHoldSnapshot> ActiveHolds,
    IReadOnlyList<EligibilityTranscriptSnapshot> CurrentTranscriptLeaves,
    EligibilityCatalogueSnapshot? Catalogue,
    EligibilityPolicySnapshot? Policy);

public sealed record EligibilityRegistrationWindowSnapshot(
    bool IsOpen,
    byte[] RowVersion);

public sealed record EligibilityHoldSnapshot(
    string Code,
    string Message,
    bool BlocksRegistration,
    string SourceReference);

public sealed record EligibilityTranscriptSnapshot(
    string CourseCode,
    string Status,
    bool IsCurrentLeaf);

public sealed record EligibilityCatalogueSnapshot(
    Guid CatalogueVersionId,
    string VersionCode,
    byte[] RowVersion,
    IReadOnlyList<EligibilityCourseSnapshot> Courses);

public sealed record EligibilityCourseSnapshot(
    Guid CourseId,
    string Code,
    string Title,
    decimal Credits,
    IReadOnlyList<string> PrerequisiteCourseCodes,
    decimal? MinimumGpa,
    decimal? MinimumEarnedCredits,
    string SourceReference,
    DateOnly SourceAccessedOn);

public sealed record EligibilityPolicySnapshot(
    Guid PolicySetId,
    string Version,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string ApprovedBy,
    string ApprovalReference,
    byte[] RowVersion,
    IReadOnlyList<EligibilityPolicyRuleSnapshot> Rules);

public sealed record EligibilityPolicyRuleSnapshot(
    string TypeKey,
    string ReasonCode,
    string SourceReference,
    DateOnly SourceAccessedOn,
    string Value);
