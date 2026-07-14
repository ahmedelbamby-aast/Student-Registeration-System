using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

/// <summary>
/// SQL Server adapter for IdentityAccess account and session-lifecycle state.
/// Contested lifecycle transitions use database predicates and one transaction.
/// </summary>
public sealed class IdentityAccountStore : IIdentityAccountStore
{
    private readonly StudentRegistrationDbContext _dbContext;
    private readonly IdentitySecurityOptions _securityOptions;
    private readonly TimeProvider _timeProvider;

    public IdentityAccountStore(
        StudentRegistrationDbContext dbContext,
        IdentitySecurityOptions securityOptions,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _securityOptions = securityOptions
            ?? throw new ArgumentNullException(nameof(securityOptions));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public Task<ApplicationUser?> FindStudentByUniversityIdAsync(
        string normalizedUniversityId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUniversityId);
        return _dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.UniversityId == normalizedUniversityId,
                cancellationToken);
    }

    public Task<ApplicationUser?> FindByNormalizedUserNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUserName);
        return _dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.NormalizedUserName == normalizedUserName,
                cancellationToken);
    }

    public Task<ApplicationUser?> FindByIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) =>
        _dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Id == applicationUserId,
                cancellationToken);

    public Task<Staff?> FindStaffAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) =>
        _dbContext.Set<Staff>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                staff => staff.ApplicationUserId == applicationUserId,
                cancellationToken);

    public Task<bool> IsStudentActivatedAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken) =>
        _dbContext.Set<StudentActivation>()
            .AsNoTracking()
            .AnyAsync(
                activation => activation.ApplicationUserId == applicationUserId
                    && activation.ActivatedAtUtc != null,
                cancellationToken);

    public async Task<IReadOnlyList<string>> GetEffectiveRolesAsync(
        Guid applicationUserId,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        await _dbContext.Set<RoleAssignment>()
            .AsNoTracking()
            .Where(assignment => assignment.ApplicationUserId == applicationUserId
                && assignment.EffectiveFromUtc <= utcNow
                && (assignment.EffectiveToUtc == null
                    || utcNow < assignment.EffectiveToUtc))
            .Select(assignment => assignment.RoleCode)
            .Distinct()
            .OrderBy(roleCode => roleCode)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task RecordAuthenticationFailureAsync(
        Guid? applicationUserId,
        string operation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        if (applicationUserId is null)
        {
            return;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var lockoutEndUtc = utcNow.Add(_securityOptions.LockoutDuration);
        var maximumFailures = _securityOptions.MaximumFailures;

        await _dbContext.Set<ApplicationUser>()
            .Where(user => user.Id == applicationUserId.Value
                && (user.LockoutEndUtc == null || user.LockoutEndUtc <= utcNow))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        user => user.AccessFailedCount,
                        user => user.LockoutEndUtc != null
                            && user.LockoutEndUtc <= utcNow
                                ? 1
                                : user.AccessFailedCount < maximumFailures
                                    ? user.AccessFailedCount + 1
                                    : user.AccessFailedCount)
                    .SetProperty(
                        user => user.LockoutEndUtc,
                        user => user.LockoutEndUtc != null
                            && user.LockoutEndUtc <= utcNow
                                ? maximumFailures == 1
                                    ? lockoutEndUtc
                                    : (DateTime?)null
                                : user.AccessFailedCount + 1 >= maximumFailures
                                    ? lockoutEndUtc
                                    : user.LockoutEndUtc),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ResetAuthenticationFailuresAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<ApplicationUser>()
            .Where(user => user.Id == applicationUserId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(user => user.AccessFailedCount, 0)
                    .SetProperty(user => user.LockoutEndUtc, (DateTime?)null),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task RecordActivationFailureAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var maximumFailures = _securityOptions.MaximumFailures;
        await _dbContext.Set<StudentActivation>()
            .Where(activation => activation.ApplicationUserId == applicationUserId
                && activation.ActivatedAtUtc == null
                && activation.FailedAttemptCount < maximumFailures)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    activation => activation.FailedAttemptCount,
                    activation => activation.FailedAttemptCount + 1),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> TryActivateAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newPasswordHash,
        string newSecurityStamp,
        DateTime activatedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expectedVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(newSecurityStamp);
        var proofIssuedAfterUtc = activatedAtUtc.Subtract(_securityOptions.ProofLifetime);

        return ExecuteAtomicallyAsync(
            async () =>
            {
                var activationUpdated = await _dbContext.Set<StudentActivation>()
                    .Where(activation => activation.ApplicationUserId == applicationUserId
                        && activation.ActivatedAtUtc == null
                        && activation.ProvisionedAtUtc <= activatedAtUtc
                        && activation.ProvisionedAtUtc > proofIssuedAfterUtc
                        && activation.FailedAttemptCount < _securityOptions.MaximumFailures)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            activation => activation.ActivatedAtUtc,
                            activatedAtUtc),
                        cancellationToken)
                    .ConfigureAwait(false);
                if (activationUpdated != 1)
                {
                    return false;
                }

                var userUpdated = await _dbContext.Set<ApplicationUser>()
                    .Where(user => user.Id == applicationUserId
                        && user.IsEnabled
                        && user.Version == expectedVersion)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(user => user.PasswordHash, newPasswordHash)
                            .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                            .SetProperty(user => user.AccessFailedCount, 0)
                            .SetProperty(user => user.LockoutEndUtc, (DateTime?)null),
                        cancellationToken)
                    .ConfigureAwait(false);
                return userUpdated == 1;
            },
            cancellationToken);
    }

    public async Task<bool> AddRecoveryChallengeAsync(
        AccountRecoveryChallenge challenge,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        await _dbContext.Set<AccountRecoveryChallenge>()
            .AddAsync(challenge, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false) == 1;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _dbContext.Entry(challenge).State = EntityState.Detached;
            return false;
        }
    }

    public Task<AccountRecoveryChallenge?> FindRecoveryChallengeAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        return _dbContext.Set<AccountRecoveryChallenge>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                challenge => challenge.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task RecordRecoveryFailureAsync(
        Guid challengeId,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        if (maximumAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }

        await _dbContext.Set<AccountRecoveryChallenge>()
            .Where(challenge => challenge.Id == challengeId
                && challenge.ConsumedAtUtc == null
                && challenge.FailedAttemptCount < maximumAttempts)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    challenge => challenge.FailedAttemptCount,
                    challenge => challenge.FailedAttemptCount + 1),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> TryCompleteRecoveryAsync(
        Guid challengeId,
        byte[] expectedChallengeVersion,
        Guid applicationUserId,
        string newPasswordHash,
        string newSecurityStamp,
        DateTime consumedAtUtc,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expectedChallengeVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(newSecurityStamp);
        if (maximumAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }

        return ExecuteAtomicallyAsync(
            async () =>
            {
                var challengeUpdated = await _dbContext.Set<AccountRecoveryChallenge>()
                    .Where(challenge => challenge.Id == challengeId
                        && challenge.ApplicationUserId == applicationUserId
                        && challenge.Version == expectedChallengeVersion
                        && challenge.ConsumedAtUtc == null
                        && challenge.ExpiresAtUtc > consumedAtUtc
                        && challenge.FailedAttemptCount < maximumAttempts)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            challenge => challenge.ConsumedAtUtc,
                            consumedAtUtc),
                        cancellationToken)
                    .ConfigureAwait(false);
                if (challengeUpdated != 1)
                {
                    return false;
                }

                var userUpdated = await _dbContext.Set<ApplicationUser>()
                    .Where(user => user.Id == applicationUserId && user.IsEnabled)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(user => user.PasswordHash, newPasswordHash)
                            .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                            .SetProperty(user => user.AccessFailedCount, 0)
                            .SetProperty(user => user.LockoutEndUtc, (DateTime?)null),
                        cancellationToken)
                    .ConfigureAwait(false);
                return userUpdated == 1;
            },
            cancellationToken);
    }

    public Task<bool> ChangePasswordAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newPasswordHash,
        string newSecurityStamp,
        CancellationToken cancellationToken) =>
        UpdateSecurityStateAsync(
            applicationUserId,
            expectedVersion,
            newPasswordHash,
            newSecurityStamp,
            cancellationToken);

    public Task<bool> RotateSecurityStampAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string newSecurityStamp,
        CancellationToken cancellationToken) =>
        UpdateSecurityStateAsync(
            applicationUserId,
            expectedVersion,
            passwordHash: null,
            newSecurityStamp,
            cancellationToken);

    private async Task<bool> UpdateSecurityStateAsync(
        Guid applicationUserId,
        byte[] expectedVersion,
        string? passwordHash,
        string newSecurityStamp,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expectedVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(newSecurityStamp);
        if (passwordHash is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        }

        var query = _dbContext.Set<ApplicationUser>()
            .Where(user => user.Id == applicationUserId
                && user.IsEnabled
                && user.Version == expectedVersion);
        var updated = passwordHash is null
            ? await query.ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        user => user.SecurityStamp,
                        newSecurityStamp),
                    cancellationToken)
                .ConfigureAwait(false)
            : await query.ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(user => user.PasswordHash, passwordHash)
                        .SetProperty(user => user.SecurityStamp, newSecurityStamp)
                        .SetProperty(user => user.AccessFailedCount, 0)
                        .SetProperty(user => user.LockoutEndUtc, (DateTime?)null),
                    cancellationToken)
                .ConfigureAwait(false);
        return updated == 1;
    }

    private async Task<bool> ExecuteAtomicallyAsync(
        Func<Task<bool>> operation,
        CancellationToken cancellationToken)
    {
        var currentTransaction = _dbContext.Database.CurrentTransaction;
        if (currentTransaction is not null)
        {
            if (!currentTransaction.SupportsSavepoints)
            {
                throw new InvalidOperationException(
                    "IDENTITY_SAVEPOINT_REQUIRED: The active transaction does not support safe nested lifecycle transitions.");
            }

            var savepointName = $"Identity_{Guid.NewGuid():N}"[..30];
            await currentTransaction.CreateSavepointAsync(savepointName, cancellationToken)
                .ConfigureAwait(false);
            try
            {
                var succeeded = await operation().ConfigureAwait(false);
                if (!succeeded)
                {
                    await currentTransaction.RollbackToSavepointAsync(
                            savepointName,
                            CancellationToken.None)
                        .ConfigureAwait(false);
                }

                await currentTransaction.ReleaseSavepointAsync(
                        savepointName,
                        CancellationToken.None)
                    .ConfigureAwait(false);
                return succeeded;
            }
            catch
            {
                await currentTransaction.RollbackToSavepointAsync(
                        savepointName,
                        CancellationToken.None)
                    .ConfigureAwait(false);
                throw;
            }
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
                async () =>
                {
                    await using var transaction = await _dbContext.Database
                        .BeginTransactionAsync(
                            IsolationLevel.ReadCommitted,
                            cancellationToken)
                        .ConfigureAwait(false);
                    var succeeded = await operation().ConfigureAwait(false);
                    if (succeeded)
                    {
                        await transaction.CommitAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await transaction.RollbackAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }

                    return succeeded;
                })
            .ConfigureAwait(false);
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
