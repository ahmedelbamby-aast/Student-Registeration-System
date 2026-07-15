using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Contracts.Auditing;

namespace StudentRegistration.Academics.Application.Ports;

public sealed record AcademicProfileReadRequest(
    int TranscriptPage,
    int TranscriptPageSize,
    int ProvenancePage,
    int ProvenancePageSize,
    int ActiveHoldTake,
    DateTime ServerNowUtc);

public sealed record AcademicProfileSnapshot(
    string UniversityId,
    Student Student,
    StudentTermAcademicState StudentTermState,
    byte[] StudentRowVersion,
    byte[] StudentTermStateRowVersion,
    TranscriptSummaryDto TranscriptSummary,
    Page<TranscriptAttemptDto> TranscriptAttempts,
    IReadOnlyList<StudentHold> ActiveHolds,
    Page<AcademicProvenanceDto> Provenance);

public enum AcademicProfileStoreOutcome
{
    Succeeded,
    NotFound,
    StaleVersion,
    InvalidSupersession,
    ProfileNotReady,
    HoldBlocked,
    StorageUnavailable
}

public sealed record AcademicProfileStoreResult(
    AcademicProfileStoreOutcome Outcome,
    AcademicProfileSnapshot? Profile = null,
    string? CurrentVersion = null);

public abstract record AcademicProfileMutation(string SourceReference);

public sealed record SetGpaMutation(
    decimal CurrentGpa,
    string SourceReference)
    : AcademicProfileMutation(SourceReference);

public sealed record SetEarnedCreditsMutation(
    decimal EarnedCredits,
    string SourceReference)
    : AcademicProfileMutation(SourceReference);

public sealed record SetStandingMutation(
    string Standing,
    string SourceReference)
    : AcademicProfileMutation(SourceReference);

public sealed record AppendTranscriptAttemptMutation(
    Guid AttemptId,
    Guid? SupersedesAttemptId,
    string CourseCode,
    string TermCode,
    decimal Credits,
    string? Grade,
    TranscriptAttemptStatus Status,
    string SourceReference)
    : AcademicProfileMutation(SourceReference);

public sealed record UpsertStudentHoldMutation(
    Guid? HoldId,
    string Code,
    string Message,
    bool BlocksRegistration,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string SourceReference)
    : AcademicProfileMutation(SourceReference);

public sealed record RemoveStudentHoldMutation(
    Guid HoldId,
    string SourceReference)
    : AcademicProfileMutation(SourceReference);

public sealed record CorrectAcademicProfileStoreCommand(
    Guid StudentId,
    Guid TermId,
    byte[] ExpectedStudentRowVersion,
    byte[] ExpectedStudentTermStateRowVersion,
    string Source,
    IReadOnlyList<AcademicProfileMutation> Mutations,
    AcademicProfileReadRequest ResponseRead,
    AuditEventDraft AuditEvent);

public sealed record RegistrationBoundaryStoreCommand(
    Guid StudentId,
    Guid TermId,
    byte[] ExpectedStudentTermStateRowVersion,
    DateTime ServerReceivedAtUtc);

public interface IStudentAcademicProfileStore
{
    Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
        Guid applicationUserId,
        AcademicProfileReadRequest request,
        CancellationToken cancellationToken = default);

    Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
        Guid studentId,
        Guid termId,
        AcademicProfileReadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks the StudentTermAcademicState, validates both expected versions,
    /// appends transcript successors, applies the other mutations, and writes
    /// the audit event in one transaction.
    /// </summary>
    Task<AcademicProfileStoreResult> CorrectAsync(
        CorrectAcademicProfileStoreCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks and revalidates the student/term boundary. The callback may run
    /// only inside the successful serial transaction and never for a blocked
    /// hold, stale version, missing profile, or unavailable store.
    /// </summary>
    Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
        RegistrationBoundaryStoreCommand command,
        Func<CancellationToken, Task> commitCallback,
        CancellationToken cancellationToken = default);
}

public sealed record DemoStudentProfileSeedCommand(
    string SeedProfileVersion,
    int FixtureOrdinal,
    string UniversityId,
    Student Student,
    IReadOnlyList<TranscriptAttempt> TranscriptAttempts,
    IReadOnlyList<StudentHold> Holds,
    StudentTermAcademicState StudentTermAcademicState,
    IReadOnlyList<AcademicProvenanceDto> Provenance);

public sealed record DemoStudentProfileSeedResult(
    Guid StudentId,
    bool Created);

public interface IDemoStudentProfileSeedStore
{
    Task<DemoStudentProfileSeedResult> ReconcileAsync(
        DemoStudentProfileSeedCommand command,
        CancellationToken cancellationToken = default);
}
