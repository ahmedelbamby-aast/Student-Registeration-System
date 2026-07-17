using StudentRegistration.Contracts.Auditing;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.StaffAdministration.Application.Ports;

public sealed record AdminExportCreateCommand(
    ExportJob Job,
    AuditEventDraft RequestAudit);

public enum AdminExportCreateOutcome
{
    Created = 1,
    Replay = 2,
    IdempotencyKeyReused = 3,
}

public sealed record AdminExportCreateResult(
    AdminExportCreateOutcome Outcome,
    ExportJob Job);

public interface IAdminExportStore
{
    Task<AdminExportCreateResult> CreateOrReplayAsync(
        AdminExportCreateCommand command,
        CancellationToken cancellationToken = default);

    Task<ExportJob?> ReadAuthorizedAsync(
        Guid jobId,
        Guid actorUserId,
        string scopeHash,
        bool canReadAll,
        CancellationToken cancellationToken = default);

    Task<ExportJob?> TryClaimAsync(
        Guid jobId,
        string leaseOwnerId,
        DateTime claimedAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> RenewLeaseAsync(
        Guid jobId,
        string leaseOwnerId,
        DateTime renewedAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> PublishAsync(
        Guid jobId,
        string leaseOwnerId,
        Guid artifactId,
        DateTime completedAtUtc,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> RecordFailureAsync(
        Guid jobId,
        string leaseOwnerId,
        DateTime failedAtUtc,
        bool retryable,
        string failureCode,
        CancellationToken cancellationToken = default);

    Task<bool> RecordDownloadAsync(
        Guid jobId,
        Guid actorUserId,
        string scopeHash,
        bool canReadAll,
        AuditEventDraft downloadAudit,
        DateTime observedAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> ExpireAsync(
        Guid jobId,
        Guid artifactId,
        DateTime observedAtUtc,
        CancellationToken cancellationToken = default);
}
