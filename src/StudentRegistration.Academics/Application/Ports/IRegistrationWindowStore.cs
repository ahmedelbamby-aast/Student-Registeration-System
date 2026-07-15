using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Contracts.Auditing;

namespace StudentRegistration.Academics.Application.Ports;

public sealed record CreateAcademicTermStoreCommand(
    AcademicTerm Term,
    IReadOnlyList<RegistrationWindow> Windows,
    AuditEventDraft AuditEvent);

public enum AcademicTermCreationOutcome
{
    Created,
    Replayed,
    IdempotencyKeyReused,
    TermCodeExists,
    TermStateConflict,
    StorageUnavailable
}

public sealed record AcademicTermCreationStoreResult(
    AcademicTermCreationOutcome Outcome,
    AdminTermDto? Term = null);

public sealed record UpdateAcademicTermStoreCommand(
    Guid TermId,
    byte[] ExpectedTermRowVersion,
    IReadOnlyDictionary<Guid, byte[]> ExpectedWindowRowVersions,
    TermInput Term,
    IReadOnlyList<TermWindowInput> Windows,
    AuditEventDraft AuditEvent);

public sealed record PublishRegistrationWindowStoreCommand(
    Guid TermId,
    Guid WindowId,
    byte[] ExpectedTermRowVersion,
    byte[] ExpectedWindowRowVersion,
    AuditEventDraft AuditEvent);

public enum AcademicTermMutationOutcome
{
    Succeeded,
    NotFound,
    StaleVersion,
    WindowOverlap,
    TermCodeExists,
    TermStateConflict,
    StorageUnavailable
}

public sealed record AcademicTermMutationStoreResult(
    AcademicTermMutationOutcome Outcome,
    AdminTermDto? Term = null,
    string? CurrentVersion = null);

public interface IRegistrationWindowStore
{
    /// <summary>
    /// Atomically inserts the term or resolves its globally unique
    /// CreationClientRequestId. An identical CreationPayloadHash returns the
    /// original aggregate; a different hash returns IdempotencyKeyReused.
    /// The audit draft commits only with a newly created aggregate.
    /// </summary>
    Task<AcademicTermCreationStoreResult> CreateOrReplayTermAsync(
        CreateAcademicTermStoreCommand command,
        CancellationToken cancellationToken = default);

    Task<AcademicTermMutationStoreResult> UpdateTermAsync(
        UpdateAcademicTermStoreCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks the AcademicTerm first, then queries and locks every candidate
    /// and existing RegistrationWindow using OrderBy(window => window.Id)
    /// before rechecking Published half-open intervals and committing audit.
    /// </summary>
    Task<AcademicTermMutationStoreResult> PublishRegistrationWindowAsync(
        PublishRegistrationWindowStoreCommand command,
        CancellationToken cancellationToken = default);
}
