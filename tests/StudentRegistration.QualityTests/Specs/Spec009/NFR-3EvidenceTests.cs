using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec009;

public sealed class NFR_3EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-009-NFR-3.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec009/NFR-3EvidenceTests.cs";
    private static readonly byte[] PreviewKey =
        Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();

    [Fact]
    public async Task Publication_has_one_winner_replay_and_full_fault_rollback()
    {
        var clock = new FixedTimeProvider(
            new DateTimeOffset(2026, 7, 16, 12, 0, 0, TimeSpan.Zero));
        var store = new AtomicPublicationStore();
        var service = new PublicationConfirmationService(store, clock, PreviewKey);
        var aggregateId = Guid.Parse("00000000-0000-0000-0000-000000009301");
        var preview = service.CreatePreview(PreviewRequest(aggregateId, clock));

        var firstRequest = ConfirmationRequest(
            aggregateId,
            preview.Token,
            "NFR3-CLIENT-1");
        var secondRequest = ConfirmationRequest(
            aggregateId,
            preview.Token,
            "NFR3-CLIENT-2");
        var results = await Task.WhenAll(
            service.ConfirmAsync(firstRequest),
            service.ConfirmAsync(secondRequest));

        Assert.Single(
            results,
            result => result.Outcome is PublicationConfirmationOutcome.Published);
        Assert.Single(
            results,
            result => result.Outcome is PublicationConfirmationOutcome.StalePreview);
        Assert.Equal(1, store.PublicationCount);
        Assert.Equal(1, store.AuditCount);

        var winner = results.Single(
            result => result.Outcome is PublicationConfirmationOutcome.Published);
        var replayRequest = winner.PublishedVersionId == results[0].PublishedVersionId
            ? firstRequest
            : secondRequest;
        var replay = await service.ConfirmAsync(replayRequest);
        Assert.Equal(PublicationConfirmationOutcome.Published, replay.Outcome);
        Assert.True(replay.IsReplay);
        Assert.Equal(1, store.PublicationCount);
        Assert.Equal(1, store.AuditCount);

        var faultStore = new AtomicPublicationStore { FailAudit = true };
        var faultService = new PublicationConfirmationService(
            faultStore,
            clock,
            PreviewKey);
        var faultPreview = faultService.CreatePreview(PreviewRequest(aggregateId, clock));
        var fault = await faultService.ConfirmAsync(
            ConfirmationRequest(
                aggregateId,
                faultPreview.Token,
                "NFR3-FAULT"));
        Assert.Equal(PublicationConfirmationOutcome.StorageUnavailable, fault.Outcome);
        Assert.Equal(0, faultStore.PublicationCount);
        Assert.Equal(0, faultStore.AuditCount);
    }

    [Fact]
    public void Evidence_is_source_bound_and_records_atomic_fault_behavior()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-009 NFR-3 Transactional Publication Evidence",
            "one winner",
            "same-payload replay",
            "audit failure",
            "0 publication rows",
            "0 audit rows",
            "scope lock",
            "1 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static PublicationPreviewRequest PreviewRequest(
        Guid aggregateId,
        TimeProvider clock) =>
        new(
            PublicationScopeKind.Catalogue,
            "AI-DS",
            aggregateId,
            "admin:nfr-3",
            "draft-rv-1",
            "sha256:NFR3",
            new Dictionary<string, string> { ["POLICY"] = "DEMO-POC-2026.1" },
            clock.GetUtcNow().AddMinutes(10));

    private static PublicationConfirmationRequest ConfirmationRequest(
        Guid aggregateId,
        string previewToken,
        string clientRequestId) =>
        new(
            PublicationScopeKind.Catalogue,
            "AI-DS",
            aggregateId,
            "admin:nfr-3",
            "draft-rv-1",
            "sha256:NFR3",
            new Dictionary<string, string> { ["POLICY"] = "DEMO-POC-2026.1" },
            previewToken,
            clientRequestId,
            "Publish the validated NFR-3 catalogue.",
            "SPEC-009-NFR-3",
            $"nfr-3-{clientRequestId}");

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class AtomicPublicationStore : IPublicationConfirmationStore
    {
        private readonly SemaphoreSlim _scopeLock = new(1, 1);
        private readonly Dictionary<string, ReplayRecord> _replays =
            new(StringComparer.Ordinal);
        private string _currentVersion = "draft-rv-1";

        public bool FailAudit { get; init; }
        public int PublicationCount { get; private set; }
        public int AuditCount { get; private set; }

        public async Task<PublicationConfirmationStoreResult> ConfirmAsync(
            PublicationConfirmationStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            await _scopeLock.WaitAsync(cancellationToken);
            try
            {
                var replayKey = $"{command.ActorReference}|{command.ScopeKey}|{command.ClientRequestId}";
                if (_replays.TryGetValue(replayKey, out var replay))
                {
                    return replay.PayloadHash == command.CanonicalPayloadHash
                        ? replay.Result with { IsReplay = true }
                        : new(PublicationConfirmationStoreOutcome.IdempotencyKeyReused);
                }

                if (_currentVersion != command.ExpectedRowVersion)
                {
                    return new(
                        PublicationConfirmationStoreOutcome.StalePreview,
                        CurrentVersion: _currentVersion);
                }

                if (FailAudit)
                {
                    return new(PublicationConfirmationStoreOutcome.StorageUnavailable);
                }

                var versionId = Guid.NewGuid();
                PublicationCount++;
                AuditCount++;
                _currentVersion = "draft-rv-2";
                var result = new PublicationConfirmationStoreResult(
                    PublicationConfirmationStoreOutcome.Published,
                    versionId,
                    _currentVersion);
                _replays.Add(
                    replayKey,
                    new(command.CanonicalPayloadHash, result));
                return result;
            }
            finally
            {
                _scopeLock.Release();
            }
        }

        private sealed record ReplayRecord(
            string PayloadHash,
            PublicationConfirmationStoreResult Result);
    }
}
