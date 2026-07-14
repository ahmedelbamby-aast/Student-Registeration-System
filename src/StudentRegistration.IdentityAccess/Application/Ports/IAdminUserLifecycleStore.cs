namespace StudentRegistration.IdentityAccess.Application.Ports;

/// <summary>
/// Persistence boundary for governed identity administration.
/// Implementations perform filtering/paging server-side and own each transaction.
/// Import publication is all-or-nothing. Same-key/same-payload retries replay the
/// committed result, while a reused key with another payload is rejected.
/// Reducing status and role writes lock AdminSecurityGuard and recheck the enabled
/// Admin invariant. A successful mutation rotates shared security state and appends
/// SecurityEvent and AuditEvent in the same transaction. If the recheck would remove
/// the final enabled Admin, the adapter returns FINAL_ADMIN_REQUIRED without changes.
/// </summary>
public interface IAdminUserLifecycleStore
{
    Task<AdminUserPageSnapshot> ListUsersAsync(
        AdminUserSearchCriteria criteria,
        CancellationToken cancellationToken);

    Task<AdminStoreResult<IdentityImportSnapshot>> CreateImportAsync(
        CreateIdentityImport command,
        CancellationToken cancellationToken);

    Task<IdentityImportSnapshot?> GetImportAsync(
        Guid requestedByUserId,
        Guid importId,
        CancellationToken cancellationToken);

    Task<AdminStoreResult<IdentityImportSnapshot>> PublishImportAsync(
        PublishIdentityImport command,
        CancellationToken cancellationToken);

    Task<AdminStoreResult<AdminUserSnapshot>> SetUserStatusAsync(
        SetUserStatus command,
        CancellationToken cancellationToken);

    Task<AdminStoreResult<AdminUserSnapshot>> ReplaceUserRolesAsync(
        ReplaceUserRoles command,
        CancellationToken cancellationToken);
}

public enum AdminStoreOutcome
{
    Succeeded,
    NotFound,
    StaleVersion,
    FinalAdminRequired,
    IdempotencyKeyReused,
    ImportContentExists,
    ImportNotValidated,
    StorageFailure
}

public sealed record AdminStoreResult<T>(
    AdminStoreOutcome Outcome,
    T? Value,
    byte[]? CurrentVersion = null)
    where T : class;

public sealed record AdminUserSearchCriteria(
    string? Search,
    int Page,
    int PageSize,
    string Sort);

public sealed record AdminUserPageSnapshot(
    IReadOnlyList<AdminUserSnapshot> Items,
    int TotalCount);

public sealed record AdminUserSnapshot(
    Guid Id,
    string DisplayName,
    string LoginIdentifier,
    bool Enabled,
    IReadOnlyList<string> Roles,
    byte[] Version);

public sealed record IdentityImportError(
    int? Row,
    string Code,
    string Message);

public sealed record IdentityImportSnapshot(
    Guid Id,
    string SourceName,
    string SourceHash,
    string State,
    IReadOnlyList<IdentityImportError> Errors,
    byte[] Version);

public sealed record IdentityImportCandidate(
    string ExternalReference,
    string Kind,
    string? UniversityId,
    string? UserName,
    string? StaffNumber,
    string DisplayName,
    IReadOnlyList<string> Roles);

public sealed record CreateIdentityImport(
    Guid RequestedByUserId,
    string ClientRequestId,
    string RequestHash,
    string SourceName,
    string SourceHash,
    IReadOnlyList<IdentityImportCandidate> Users,
    string ActorReference,
    string CorrelationId,
    DateTime RequestedAtUtc);

public sealed record PublishIdentityImport(
    Guid RequestedByUserId,
    Guid ImportId,
    string ClientRequestId,
    string RequestHash,
    byte[] ExpectedVersion,
    string ActorReference,
    string CorrelationId,
    DateTime RequestedAtUtc);

public sealed record SetUserStatus(
    Guid ActorUserId,
    Guid UserId,
    bool Enabled,
    byte[] ExpectedVersion,
    string Reason,
    string NewSecurityStamp,
    string ActorReference,
    string SubjectReference,
    string CorrelationId,
    DateTime RequestedAtUtc);

public sealed record ReplaceUserRoles(
    Guid ActorUserId,
    Guid UserId,
    IReadOnlyList<string> Roles,
    byte[] ExpectedVersion,
    string Reason,
    string NewSecurityStamp,
    string ActorReference,
    string SubjectReference,
    string CorrelationId,
    DateTime RequestedAtUtc);
