using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

/// <summary>
/// Replica-shared keyed abuse counters. Raw login, University ID, and network
/// values never cross this adapter; only the application-produced HMAC key is stored.
/// </summary>
public sealed class IdentityAbuseStateStore : IIdentityAbuseStateStore
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly StudentRegistrationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public IdentityAbuseStateStore(
        StudentRegistrationDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<IdentityAbuseStateSnapshot?> FindAsync(
        SubjectKeyHash subjectKey,
        string operation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        var state = await _dbContext.Set<AuthenticationAbuseState>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.SubjectKeyHash == subjectKey.Value
                    && candidate.Operation == operation,
                cancellationToken)
            .ConfigureAwait(false);
        return state is null ? null : ToSnapshot(state);
    }

    public async ValueTask<IdentityAbuseStateSnapshot> RecordFailureAsync(
        IdentityAbuseFailure failure,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(failure);
        if (failure.MaximumFailures < 1 || failure.BlockDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(failure));
        }

        var eventId = Guid.NewGuid();
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    .ConfigureAwait(false);
                var state = await LockStateAsync(failure.SubjectKeyHash, cancellationToken)
                    .ConfigureAwait(false);
                var alreadyCommitted = await _dbContext.Set<SecurityEvent>()
                    .AsNoTracking()
                    .AnyAsync(
                        securityEvent => securityEvent.Id == eventId,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (alreadyCommitted)
                {
                    if (state is null)
                    {
                        throw new InvalidOperationException(
                            "IDENTITY_ABUSE_AUDIT_STATE_MISMATCH: A committed failure event has no durable abuse state.");
                    }

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    return ToSnapshot(state);
                }

                var observedAtUtc = failure.ObservedAtUtc.UtcDateTime;
                if (state is null)
                {
                    state = new AuthenticationAbuseState(
                        Guid.NewGuid(),
                        failure.SubjectKeyHash.Value,
                        failure.Operation,
                        observedAtUtc);
                    _dbContext.Add(state);
                }
                else if (!string.Equals(
                    state.Operation,
                    failure.Operation,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "IDENTITY_ABUSE_OPERATION_MISMATCH: The keyed subject belongs to another operation.");
                }

                var before = ToAuditSummary(state, observedAtUtc);
                state.RecordFailure(
                    observedAtUtc,
                    failure.MaximumFailures,
                    failure.BlockDuration,
                    failure.BlockDuration);
                var after = ToAuditSummary(state, observedAtUtc);
                _dbContext.Add(CreateFailureEvent(eventId, failure, before, after));
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return ToSnapshot(state);
            }).ConfigureAwait(false);
        }
        finally
        {
            _dbContext.ChangeTracker.Clear();
        }
    }

    public async ValueTask ResetAsync(
        SubjectKeyHash subjectKey,
        string operation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                .ConfigureAwait(false);
            var state = await LockStateAsync(subjectKey, cancellationToken)
                .ConfigureAwait(false);
            if (state is not null)
            {
                if (!string.Equals(state.Operation, operation, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "IDENTITY_ABUSE_OPERATION_MISMATCH: The keyed subject belongs to another operation.");
                }

                state.Reset(_timeProvider.GetUtcNow().UtcDateTime);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _dbContext.ChangeTracker.Clear();
        }).ConfigureAwait(false);
    }

    private Task<AuthenticationAbuseState?> LockStateAsync(
        SubjectKeyHash subjectKey,
        CancellationToken cancellationToken) =>
        _dbContext.Set<AuthenticationAbuseState>()
            .FromSqlInterpolated($"""
                SELECT [Id], [SubjectKeyHash], [Operation], [FailureCount],
                       [WindowStartedAtUtc], [LockedUntilUtc], [Version]
                FROM [auth].[AuthenticationAbuseStates] WITH (UPDLOCK, HOLDLOCK)
                WHERE [SubjectKeyHash] = {subjectKey.Value}
                """)
            .SingleOrDefaultAsync(cancellationToken);

    private static IdentityAbuseStateSnapshot ToSnapshot(AuthenticationAbuseState state) =>
        new(
            state.FailureCount,
            AsUtcOffset(state.WindowStartedAtUtc),
            state.LockedUntilUtc is { } blockedUntil
                ? AsUtcOffset(blockedUntil)
                : null);

    private static DateTimeOffset AsUtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static SecurityEvent CreateFailureEvent(
        Guid eventId,
        IdentityAbuseFailure failure,
        string beforeSummary,
        string afterSummary) =>
        new(
            eventId,
            applicationUserId: null,
            EventTypeFor(failure.Operation),
            "identity-gateway",
            $"hmac-sha256:{failure.SubjectKeyHash.Value}",
            "Authentication lifecycle failure recorded.",
            beforeSummary,
            afterSummary,
            JsonSerializer.Serialize(new { operation = failure.Operation }, JsonOptions),
            $"identity-abuse:{eventId:N}",
            failure.ObservedAtUtc.UtcDateTime);

    private static string ToAuditSummary(
        AuthenticationAbuseState state,
        DateTime observedAtUtc) =>
        JsonSerializer.Serialize(
            new
            {
                failureCount = state.FailureCount,
                blocked = state.IsLockedAt(observedAtUtc)
            },
            JsonOptions);

    private static string EventTypeFor(string operation) => operation switch
    {
        "login" => "IdentityLoginFailure",
        "activation" => "IdentityActivationFailure",
        "recovery" => "IdentityRecoveryFailure",
        "password-change" => "IdentityPasswordChangeFailure",
        _ => throw new ArgumentOutOfRangeException(
            nameof(operation),
            operation,
            "The identity failure operation is not allow-listed.")
    };
}
