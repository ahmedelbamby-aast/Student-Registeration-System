using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Admin;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.StaffAdministration.Application;

namespace StudentRegistration.IntegrationTests.Admin;

public sealed class AuditExportSqlConcurrencyTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Two_replicas_share_idempotency_lease_artifact_scope_audit_and_expiry()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec017_Export_{Guid.NewGuid():N}"
        }.ConnectionString;
        var artifactRoot = Path.Combine(
            Path.GetTempPath(),
            $"StudentRegistration_Test_Spec017_Export_{Guid.NewGuid():N}");
        var time = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 17, 12, 0, 0, TimeSpan.Zero));

        await using var setup = CreateContext(connectionString);
        try
        {
            await setup.Database.MigrateAsync();
            await using var replicaOneContext = CreateContext(connectionString);
            await using var replicaTwoContext = CreateContext(connectionString);
            var replicaOne = Service(replicaOneContext, artifactRoot, time);
            var replicaTwo = Service(replicaTwoContext, artifactRoot, time);
            var ownerId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
            var scopeHash = "SHA256:SCOPE-AI";
            var filter = new AdminExportFilter(
                time.UtcNow.AddHours(-1),
                time.UtcNow.AddHours(1),
                actorId,
                "AcademicTermUpdated",
                "audit");
            var requestId = Guid.NewGuid();
            var request = Request(ownerId, requestId, scopeHash, filter, time.UtcNow);

            var created = await replicaOne.RequestAsync(request);
            Assert.Equal(AdminExportOutcome.Created, created.Outcome);
            var jobId = created.Job!.Id;
            Assert.Equal(filter, AuditExportService.DeserializeFilter(created.Job.FilterJson));

            var replay = await replicaTwo.RequestAsync(request);
            Assert.Equal(AdminExportOutcome.Replay, replay.Outcome);
            Assert.Equal(jobId, replay.Job!.Id);
            Assert.Equal(
                1,
                await setup.AuditEvents.CountAsync(row => row.Action == "AdminExportRequested"));

            var mismatch = await replicaTwo.RequestAsync(
                request with
                {
                    Filter = filter with { Action = "PolicyPublished" }
                });
            Assert.Equal(AdminExportOutcome.IdempotencyKeyReused, mismatch.Outcome);
            Assert.Equal("IDEMPOTENCY_KEY_REUSED", mismatch.ErrorCode);
            Assert.Equal(1, await setup.ExportJobs.CountAsync());

            var denied = await replicaTwo.GetStatusAsync(
                jobId,
                Access(Guid.NewGuid(), "SHA256:OTHER-SCOPE", canReadAll: false));
            Assert.Equal(AdminExportOutcome.NotFound, denied.Outcome);

            var claims = await Task.WhenAll(
                replicaOne.TryClaimAsync(jobId, "replica-one"),
                replicaTwo.TryClaimAsync(jobId, "replica-two"));
            var claim = Assert.Single(claims, candidate => candidate is not null)!;
            Assert.Single(claims, candidate => candidate is null);
            var winner = claim.LeaseOwnerId == "replica-one" ? replicaOne : replicaTwo;
            var loserStore = claim.LeaseOwnerId == "replica-one"
                ? Store(replicaTwoContext)
                : Store(replicaOneContext);

            var unsafePublish = await winner.PublishAsync(
                claim,
                claim.LeaseOwnerId!,
                filter,
                [Row(actorId, scopeHash, time.UtcNow) with
                    { RedactedAfterSummary = "{\"password\":\"forbidden\"}" }]);
            Assert.Equal(AdminExportOutcome.Invalid, unsafePublish.Outcome);

            var published = await winner.PublishAsync(
                claim,
                claim.LeaseOwnerId!,
                filter,
                [Row(actorId, scopeHash, time.UtcNow)],
                TimeSpan.FromMinutes(1));
            Assert.Equal(AdminExportOutcome.Succeeded, published.Outcome);
            Assert.False(await loserStore.PublishAsync(
                jobId,
                "losing-replica",
                Guid.NewGuid(),
                time.UtcNow,
                time.UtcNow.AddMinutes(1)));

            var readAll = await replicaTwo.GetStatusAsync(
                jobId,
                Access(Guid.NewGuid(), scopeHash, canReadAll: true));
            Assert.Equal(AdminExportOutcome.Succeeded, readAll.Outcome);
            Assert.Equal(StudentRegistration.StaffAdministration.Domain.ExportJobState.Complete,
                readAll.Job!.State);

            var downloaded = await replicaTwo.DownloadAsync(
                jobId,
                Access(ownerId, scopeHash, canReadAll: false));
            Assert.Equal(AdminExportOutcome.Succeeded, downloaded.Outcome);
            Assert.NotNull(downloaded.Content);
            Assert.Contains(
                "AcademicTermUpdated",
                System.Text.Encoding.UTF8.GetString(downloaded.Content!.Value.Span),
                StringComparison.Ordinal);
            Assert.Equal(
                1,
                await setup.AuditEvents.CountAsync(row => row.Action == "AdminExportDownloaded"));

            time.Advance(TimeSpan.FromMinutes(2));
            var expired = await replicaOne.GetStatusAsync(
                jobId,
                Access(ownerId, scopeHash, canReadAll: false));
            Assert.Equal(AdminExportOutcome.Expired, expired.Outcome);
            var artifactId = readAll.Job.ArtifactId!.Value;
            Assert.Null(await new SharedFileAdminExportArtifactStore(artifactRoot)
                .ReadAsync(artifactId));
            var expiredDownload = await replicaTwo.DownloadAsync(
                jobId,
                Access(ownerId, scopeHash, canReadAll: false));
            Assert.Equal(AdminExportOutcome.Expired, expiredDownload.Outcome);

            var newKeyRetry = await replicaTwo.RequestAsync(
                Request(ownerId, Guid.NewGuid(), scopeHash, filter, time.UtcNow));
            Assert.Equal(AdminExportOutcome.Created, newKeyRetry.Outcome);
            Assert.NotEqual(jobId, newKeyRetry.Job!.Id);
            var terminalReplay = await replicaOne.RequestAsync(request);
            Assert.Equal(AdminExportOutcome.Replay, terminalReplay.Outcome);
            Assert.Equal(jobId, terminalReplay.Job!.Id);
            Assert.Equal(
                StudentRegistration.StaffAdministration.Domain.ExportJobState.Expired,
                terminalReplay.Job.State);

            var nextClaim = await replicaOne.TryClaimNextAsync("replica-next");
            Assert.NotNull(nextClaim);
            Assert.Equal(newKeyRetry.Job.Id, nextClaim.Id);
            Assert.Equal("replica-next", nextClaim.LeaseOwnerId);

            Assert.True(AuditExportService.IsValidPage(1, 100));
            Assert.False(AuditExportService.IsValidPage(0, 20));
            Assert.False(AuditExportService.IsValidPage(1, 101));
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
            if (Directory.Exists(artifactRoot))
            {
                Directory.Delete(artifactRoot, recursive: true);
            }
        }
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Retryable_worker_failures_stop_after_three_claims()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec017_Attempts_{Guid.NewGuid():N}"
        }.ConnectionString;
        var artifactRoot = Path.Combine(
            Path.GetTempPath(),
            $"StudentRegistration_Test_Spec017_Attempts_{Guid.NewGuid():N}");
        var time = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 17, 13, 0, 0, TimeSpan.Zero));
        await using var context = CreateContext(connectionString);
        try
        {
            await context.Database.MigrateAsync();
            var service = Service(context, artifactRoot, time);
            var ownerId = Guid.NewGuid();
            var scopeHash = "SHA256:SCOPE-ATTEMPTS";
            var filter = new AdminExportFilter(SourceStream: "audit");
            var created = await service.RequestAsync(
                Request(ownerId, Guid.NewGuid(), scopeHash, filter, time.UtcNow));

            for (var attempt = 1; attempt <= 3; attempt++)
            {
                var claim = await service.TryClaimAsync(created.Job!.Id, $"replica-{attempt}");
                Assert.NotNull(claim);
                Assert.Equal(attempt, claim.AttemptCount);
                Assert.True(await service.RecordFailureAsync(
                    claim.Id,
                    claim.LeaseOwnerId!,
                    retryable: true,
                    "EXPORT_SOURCE_UNAVAILABLE"));
            }

            Assert.Null(await service.TryClaimAsync(created.Job!.Id, "replica-four"));
            var status = await service.GetStatusAsync(
                created.Job.Id,
                Access(ownerId, scopeHash, canReadAll: false));
            Assert.Equal(AdminExportOutcome.Succeeded, status.Outcome);
            Assert.Equal(
                StudentRegistration.StaffAdministration.Domain.ExportJobState.Failed,
                status.Job!.State);
            Assert.Equal(3, status.Job.AttemptCount);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
            if (Directory.Exists(artifactRoot))
            {
                Directory.Delete(artifactRoot, recursive: true);
            }
        }
    }

    private static AdminExportRequest Request(
        Guid ownerId,
        Guid requestId,
        string scopeHash,
        AdminExportFilter filter,
        DateTime requestedAtUtc) =>
        new(
            ownerId,
            requestId,
            scopeHash,
            filter,
            $"user:{ownerId:N}",
            $"correlation-{requestId:N}",
            requestedAtUtc);

    private static AdminExportAccessContext Access(
        Guid actorId,
        string scopeHash,
        bool canReadAll) =>
        new(
            actorId,
            $"user:{actorId:N}",
            scopeHash,
            canReadAll,
            $"download-{Guid.NewGuid():N}");

    private static AdminAuditExportRow Row(
        Guid actorId,
        string scopeHash,
        DateTime occurredAtUtc) =>
        new(
            Guid.NewGuid(),
            occurredAtUtc,
            actorId,
            $"user:{actorId:N}",
            "AcademicTermUpdated",
            "AcademicTerm",
            Guid.NewGuid().ToString("N"),
            "Approved demo update.",
            "{\"state\":\"draft\"}",
            "{\"state\":\"published\"}",
            "correlation-export-row",
            "audit",
            scopeHash);

    private static AuditExportService Service(
        StudentRegistrationDbContext context,
        string artifactRoot,
        TimeProvider timeProvider) =>
        new(
            Store(context),
            new SharedFileAdminExportArtifactStore(artifactRoot),
            timeProvider);

    private static SqlAdminExportStore Store(StudentRegistrationDbContext context) =>
        new(context, new AuditTransactionWriter(context));

    private static StudentRegistrationDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServer => sqlServer.EnableRetryOnFailure())
            .Options);

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public DateTime UtcNow => _utcNow.UtcDateTime;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan elapsed) => _utcNow = _utcNow.Add(elapsed);
    }
}
