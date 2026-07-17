using System.Security.Cryptography;
using System.Text;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

public sealed record RegistrationRequestScope
{
    public RegistrationRequestScope(
        Guid studentId,
        Guid termId,
        Guid clientRequestId)
    {
        EnsureIdentifier(studentId, nameof(studentId));
        EnsureIdentifier(termId, nameof(termId));
        EnsureIdentifier(clientRequestId, nameof(clientRequestId));
        StudentId = studentId;
        TermId = termId;
        ClientRequestId = clientRequestId;
    }

    public Guid StudentId { get; }
    public Guid TermId { get; }
    public Guid ClientRequestId { get; }

    private static void EnsureIdentifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("The identifier is required.", parameterName);
        }
    }
}

public enum SubmissionClaimStatus
{
    Claimed = 1,
    Replayed = 2,
    InProgress = 3,
    NotFound = 4,
    PayloadMismatch = 5,
}

public sealed record SubmissionClaimResult(
    SubmissionClaimStatus Status,
    RegistrationSubmission? Submission,
    string? ReasonCode)
{
    public bool MayExecute => Status is SubmissionClaimStatus.Claimed;
}

public sealed class RegistrationClaimContendedException : Exception
{
    public RegistrationClaimContendedException(Exception innerException)
        : base(
            "REGISTRATION_CLAIM_CONTENDED: Roll back the current transaction, then observe the scoped final result for at most 500 milliseconds.",
            innerException)
    {
    }
}

/// <summary>
/// Owns the single student-term-request claim and its final replay. A claim can
/// be inserted only in the caller's current SQL transaction; observation never
/// creates a durable Processing row. Accepted replay returns the immutable
/// Reference and ReceiptSnapshot stored on that same RegistrationSubmission;
/// Rejected replay returns its deterministic result without allocating again.
/// </summary>
public sealed class RegistrationSubmissionStore
{
    public const string IdempotentReplayMetricName = "registration.idempotent_replays";

    private const string MeterName = "StudentRegistration.Operations";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> IdempotentReplayMetric =
        Meter.CreateCounter<long>(IdempotentReplayMetricName);
    public static readonly TimeSpan MaximumObservationWindow =
        TimeSpan.FromMilliseconds(500);

    private static readonly TimeSpan ObservationPollInterval =
        TimeSpan.FromMilliseconds(25);

    private readonly StudentRegistrationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public RegistrationSubmissionStore(
        StudentRegistrationDbContext dbContext,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<SubmissionClaimResult> ClaimOrObserveAsync(
        RegistrationRequestScope scope,
        string payloadHash,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await ClaimInsideTransactionAsync(
                scope,
                payloadHash,
                receivedAtUtc,
                cancellationToken);
        }

        return await WaitForFinalResultAsync(
            scope,
            payloadHash,
            MaximumObservationWindow,
            cancellationToken);
    }

    public Task<SubmissionClaimResult> ClaimOrReplayAsync(
        RegistrationRequestScope scope,
        string payloadHash,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken = default) =>
        ClaimInsideTransactionAsync(
            scope,
            payloadHash,
            receivedAtUtc,
            cancellationToken);

    public async Task<SubmissionClaimResult> ClaimInsideTransactionAsync(
        RegistrationRequestScope scope,
        string payloadHash,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var normalizedHash = Required(payloadHash, nameof(payloadHash));
        EnsureUtc(receivedAtUtc, nameof(receivedAtUtc));
        RequireCallerTransaction();
        cancellationToken.ThrowIfCancellationRequested();
        await _dbContext.Database.ExecuteSqlRawAsync(
            "SET LOCK_TIMEOUT 500;",
            cancellationToken);
        try
        {
            var existing = await FindScopedAsync(
                scope,
                tracking: true,
                cancellationToken);
            if (existing is not null)
            {
                return Resolve(existing, normalizedHash);
            }

            var submission = new RegistrationSubmission(
                Guid.NewGuid(),
                scope.StudentId,
                scope.TermId,
                scope.ClientRequestId,
                normalizedHash,
                receivedAtUtc);
            _dbContext.Set<RegistrationSubmission>().Add(submission);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new SubmissionClaimResult(
                SubmissionClaimStatus.Claimed,
                submission,
                null);
        }
        catch (DbUpdateException exception)
            when (SqlErrorNumber(exception) is 1222)
        {
            _dbContext.ChangeTracker.Clear();
            throw new RegistrationClaimContendedException(exception);
        }
        catch (DbUpdateException exception)
            when (SqlErrorNumber(exception) is 2601 or 2627)
        {
            _dbContext.ChangeTracker.Clear();
            var winner = await FindScopedAsync(
                scope,
                tracking: false,
                cancellationToken);
            return winner is null
                ? new SubmissionClaimResult(
                    SubmissionClaimStatus.InProgress,
                    null,
                    "REGISTRATION_IN_PROGRESS")
                : Resolve(winner, normalizedHash);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception)
            when (exception.Number is 1222)
        {
            _dbContext.ChangeTracker.Clear();
            throw new RegistrationClaimContendedException(exception);
        }
        finally
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SET LOCK_TIMEOUT -1;",
                CancellationToken.None);
        }
    }

    public async Task<SubmissionClaimResult> WaitForFinalResultAsync(
        RegistrationRequestScope scope,
        string payloadHash,
        TimeSpan maximumWait,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var normalizedHash = Required(payloadHash, nameof(payloadHash));
        if (maximumWait < TimeSpan.Zero || maximumWait > MaximumObservationWindow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumWait),
                "Same-key observation must be between zero and 500 milliseconds.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var startedAt = _timeProvider.GetTimestamp();
        while (_timeProvider.GetElapsedTime(startedAt) < maximumWait)
        {
            var replay = await ReplayCommittedAsync(
                scope,
                normalizedHash,
                cancellationToken);
            if (replay.Status is not SubmissionClaimStatus.NotFound)
            {
                return replay;
            }

            var remaining = maximumWait - _timeProvider.GetElapsedTime(startedAt);
            if (remaining <= TimeSpan.Zero)
            {
                break;
            }

            await Task.Delay(
                remaining < ObservationPollInterval
                    ? remaining
                    : ObservationPollInterval,
                _timeProvider,
                cancellationToken);
        }

        return new SubmissionClaimResult(
            SubmissionClaimStatus.InProgress,
            null,
            "REGISTRATION_IN_PROGRESS");
    }

    public Task<SubmissionClaimResult> ReplayAsync(
        RegistrationRequestScope scope,
        string payloadHash,
        CancellationToken cancellationToken = default) =>
        ReplayCommittedAsync(scope, payloadHash, cancellationToken);

    public async Task<SubmissionClaimResult> ReplayCommittedAsync(
        RegistrationRequestScope scope,
        string payloadHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var normalizedHash = Required(payloadHash, nameof(payloadHash));
        var submission = await FindScopedAsync(scope, tracking: false, cancellationToken);
        if (submission is null || !submission.IsFinal)
        {
            return new SubmissionClaimResult(
                SubmissionClaimStatus.NotFound,
                null,
                "REQUEST_NOT_FOUND");
        }

        return Resolve(submission, normalizedHash);
    }

    public async Task<RegistrationSubmission?> ReadFinalByRequestAsync(
        RegistrationRequestScope scope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var submission = await FindScopedAsync(scope, tracking: false, cancellationToken);
        return submission?.IsFinal == true ? submission : null;
    }

    public async Task FinalizeAcceptedAsync(
        RegistrationSubmission submission,
        string resultCode,
        string reference,
        string receiptSnapshotJson,
        string decisionSnapshotJson,
        DateTime completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        RequireCallerTransaction();
        submission.CompleteAccepted(
            resultCode,
            reference,
            receiptSnapshotJson,
            decisionSnapshotJson,
            completedAtUtc);
        submission.EnsureFinalForCommit();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task FinalizeRejectedAsync(
        RegistrationSubmission submission,
        string resultCode,
        string decisionSnapshotJson,
        DateTime completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        RequireCallerTransaction();
        submission.CompleteRejected(
            resultCode,
            decisionSnapshotJson,
            completedAtUtc);
        submission.EnsureFinalForCommit();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public static string ComputeCanonicalPayloadHash(string canonicalPayload)
    {
        var normalized = Required(canonicalPayload, nameof(canonicalPayload));
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private async Task<RegistrationSubmission?> FindScopedAsync(
        RegistrationRequestScope scope,
        bool tracking,
        CancellationToken cancellationToken)
    {
        IQueryable<RegistrationSubmission> query = tracking
            ? _dbContext.Set<RegistrationSubmission>()
            : _dbContext.Set<RegistrationSubmission>()
                .FromSqlInterpolated(
                    $"""
                    SELECT *
                    FROM [registration].[RegistrationSubmissions] WITH (READPAST)
                    WHERE [StudentId] = {scope.StudentId}
                      AND [TermId] = {scope.TermId}
                      AND [ClientRequestId] = {scope.ClientRequestId}
                    """)
                .AsNoTracking();

        return tracking
            ? await query.SingleOrDefaultAsync(
                submission =>
                    submission.StudentId == scope.StudentId &&
                    submission.TermId == scope.TermId &&
                    submission.ClientRequestId == scope.ClientRequestId,
                cancellationToken)
            : await query.SingleOrDefaultAsync(cancellationToken);
    }

    private static SubmissionClaimResult Resolve(
        RegistrationSubmission submission,
        string payloadHash)
    {
        if (!string.Equals(
                submission.PayloadHash,
                payloadHash,
                StringComparison.Ordinal))
        {
            return new SubmissionClaimResult(
                SubmissionClaimStatus.PayloadMismatch,
                null,
                "IDEMPOTENCY_KEY_REUSED");
        }

        if (!submission.IsFinal)
        {
            return new SubmissionClaimResult(
                SubmissionClaimStatus.InProgress,
                null,
                "REGISTRATION_IN_PROGRESS");
        }

        IdempotentReplayMetric.Add(
            1,
            new KeyValuePair<string, object?>("module", "registration"),
            new KeyValuePair<string, object?>("operation", "commit"),
            new KeyValuePair<string, object?>("outcome", "success"));
        return new SubmissionClaimResult(
            SubmissionClaimStatus.Replayed,
            submission,
            submission.ResultCode);
    }

    private void RequireCallerTransaction()
    {
        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "REGISTRATION_TRANSACTION_REQUIRED: The submission claim must share the caller's SQL transaction.");
        }
    }

    private static string Required(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", parameterName);
        }
    }

    private static int? SqlErrorNumber(DbUpdateException exception) =>
        exception.GetBaseException() is Microsoft.Data.SqlClient.SqlException sql
            ? sql.Number
            : null;
}
