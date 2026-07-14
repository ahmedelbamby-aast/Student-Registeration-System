namespace StudentRegistration.Contracts.Identity;

public sealed record IdentityImportErrorDto(
    int? Row,
    string Code,
    string Message);

public sealed record IdentityImportBatchDto(
    Guid Id,
    string Source,
    string ContentHash,
    string State,
    string RowVersion,
    IReadOnlyList<IdentityImportErrorDto> Errors);

public sealed record IdentityUserSummaryDto(
    Guid Id,
    string DisplayName,
    string LoginIdentifier,
    bool Enabled,
    IReadOnlyList<string> Roles,
    string RowVersion);

public sealed record IdentityImportUserRequest(
    string ExternalReference,
    string Kind,
    string? UniversityId,
    string? UserName,
    string? StaffNumber,
    string DisplayName,
    IReadOnlyList<string> Roles);

public sealed record IdentityImportRequest(
    string Source,
    string ContentHash,
    string ClientRequestId,
    IReadOnlyList<IdentityImportUserRequest> Users);

public sealed record IdentityImportPublishRequest(
    string ExpectedRowVersion,
    string ClientRequestId);

public sealed record UserStatusRequest(
    bool Enabled,
    string ExpectedRowVersion,
    string Reason);

public sealed record UserRolesRequest(
    IReadOnlyList<string> Roles,
    string ExpectedRowVersion,
    string Reason);
