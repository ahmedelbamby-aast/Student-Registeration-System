using System.Diagnostics;
using System.Text.Json;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.Infrastructure.SqlServer.Admin;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec017;

public sealed class NFR_3EvidenceTests
{
    private const string ImplementationCommit =
        "e3c567f43e608597a66b9d8e5f1f0bcfc4b04594";
    private const string RawArtifactPath =
        "docs/release-evidence/SPEC-017-NFR-3-results.json";
    private const string EvidencePath =
        "docs/release-evidence/SPEC-017-NFR-3.md";

    [Fact]
    public async Task Two_replicas_publish_one_maximum_size_export_then_securely_expire_it()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"StudentRegistration_SPEC017_NFR3_{Guid.NewGuid():N}");
        var now = new DateTimeOffset(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);
        var time = new MutableTimeProvider(now);
        var filter = new AdminExportFilter(SourceStream: "audit");
        var scopeHash = "SHA256:SPEC017-NFR3-SCOPE";
        var job = new ExportJob(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scopeHash,
            AuditExportService.ComputeRequestHash(filter),
            now.UtcDateTime,
            AuditExportService.SerializeFilter(filter));
        Assert.True(job.TryClaim("replica-one", now.UtcDateTime));
        Assert.Equal(TimeSpan.FromSeconds(60), job.LeaseExpiresAtUtc - now.UtcDateTime);

        var store = new ConditionalExportStore(job);
        var artifacts = new SharedFileAdminExportArtifactStore(root);
        var replicaOne = new AuditExportService(store, artifacts, time);
        var replicaTwo = new AuditExportService(store, artifacts, time);
        var rows = Enumerable.Range(1, AuditExportService.MaximumExportRows)
            .Select(index => Row(index, scopeHash, now.UtcDateTime.AddSeconds(-index)))
            .ToArray();

        try
        {
            var started = Stopwatch.GetTimestamp();
            var outcomes = await Task.WhenAll(
                replicaOne.PublishAsync(
                    job,
                    "replica-one",
                    filter,
                    rows,
                    TimeSpan.FromMinutes(1)),
                replicaTwo.PublishAsync(
                    job,
                    "replica-one",
                    filter,
                    rows,
                    TimeSpan.FromMinutes(1)));
            var elapsed = Stopwatch.GetElapsedTime(started);

            Assert.Single(outcomes, result => result.Outcome is AdminExportOutcome.Succeeded);
            Assert.Single(outcomes, result => result.Outcome is AdminExportOutcome.LeaseUnavailable);
            Assert.Equal(1, store.SuccessfulPublications);
            var artifactPath = Assert.Single(Directory.GetFiles(root, "*.export"));
            var bytes = new FileInfo(artifactPath).Length;
            Assert.InRange(bytes, 1, AuditExportService.MaximumArtifactBytes);
            Assert.Equal(AuditExportService.MaximumExportRows + 1, File.ReadLines(artifactPath).Count());
            Assert.Equal(TimeSpan.FromMinutes(1), job.ExpiresAtUtc - job.CompletedAtUtc);

            Console.WriteLine(FormattableString.Invariant(
                $"""SPEC-017 NFR-3 rows={rows.Length}; bytes={bytes}; elapsed-ms={elapsed.TotalMilliseconds:F4}; publications={store.SuccessfulPublications}; artifacts-before-expiry=1; lease-seconds=60; retention-seconds=60"""));

            time.Advance(TimeSpan.FromSeconds(61));
            var expired = await replicaTwo.GetStatusAsync(
                job.Id,
                new AdminExportAccessContext(
                    job.OwnerId,
                    $"user:{job.OwnerId:N}",
                    scopeHash,
                    CanReadAll: false,
                    CorrelationId: "nfr3-expiry"));
            Assert.Equal(AdminExportOutcome.Expired, expired.Outcome);
            Assert.Equal(ExportJobState.Expired, job.State);
            Assert.Empty(Directory.GetFiles(root, "*.export"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void Recorded_run_is_bound_to_limits_concurrency_expiry_and_real_sql_evidence()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(RawArtifactPath));
        var root = document.RootElement;
        Assert.Equal("spec017-nfr3-results/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal(ImplementationCommit, root.GetProperty("implementationCommit").GetString());
        Assert.Equal("PASS", root.GetProperty("result").GetString());
        Assert.Equal(10_000, root.GetProperty("fixture").GetProperty("rows").GetInt32());
        Assert.Equal(2, root.GetProperty("fixture").GetProperty("replicas").GetInt32());
        Assert.Equal(1, root.GetProperty("measurement").GetProperty("successfulPublications").GetInt32());
        Assert.Equal(1, root.GetProperty("measurement").GetProperty("artifactsBeforeExpiry").GetInt32());
        Assert.Equal(0, root.GetProperty("measurement").GetProperty("artifactsAfterExpiry").GetInt32());
        Assert.True(
            root.GetProperty("measurement").GetProperty("artifactBytes").GetInt64()
            <= AuditExportService.MaximumArtifactBytes);
        Assert.Equal(60, root.GetProperty("measurement").GetProperty("leaseSeconds").GetInt32());
        Assert.Equal(60, root.GetProperty("measurement").GetProperty("retentionSeconds").GetInt32());

        var sqlEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Admin/AuditExportSqlConcurrencyTests.cs");
        RepositoryFiles.ContainsAll(
            sqlEvidence,
            "Two_replicas_share_idempotency_lease_artifact_scope_audit_and_expiry",
            "Task.WhenAll",
            "TryClaimNextAsync",
            "Retryable_worker_failures_stop_after_three_claims",
            "replica-four");

        var report = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            report,
            "# SPEC-017 NFR-3 Export Lifecycle Evidence",
            ImplementationCommit,
            RawArtifactPath,
            "10,000 rows",
            "two replicas",
            "one published artifact",
            "60-second lease",
            "secure expiry",
            "**Result: PASS.**");
    }

    private static AdminAuditExportRow Row(int index, string scopeHash, DateTime occurredAtUtc) =>
        new(
            Guid.Parse($"00000000-0000-0000-0000-{index:D12}"),
            occurredAtUtc,
            Guid.Parse("00000000-0000-0000-0000-000000000017"),
            "user:00000000000000000000000000000017",
            "AcademicTermUpdated",
            "AcademicTerm",
            index.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Approved demo export row.",
            "{\"state\":\"draft\"}",
            "{\"state\":\"published\"}",
            $"nfr3-{index:D5}",
            "audit",
            scopeHash);

    private sealed class ConditionalExportStore(ExportJob job) : IAdminExportStore
    {
        private readonly object _gate = new();
        private readonly ExportJob _job = job;

        public int SuccessfulPublications { get; private set; }

        public Task<bool> PublishAsync(
            Guid jobId,
            string leaseOwnerId,
            Guid artifactId,
            DateTime completedAtUtc,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (_job.Id != jobId
                    || _job.State is not ExportJobState.Running
                    || !string.Equals(_job.LeaseOwnerId, leaseOwnerId, StringComparison.Ordinal)
                    || _job.LeaseExpiresAtUtc <= completedAtUtc)
                {
                    return Task.FromResult(false);
                }

                _job.Complete(leaseOwnerId, artifactId, completedAtUtc, expiresAtUtc);
                SuccessfulPublications++;
                return Task.FromResult(true);
            }
        }

        public Task<ExportJob?> ReadAuthorizedAsync(
            Guid jobId,
            Guid actorUserId,
            string scopeHash,
            bool canReadAll,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ExportJob?>(
                _job.Id == jobId
                && string.Equals(_job.ScopeHash, scopeHash, StringComparison.Ordinal)
                && (canReadAll || _job.OwnerId == actorUserId)
                    ? _job
                    : null);

        public Task<bool> ExpireAsync(
            Guid jobId,
            Guid artifactId,
            DateTime observedAtUtc,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _job.Id == jobId
                && _job.ArtifactId == artifactId
                && _job.Expire(observedAtUtc));

        public Task<AdminExportCreateResult> CreateOrReplayAsync(
            AdminExportCreateCommand command,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ExportJob?> TryClaimAsync(
            Guid jobId,
            string leaseOwnerId,
            DateTime claimedAtUtc,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ExportJob?> TryClaimNextAsync(
            string leaseOwnerId,
            DateTime claimedAtUtc,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> RenewLeaseAsync(
            Guid jobId,
            string leaseOwnerId,
            DateTime renewedAtUtc,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> RecordFailureAsync(
            Guid jobId,
            string leaseOwnerId,
            DateTime failedAtUtc,
            bool retryable,
            string failureCode,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> RecordDownloadAsync(
            Guid jobId,
            Guid actorUserId,
            string scopeHash,
            bool canReadAll,
            AuditEventDraft downloadAudit,
            DateTime observedAtUtc,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan elapsed) => _utcNow = _utcNow.Add(elapsed);
    }
}
