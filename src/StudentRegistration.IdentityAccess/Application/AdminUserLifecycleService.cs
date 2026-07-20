using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.IdentityAccess.Application;

public enum AdminUserLifecycleOutcome
{
    Succeeded,
    ValidationFailed,
    PageSizeInvalid,
    ImportInvalid,
    ImportNotValidated,
    NotFound,
    StaleVersion,
    FinalAdminRequired,
    IdempotencyKeyReused,
    ImportContentExists,
    StorageFailure
}

public sealed record AdminUserLifecycleResult<T>(
    AdminUserLifecycleOutcome Outcome,
    T? Value = null,
    string? CurrentVersion = null)
    where T : class;

/// <summary>
/// Validates and normalizes Identity Admin commands. The persistence port owns the
/// transaction, AdminSecurityGuard lock, final-Admin recheck, and atomic audit writes.
/// </summary>
public sealed class AdminUserLifecycleService
{
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;
    private const int MaximumImportRows = 500;
    private const int MaximumReturnedErrors = 100;
    private const string DefaultSort = "displayName,id";

    private static readonly HashSet<string> AssignableRoles =
        new(StringComparer.Ordinal)
        {
            "Admin",
            "Lecturer",
            "TeachingAssistant"
        };

    private static readonly HashSet<string> AllowedSorts =
        new(StringComparer.OrdinalIgnoreCase)
        {
            DefaultSort,
            "displayName desc,id",
            "loginIdentifier,id",
            "loginIdentifier desc,id",
            "id"
        };

    private readonly IAdminUserLifecycleStore _store;
    private readonly TimeProvider _timeProvider;

    public AdminUserLifecycleService(
        IAdminUserLifecycleStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<AdminUserLifecycleResult<Page<IdentityUserSummaryDto>>> ListUsersAsync(
        string? search = null,
        int page = 1,
        int pageSize = DefaultPageSize,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > MaximumPageSize)
        {
            return new(AdminUserLifecycleOutcome.PageSizeInvalid);
        }

        var normalizedSearch = NormalizeOptional(search, 200);
        if (search is not null && normalizedSearch is null && !string.IsNullOrWhiteSpace(search) ||
            normalizedSearch is { Length: > 200 })
        {
            return new(AdminUserLifecycleOutcome.ValidationFailed);
        }

        var normalizedSort = string.IsNullOrWhiteSpace(sort) ? DefaultSort : sort.Trim();
        if (!AllowedSorts.Contains(normalizedSort))
        {
            return new(AdminUserLifecycleOutcome.ValidationFailed);
        }

        normalizedSort = AllowedSorts.First(value =>
            string.Equals(value, normalizedSort, StringComparison.OrdinalIgnoreCase));
        var snapshot = await _store.ListUsersAsync(
            new AdminUserSearchCriteria(normalizedSearch, page, pageSize, normalizedSort),
            cancellationToken);
        if (snapshot.TotalCount < 0 || snapshot.Items.Count > pageSize)
        {
            return new(AdminUserLifecycleOutcome.StorageFailure);
        }

        var users = snapshot.Items.Select(ToDto).ToArray();
        return new(
            AdminUserLifecycleOutcome.Succeeded,
            new Page<IdentityUserSummaryDto>(
                users,
                page,
                pageSize,
                snapshot.TotalCount,
                normalizedSort));
    }

    public async Task<AdminUserLifecycleResult<IdentityImportBatchDto>> CreateImportAsync(
        Guid requestedByUserId,
        IdentityImportRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (requestedByUserId == Guid.Empty ||
            request is null ||
            !TryRequired(request.Source, 255, out var source) ||
            !TryRequired(request.ClientRequestId, 100, out var clientRequestId) ||
            !TryRequired(correlationId, 100, out var correlation) ||
            request.Users is null ||
            request.Users.Count is < 1 or > MaximumImportRows ||
            !TryNormalizeImportRows(request.Users, out var users))
        {
            return new(AdminUserLifecycleOutcome.ImportInvalid);
        }

        var normalizedRows = users
            .Select(ToContractRow)
            .ToArray();
        var sourceHash = ComputeCanonicalContentHash(normalizedRows);
        if (!FixedTimeHashEquals(request.ContentHash, sourceHash))
        {
            return new(AdminUserLifecycleOutcome.ImportInvalid);
        }

        var requestHash = HashText($"{source}\n{sourceHash}");
        var command = new CreateIdentityImport(
            requestedByUserId,
            clientRequestId,
            requestHash,
            source,
            sourceHash,
            users,
            UserReference(requestedByUserId),
            correlation,
            UtcNow());
        var result = await _store.CreateImportAsync(command, cancellationToken);
        return Map(result, ToDto);
    }

    public async Task<AdminUserLifecycleResult<IdentityImportBatchDto>> GetImportAsync(
        Guid requestedByUserId,
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        if (requestedByUserId == Guid.Empty || importId == Guid.Empty)
        {
            return new(AdminUserLifecycleOutcome.NotFound);
        }

        var import = await _store.GetImportAsync(
            requestedByUserId,
            importId,
            cancellationToken);
        return import is null
            ? new(AdminUserLifecycleOutcome.NotFound)
            : new(AdminUserLifecycleOutcome.Succeeded, ToDto(import));
    }

    public async Task<AdminUserLifecycleResult<IdentityImportBatchDto>> PublishImportAsync(
        Guid requestedByUserId,
        Guid importId,
        IdentityImportPublishRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (requestedByUserId == Guid.Empty ||
            importId == Guid.Empty ||
            request is null ||
            !TryRequired(request.ClientRequestId, 100, out var clientRequestId) ||
            !TryDecodeVersion(request.ExpectedRowVersion, out var expectedVersion) ||
            !TryRequired(correlationId, 100, out var correlation))
        {
            return new(AdminUserLifecycleOutcome.ValidationFailed);
        }

        var requestHash = HashText(
            $"{importId:N}\n{Convert.ToBase64String(expectedVersion)}");
        var result = await _store.PublishImportAsync(
            new PublishIdentityImport(
                requestedByUserId,
                importId,
                clientRequestId,
                requestHash,
                expectedVersion,
                UserReference(requestedByUserId),
                correlation,
                UtcNow()),
            cancellationToken);
        return Map(result, ToDto);
    }

    public async Task<AdminUserLifecycleResult<IdentityUserSummaryDto>> ChangeStatusAsync(
        Guid actorUserId,
        Guid userId,
        UserStatusRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty ||
            userId == Guid.Empty ||
            request is null ||
            !TryDecodeVersion(request.ExpectedRowVersion, out var expectedVersion) ||
            !TryRequired(request.Reason, 1000, out var reason) ||
            !TryRequired(correlationId, 100, out var correlation))
        {
            return new(AdminUserLifecycleOutcome.ValidationFailed);
        }

        var result = await _store.SetUserStatusAsync(
            new SetUserStatus(
                actorUserId,
                userId,
                request.Enabled,
                expectedVersion,
                reason,
                CreateSecurityStamp(),
                UserReference(actorUserId),
                UserReference(userId),
                correlation,
                UtcNow()),
            cancellationToken);
        return Map(result, ToDto);
    }

    public async Task<AdminUserLifecycleResult<IdentityUserSummaryDto>> ReplaceRolesAsync(
        Guid actorUserId,
        Guid userId,
        UserRolesRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty ||
            userId == Guid.Empty ||
            request is null ||
            request.Roles is null ||
            !TryNormalizeRoles(request.Roles, out var roles) ||
            !TryDecodeVersion(request.ExpectedRowVersion, out var expectedVersion) ||
            !TryRequired(request.Reason, 1000, out var reason) ||
            !TryRequired(correlationId, 100, out var correlation))
        {
            return new(AdminUserLifecycleOutcome.ValidationFailed);
        }

        var result = await _store.ReplaceUserRolesAsync(
            new ReplaceUserRoles(
                actorUserId,
                userId,
                roles,
                expectedVersion,
                reason,
                CreateSecurityStamp(),
                UserReference(actorUserId),
                UserReference(userId),
                correlation,
                UtcNow()),
            cancellationToken);
        return Map(result, ToDto);
    }

    public static string ComputeCanonicalContentHash(
        IReadOnlyList<IdentityImportUserRequest> users)
    {
        ArgumentNullException.ThrowIfNull(users);

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartArray();
            foreach (var row in users)
            {
                ArgumentNullException.ThrowIfNull(row);
                writer.WriteStartObject();
                writer.WriteString("externalReference", row.ExternalReference?.Trim());
                writer.WriteString("kind", row.Kind?.Trim().ToLowerInvariant());
                WriteOptional(writer, "universityId", row.UniversityId);
                WriteOptional(writer, "userName", row.UserName);
                WriteOptional(writer, "staffNumber", row.StaffNumber);
                writer.WriteString("displayName", row.DisplayName?.Trim());
                writer.WriteStartArray("roles");
                foreach (var role in (row.Roles ?? [])
                    .Where(role => !string.IsNullOrWhiteSpace(role))
                    .Select(role => role.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal))
                {
                    writer.WriteStringValue(role);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        return $"SHA256:{Convert.ToHexString(SHA256.HashData(buffer.ToArray()))}";
    }

    private static AdminUserLifecycleResult<TDto> Map<TSnapshot, TDto>(
        AdminStoreResult<TSnapshot> result,
        Func<TSnapshot, TDto> projector)
        where TSnapshot : class
        where TDto : class
    {
        if (result.Outcome == AdminStoreOutcome.Succeeded && result.Value is not null)
        {
            return new(AdminUserLifecycleOutcome.Succeeded, projector(result.Value));
        }

        var outcome = result.Outcome switch
        {
            AdminStoreOutcome.NotFound => AdminUserLifecycleOutcome.NotFound,
            AdminStoreOutcome.StaleVersion => AdminUserLifecycleOutcome.StaleVersion,
            AdminStoreOutcome.FinalAdminRequired =>
                AdminUserLifecycleOutcome.FinalAdminRequired,
            AdminStoreOutcome.IdempotencyKeyReused =>
                AdminUserLifecycleOutcome.IdempotencyKeyReused,
            AdminStoreOutcome.ImportContentExists =>
                AdminUserLifecycleOutcome.ImportContentExists,
            AdminStoreOutcome.ImportNotValidated =>
                AdminUserLifecycleOutcome.ImportNotValidated,
            _ => AdminUserLifecycleOutcome.StorageFailure
        };
        return new(
            outcome,
            null,
            result.CurrentVersion is { Length: > 0 }
                ? Convert.ToBase64String(result.CurrentVersion)
                : null);
    }

    private static IdentityUserSummaryDto ToDto(AdminUserSnapshot user) =>
        new(
            user.Id,
            BoundedText(user.DisplayName, 200, "Unknown user"),
            BoundedText(user.LoginIdentifier, 200, "Unavailable"),
            user.Enabled,
            user.Roles
                .Where(role => role is "Student" or "Admin" or "Lecturer" or "TeachingAssistant")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            EncodeVersion(user.Version));

    private static IdentityImportBatchDto ToDto(IdentityImportSnapshot import) =>
        new(
            import.Id,
            BoundedText(import.SourceName, 255, "import"),
            BoundedText(import.SourceHash, 200, "unavailable"),
            import.State is "uploaded" or "invalid" or "validated" or "published" or "failed"
                ? import.State
                : "failed",
            EncodeVersion(import.Version),
            import.Errors
                .Take(MaximumReturnedErrors)
                .Select(error => new IdentityImportErrorDto(
                    error.Row is > 0 ? error.Row : null,
                    BoundedText(error.Code, 100, "IMPORT_INVALID"),
                    BoundedText(error.Message, 500, "The row could not be validated.")))
                .ToArray());

    private static IdentityImportUserRequest ToContractRow(IdentityImportCandidate row) =>
        new(
            row.ExternalReference,
            row.Kind,
            row.UniversityId,
            row.UserName,
            row.StaffNumber,
            row.DisplayName,
            row.Kind == "student" ? [] : row.Roles);

    private static bool TryNormalizeImportRows(
        IReadOnlyList<IdentityImportUserRequest> rows,
        out IReadOnlyList<IdentityImportCandidate> normalized)
    {
        var candidates = new List<IdentityImportCandidate>(rows.Count);
        var externalReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var universityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var staffNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (row is null ||
                !TryRequired(row.ExternalReference, 100, out var externalReference) ||
                !externalReferences.Add(externalReference) ||
                !TryRequired(row.Kind, 20, out var kind) ||
                !TryRequired(row.DisplayName, 200, out var displayName) ||
                row.Roles is null ||
                !TryNormalizeRoles(row.Roles, out var roles))
            {
                normalized = [];
                return false;
            }

            kind = kind.ToLowerInvariant();
            if (kind == "student")
            {
                if (!TryRequired(row.UniversityId, 50, out var universityId) ||
                    !universityIds.Add(universityId) ||
                    !string.IsNullOrWhiteSpace(row.UserName) ||
                    !string.IsNullOrWhiteSpace(row.StaffNumber) ||
                    roles.Count != 0)
                {
                    normalized = [];
                    return false;
                }

                candidates.Add(new IdentityImportCandidate(
                    externalReference,
                    kind,
                    universityId.ToUpperInvariant(),
                    null,
                    null,
                    displayName,
                    ["Student"]));
                continue;
            }

            if (kind == "staff")
            {
                if (!string.IsNullOrWhiteSpace(row.UniversityId) ||
                    !TryRequired(row.UserName, 200, out var userName) ||
                    !TryRequired(row.StaffNumber, 50, out var staffNumber) ||
                    roles.Count == 0 ||
                    !userNames.Add(userName) ||
                    !staffNumbers.Add(staffNumber))
                {
                    normalized = [];
                    return false;
                }

                candidates.Add(new IdentityImportCandidate(
                    externalReference,
                    kind,
                    null,
                    userName,
                    staffNumber,
                    displayName,
                    roles));
                continue;
            }

            normalized = [];
            return false;
        }

        normalized = candidates;
        return true;
    }

    private static bool TryNormalizeRoles(
        IReadOnlyList<string> requestedRoles,
        out IReadOnlyList<string> roles)
    {
        if (requestedRoles.Count > AssignableRoles.Count)
        {
            roles = [];
            return false;
        }

        var normalized = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in requestedRoles)
        {
            if (!TryRequired(role, 50, out var value) || !AssignableRoles.Contains(value))
            {
                roles = [];
                return false;
            }

            normalized.Add(value);
        }

        if (normalized.Count > 1)
        {
            roles = [];
            return false;
        }

        roles = normalized.Order(StringComparer.Ordinal).ToArray();
        return true;
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private static string CreateSecurityStamp() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string UserReference(Guid userId) => $"user:{userId:N}";

    private static string EncodeVersion(byte[] version) =>
        version is { Length: > 0 }
            ? Convert.ToBase64String(version)
            : throw new InvalidOperationException("A row version is required.");

    private static bool TryDecodeVersion(string? value, out byte[] version)
    {
        version = [];
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            return false;
        }

        try
        {
            version = Convert.FromBase64String(value);
            return version.Length is > 0 and <= 64;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryRequired(string? value, int maximumLength, out string normalized)
    {
        normalized = value?.Trim() ?? string.Empty;
        return normalized.Length is > 0 && normalized.Length <= maximumLength;
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maximumLength ? normalized : new string('x', maximumLength + 1);
    }

    private static bool FixedTimeHashEquals(string? supplied, string expected)
    {
        if (string.IsNullOrWhiteSpace(supplied))
        {
            return false;
        }

        var left = Encoding.ASCII.GetBytes(supplied.Trim().ToUpperInvariant());
        var right = Encoding.ASCII.GetBytes(expected.ToUpperInvariant());
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private static string HashText(string value) =>
        $"SHA256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))}";

    private static string BoundedText(string? value, int maximumLength, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim();
        return normalized.Length <= maximumLength
            ? normalized
            : normalized[..maximumLength];
    }

    private static void WriteOptional(Utf8JsonWriter writer, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            writer.WriteNull(name);
        }
        else
        {
            writer.WriteString(name, value.Trim());
        }
    }
}
