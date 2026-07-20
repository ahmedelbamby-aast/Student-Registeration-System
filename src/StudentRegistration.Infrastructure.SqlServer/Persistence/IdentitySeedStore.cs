using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

/// <summary>
/// Adds each stable synthetic identity exactly once. Existing identities are
/// validated but never have their issued or user-selected credential rotated.
/// </summary>
public sealed class IdentitySeedStore : IIdentitySeedStore
{
    private const int QueryBatchSize = 500;
    private static readonly HashSet<string> AllowedRoles = new(
        ["Student", "Admin", "Lecturer", "TeachingAssistant"],
        StringComparer.Ordinal);

    private readonly StudentRegistrationDbContext _dbContext;

    public IdentitySeedStore(StudentRegistrationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<IReadOnlySet<Guid>> ReconcileAsync(
        IReadOnlyList<DemoSeedIdentity> identities,
        IReadOnlySet<Guid> retiredUserIds,
        DateTime retiredAtUtc,
        string clientRequestId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identities);
        ArgumentNullException.ThrowIfNull(retiredUserIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientRequestId);
        if (clientRequestId.Length > 100
            || retiredAtUtc.Kind != DateTimeKind.Utc
            || retiredUserIds.Contains(Guid.Empty)
            || identities.Any(identity => retiredUserIds.Contains(identity.UserId)))
        {
            throw new ArgumentException(
                "The synthetic identity reconciliation request is invalid.");
        }

        ValidateInput(identities);
        return ExecuteAtomicallyAsync(
            () => ReconcileCoreAsync(
                identities,
                retiredUserIds,
                retiredAtUtc,
                clientRequestId,
                cancellationToken),
            cancellationToken);
    }

    private async Task<IReadOnlySet<Guid>> ReconcileCoreAsync(
        IReadOnlyList<DemoSeedIdentity> identities,
        IReadOnlySet<Guid> retiredUserIds,
        DateTime retiredAtUtc,
        string clientRequestId,
        CancellationToken cancellationToken)
    {
        var existingUsers = await LoadUsersAsync(
                identities.Select(identity => identity.UserId),
                cancellationToken)
            .ConfigureAwait(false);
        var existingById = existingUsers.ToDictionary(user => user.Id);
        var existingIds = existingById.Keys.ToHashSet();

        await ValidateExistingRowsAsync(
                identities.Where(identity => existingIds.Contains(identity.UserId)).ToArray(),
                existingById,
                cancellationToken)
            .ConfigureAwait(false);

        var retiredStateChanged = await RetireExistingUsersAsync(
                retiredUserIds,
                retiredAtUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var insertedIds = new HashSet<Guid>();
        foreach (var identity in identities)
        {
            if (existingIds.Contains(identity.UserId))
            {
                continue;
            }

            var user = new ApplicationUser(
                identity.UserId,
                identity.UserName,
                identity.NormalizedUserName,
                identity.UniversityId,
                identity.PasswordHash,
                identity.SecurityStamp);
            _dbContext.Add(user);

            if (identity.StaffNumber is null)
            {
                _dbContext.Add(new StudentActivation(
                    StableGuid(identity.UserId, "student-activation"),
                    identity.UserId,
                    identity.ProvisionedAtUtc));
            }
            else
            {
                _dbContext.Add(new Staff(
                    StableGuid(identity.UserId, "staff"),
                    identity.UserId,
                    identity.StaffNumber,
                    identity.DisplayName));
            }

            foreach (var role in identity.Roles.Distinct(StringComparer.Ordinal))
            {
                _dbContext.Add(new RoleAssignment(
                    StableGuid(identity.UserId, $"role:{role}"),
                    identity.UserId,
                    role,
                    identity.ProvisionedAtUtc,
                    effectiveToUtc: null,
                    $"demo-seed:{clientRequestId}"));
            }

            insertedIds.Add(identity.UserId);
        }

        if (insertedIds.Count > 0 || retiredStateChanged)
        {
            await _dbContext.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
            _dbContext.ChangeTracker.Clear();
        }

        return insertedIds;
    }

    private async Task<bool> RetireExistingUsersAsync(
        IReadOnlySet<Guid> retiredUserIds,
        DateTime retiredAtUtc,
        CancellationToken cancellationToken)
    {
        if (retiredUserIds.Count == 0)
        {
            return false;
        }

        var users = new List<ApplicationUser>();
        var roles = new List<RoleAssignment>();
        foreach (var batch in retiredUserIds.Chunk(QueryBatchSize))
        {
            users.AddRange(await _dbContext.Set<ApplicationUser>()
                .Where(user => batch.Contains(user.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false));
            roles.AddRange(await _dbContext.Set<RoleAssignment>()
                .Where(role => batch.Contains(role.ApplicationUserId)
                    && role.EffectiveFromUtc < retiredAtUtc
                    && (role.EffectiveToUtc == null || retiredAtUtc < role.EffectiveToUtc))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false));
        }

        var changed = false;
        foreach (var user in users.Where(user => user.IsEnabled))
        {
            user.SetEnabled(
                false,
                Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
            changed = true;
        }

        foreach (var role in roles)
        {
            role.EndAt(retiredAtUtc);
            changed = true;
        }

        return changed;
    }

    private async Task ValidateExistingRowsAsync(
        IReadOnlyList<DemoSeedIdentity> identities,
        IReadOnlyDictionary<Guid, ApplicationUser> existingById,
        CancellationToken cancellationToken)
    {
        if (identities.Count == 0)
        {
            return;
        }

        var ids = identities.Select(identity => identity.UserId).ToArray();
        var staffRows = await LoadAsync<Staff>(
                ids,
                (query, batch) => query.Where(
                    staff => batch.Contains(staff.ApplicationUserId)),
                cancellationToken)
            .ConfigureAwait(false);
        var activationRows = await LoadAsync<StudentActivation>(
                ids,
                (query, batch) => query.Where(
                    activation => batch.Contains(activation.ApplicationUserId)),
                cancellationToken)
            .ConfigureAwait(false);
        var roleRows = await LoadAsync<RoleAssignment>(
                ids,
                (query, batch) => query.Where(
                    role => batch.Contains(role.ApplicationUserId)),
                cancellationToken)
            .ConfigureAwait(false);

        var staffByUser = staffRows.ToDictionary(staff => staff.ApplicationUserId);
        var activationUsers = activationRows
            .Select(activation => activation.ApplicationUserId)
            .ToHashSet();
        var rolesByUser = roleRows
            .GroupBy(role => role.ApplicationUserId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(role => role.RoleCode).ToHashSet(StringComparer.Ordinal));

        foreach (var identity in identities)
        {
            var existing = existingById[identity.UserId];
            var sameStableIdentity =
                string.Equals(existing.UserName, identity.UserName, StringComparison.Ordinal)
                && string.Equals(
                    existing.NormalizedUserName,
                    identity.NormalizedUserName,
                    StringComparison.Ordinal)
                && string.Equals(
                    existing.UniversityId,
                    identity.UniversityId,
                    StringComparison.Ordinal);
            var expectedProfileExists = identity.StaffNumber is null
                ? activationUsers.Contains(identity.UserId)
                : staffByUser.TryGetValue(identity.UserId, out var staff)
                    && string.Equals(
                        staff.StaffNumber,
                        identity.StaffNumber,
                        StringComparison.Ordinal);
            var expectedRolesExist = rolesByUser.TryGetValue(identity.UserId, out var roles)
                && identity.Roles.All(roles.Contains);

            if (!sameStableIdentity || !expectedProfileExists || !expectedRolesExist)
            {
                throw new InvalidOperationException(
                    "IDENTITY_SEED_STATE_MISMATCH: Existing synthetic identity state does not match the stable fixture.");
            }
        }
    }

    private async Task<List<ApplicationUser>> LoadUsersAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var result = new List<ApplicationUser>();
        foreach (var batch in userIds.Chunk(QueryBatchSize))
        {
            result.AddRange(await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(user => batch.Contains(user.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false));
        }

        return result;
    }

    private async Task<List<TEntity>> LoadAsync<TEntity>(
        IReadOnlyCollection<Guid> userIds,
        Func<IQueryable<TEntity>, Guid[], IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var result = new List<TEntity>();
        foreach (var batch in userIds.Chunk(QueryBatchSize))
        {
            var query = filter(_dbContext.Set<TEntity>().AsNoTracking(), batch);
            result.AddRange(await query.ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        return result;
    }

    private static void ValidateInput(IReadOnlyList<DemoSeedIdentity> identities)
    {
        if (identities.Count == 0)
        {
            throw new ArgumentException("At least one synthetic identity is required.", nameof(identities));
        }

        EnsureDistinct(identities.Select(identity => identity.UserId), "user identifier");
        EnsureDistinct(
            identities.Select(identity => identity.NormalizedUserName),
            "normalized username");
        EnsureDistinct(
            identities.Where(identity => identity.UniversityId is not null)
                .Select(identity => identity.UniversityId!),
            "University ID");
        EnsureDistinct(
            identities.Where(identity => identity.StaffNumber is not null)
                .Select(identity => identity.StaffNumber!),
            "staff number");

        foreach (var identity in identities)
        {
            if (identity.UserId == Guid.Empty
                || string.IsNullOrWhiteSpace(identity.UserName)
                || string.IsNullOrWhiteSpace(identity.NormalizedUserName)
                || string.IsNullOrWhiteSpace(identity.PasswordHash)
                || string.IsNullOrWhiteSpace(identity.SecurityStamp)
                || string.IsNullOrWhiteSpace(identity.DisplayName)
                || identity.Roles.Count == 0
                || identity.Roles.Any(role => !AllowedRoles.Contains(role))
                || (identity.StaffNumber is null) == (identity.UniversityId is null))
            {
                throw new InvalidOperationException(
                    "IDENTITY_SEED_INPUT_INVALID: Synthetic identity input is incomplete or outside the approved role/profile shape.");
            }
        }
    }

    private static void EnsureDistinct<T>(IEnumerable<T> values, string description)
        where T : notnull
    {
        var seen = new HashSet<T>();
        if (values.Any(value => !seen.Add(value)))
        {
            throw new InvalidOperationException(
                $"IDENTITY_SEED_DUPLICATE: Duplicate {description} in the synthetic fixture.");
        }
    }

    private async Task<IReadOnlySet<Guid>> ExecuteAtomicallyAsync(
        Func<Task<IReadOnlySet<Guid>>> operation,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await operation().ConfigureAwait(false);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
                async () =>
                {
                    await using var transaction = await _dbContext.Database
                        .BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            cancellationToken)
                        .ConfigureAwait(false);
                    var insertedIds = await operation().ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return insertedIds;
                })
            .ConfigureAwait(false);
    }

    private static Guid StableGuid(Guid userId, string discriminator)
    {
        var input = Encoding.UTF8.GetBytes($"{userId:N}|{discriminator}");
        var hash = SHA256.HashData(input);
        return new Guid(hash.AsSpan(0, 16));
    }
}
