using System.Data;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Admin;

public sealed class SqlAdminExportStore : IAdminExportStore
{
    private readonly StudentRegistrationDbContext _dbContext;
    private readonly IAuditEventWriter _auditWriter;

    public SqlAdminExportStore(
        StudentRegistrationDbContext dbContext,
        IAuditEventWriter auditWriter)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _auditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
    }

    public async Task<AdminExportCreateResult> CreateOrReplayAsync(
        AdminExportCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Job);
        ArgumentNullException.ThrowIfNull(command.RequestAudit);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<AdminExportCreateResult>(async () =>
        {
            _dbContext.ChangeTracker.Clear();
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                .ConfigureAwait(false);
            var existing = await _dbContext.ExportJobs
                .FromSqlInterpolated($$"""
                    SELECT *
                    FROM [administration].[ExportJobs] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [OwnerId] = {{command.Job.OwnerId}}
                      AND [ScopeHash] = {{command.Job.ScopeHash}}
                      AND [ClientRequestId] = {{command.Job.ClientRequestId}}
                    """)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (existing is not null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return new(
                    string.Equals(
                        existing.RequestHash,
                        command.Job.RequestHash,
                        StringComparison.Ordinal)
                        ? AdminExportCreateOutcome.Replay
                        : AdminExportCreateOutcome.IdempotencyKeyReused,
                    existing);
            }

            _dbContext.ExportJobs.Add(command.Job);
            await _auditWriter.AppendAsync(command.RequestAudit, cancellationToken)
                .ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _dbContext.ChangeTracker.Clear();
            return new(AdminExportCreateOutcome.Created, command.Job);
        }).ConfigureAwait(false);
    }

    public Task<ExportJob?> ReadAuthorizedAsync(
        Guid jobId,
        Guid actorUserId,
        string scopeHash,
        bool canReadAll,
        CancellationToken cancellationToken = default) =>
        _dbContext.ExportJobs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                job => job.Id == jobId
                    && job.ScopeHash == scopeHash
                    && (job.OwnerId == actorUserId || canReadAll),
                cancellationToken);

    public async Task<ExportJob?> TryClaimAsync(
        Guid jobId,
        string leaseOwnerId,
        DateTime claimedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var leaseExpiresAtUtc = claimedAtUtc.Add(ExportJob.LeaseDuration);
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [administration].[ExportJobs] WITH (UPDLOCK, ROWLOCK)
            SET [State] = N'running',
                [LeaseOwnerId] = {leaseOwnerId},
                [LeaseExpiresAtUtc] = {leaseExpiresAtUtc},
                [AttemptCount] = [AttemptCount] + 1
            WHERE [JobId] = {jobId}
              AND [AttemptCount] < 3
              AND
              (
                  [State] = N'pending'
                  OR ([State] = N'running' AND [LeaseExpiresAtUtc] <= {claimedAtUtc})
              )
            """,
            cancellationToken).ConfigureAwait(false);
        if (affected != 1)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE [administration].[ExportJobs] WITH (UPDLOCK, ROWLOCK)
                SET [State] = N'failed',
                    [CompletedAtUtc] = {claimedAtUtc},
                    [FailureCode] = N'EXPORT_ATTEMPTS_EXHAUSTED',
                    [LeaseOwnerId] = NULL,
                    [LeaseExpiresAtUtc] = NULL
                WHERE [JobId] = {jobId}
                  AND [AttemptCount] >= 3
                  AND
                  (
                      [State] = N'pending'
                      OR ([State] = N'running' AND [LeaseExpiresAtUtc] <= {claimedAtUtc})
                  )
                """,
                cancellationToken).ConfigureAwait(false);
            return null;
        }

        _dbContext.ChangeTracker.Clear();
        return await _dbContext.ExportJobs
            .AsNoTracking()
            .SingleAsync(
                job => job.Id == jobId && job.LeaseOwnerId == leaseOwnerId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> RenewLeaseAsync(
        Guid jobId,
        string leaseOwnerId,
        DateTime renewedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var leaseExpiresAtUtc = renewedAtUtc.Add(ExportJob.LeaseDuration);
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [administration].[ExportJobs] WITH (UPDLOCK, ROWLOCK)
            SET [LeaseExpiresAtUtc] = {leaseExpiresAtUtc}
            WHERE [JobId] = {jobId}
              AND [State] = N'running'
              AND [LeaseOwnerId] = {leaseOwnerId}
              AND [LeaseExpiresAtUtc] > {renewedAtUtc}
            """,
            cancellationToken).ConfigureAwait(false);
        return affected == 1;
    }

    public async Task<bool> PublishAsync(
        Guid jobId,
        string leaseOwnerId,
        Guid artifactId,
        DateTime completedAtUtc,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [administration].[ExportJobs] WITH (UPDLOCK, ROWLOCK)
            SET [State] = N'complete',
                [ArtifactId] = {artifactId},
                [CompletedAtUtc] = {completedAtUtc},
                [ExpiresAtUtc] = {expiresAtUtc},
                [LeaseOwnerId] = NULL,
                [LeaseExpiresAtUtc] = NULL
            WHERE [JobId] = {jobId}
              AND [State] = N'running'
              AND [LeaseOwnerId] = {leaseOwnerId}
              AND [LeaseExpiresAtUtc] > {completedAtUtc}
              AND [ArtifactId] IS NULL
            """,
            cancellationToken).ConfigureAwait(false);
        return affected == 1;
    }

    public async Task<bool> RecordFailureAsync(
        Guid jobId,
        string leaseOwnerId,
        DateTime failedAtUtc,
        bool retryable,
        string failureCode,
        CancellationToken cancellationToken = default)
    {
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [administration].[ExportJobs] WITH (UPDLOCK, ROWLOCK)
            SET [State] = CASE
                    WHEN {retryable} = CAST(1 AS bit) AND [AttemptCount] < 3
                        THEN N'pending'
                    ELSE N'failed'
                END,
                [CompletedAtUtc] = CASE
                    WHEN {retryable} = CAST(1 AS bit) AND [AttemptCount] < 3
                        THEN NULL
                    ELSE {failedAtUtc}
                END,
                [FailureCode] = CASE
                    WHEN {retryable} = CAST(1 AS bit) AND [AttemptCount] < 3
                        THEN NULL
                    ELSE {failureCode}
                END,
                [LeaseOwnerId] = NULL,
                [LeaseExpiresAtUtc] = NULL
            WHERE [JobId] = {jobId}
              AND [State] = N'running'
              AND [LeaseOwnerId] = {leaseOwnerId}
              AND [LeaseExpiresAtUtc] > {failedAtUtc}
            """,
            cancellationToken).ConfigureAwait(false);
        return affected == 1;
    }

    public async Task<bool> RecordDownloadAsync(
        Guid jobId,
        Guid actorUserId,
        string scopeHash,
        bool canReadAll,
        AuditEventDraft downloadAudit,
        DateTime observedAtUtc,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
        var authorized = await _dbContext.ExportJobs
            .FromSqlInterpolated($$"""
                SELECT *
                FROM [administration].[ExportJobs] WITH (UPDLOCK, HOLDLOCK)
                WHERE [JobId] = {{jobId}}
                  AND [ScopeHash] = {{scopeHash}}
                  AND ([OwnerId] = {{actorUserId}} OR {{canReadAll}} = CAST(1 AS bit))
                  AND [State] = N'complete'
                  AND [ExpiresAtUtc] > {{observedAtUtc}}
                """)
            .AsNoTracking()
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
        if (!authorized)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return false;
        }

        await _auditWriter.AppendAsync(downloadAudit, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        _dbContext.ChangeTracker.Clear();
        return true;
    }

    public async Task<bool> ExpireAsync(
        Guid jobId,
        Guid artifactId,
        DateTime observedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [administration].[ExportJobs] WITH (UPDLOCK, ROWLOCK)
            SET [State] = N'expired'
            WHERE [JobId] = {jobId}
              AND [State] = N'complete'
              AND [ArtifactId] = {artifactId}
              AND [ExpiresAtUtc] <= {observedAtUtc}
            """,
            cancellationToken).ConfigureAwait(false);
        return affected == 1;
    }
}
