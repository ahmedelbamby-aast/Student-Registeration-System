using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class PublicationRaceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly byte[] PreviewKey =
        "spec-009-local-preview-key-32-bytes"u8.ToArray();

    [Fact]
    public async Task Preview_is_bound_to_actor_scope_content_dependencies_and_expiry()
    {
        var store = new InMemoryPublicationStore();
        store.Seed("catalogue:AI-DS", "v1", "hash-1", new Dictionary<string, string>
        {
            ["term"] = "term-v1",
        });
        var service = Service(store);
        var preview = service.CreatePreview(PreviewRequest());

        var wrongActor = await service.ConfirmAsync(
            Confirm(preview.Token) with { ActorReference = "admin-2" });
        var wrongDependency = await service.ConfirmAsync(
            Confirm(preview.Token) with
            {
                DependencyVersions = new Dictionary<string, string>
                {
                    ["term"] = "term-v2",
                }
            });
        var expiredService = Service(store, Now.AddMinutes(11));
        var expired = await expiredService.ConfirmAsync(Confirm(preview.Token));

        Assert.Equal(PublicationConfirmationOutcome.StalePreview, wrongActor.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.StalePreview, wrongDependency.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.StalePreview, expired.Outcome);
        Assert.Equal(0, store.CommitCount);
        Assert.Empty(store.AuditEntries);
    }

    [Fact]
    public async Task Edited_draft_old_preview_rejects_and_same_key_replays_without_audit()
    {
        var store = new InMemoryPublicationStore();
        store.Seed("catalogue:AI-DS", "v1", "hash-1", EmptyDependencies());
        var service = Service(store);
        var preview = service.CreatePreview(PreviewRequest(dependencies: EmptyDependencies()));
        store.Edit("catalogue:AI-DS", "v2", "hash-2");
        var command = Confirm(preview.Token, dependencies: EmptyDependencies());

        var first = await service.ConfirmAsync(command);
        var replay = await service.ConfirmAsync(command);

        Assert.Equal(PublicationConfirmationOutcome.StalePreview, first.Outcome);
        Assert.False(first.IsReplay);
        Assert.Equal(PublicationConfirmationOutcome.StalePreview, replay.Outcome);
        Assert.True(replay.IsReplay);
        Assert.Equal("v2", replay.CurrentVersion);
        Assert.Equal(0, store.CommitCount);
        Assert.Empty(store.AuditEntries);
    }

    [Fact]
    public async Task Same_key_with_different_canonical_payload_is_rejected()
    {
        var store = new InMemoryPublicationStore();
        store.Seed("catalogue:AI-DS", "v1", "hash-1", EmptyDependencies());
        var service = Service(store);
        var preview = service.CreatePreview(PreviewRequest(dependencies: EmptyDependencies()));
        var command = Confirm(preview.Token, dependencies: EmptyDependencies());

        var published = await service.ConfirmAsync(command);
        var mismatch = await service.ConfirmAsync(command with { Reason = "different reason" });

        Assert.Equal(PublicationConfirmationOutcome.Published, published.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.IdempotencyKeyReused, mismatch.Outcome);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
    }

    [Fact]
    public async Task Concurrent_current_confirmations_have_exactly_one_winner()
    {
        var store = new InMemoryPublicationStore();
        store.Seed("policy:AI-DS", "v1", "hash-1", new Dictionary<string, string>
        {
            ["term"] = "term-v1",
        });
        var service = Service(store);
        var preview = service.CreatePreview(PreviewRequest(
            scopeKind: PublicationScopeKind.Policy,
            dependencies: new Dictionary<string, string> { ["term"] = "term-v1" }));

        var results = await Task.WhenAll(
            service.ConfirmAsync(Confirm(
                preview.Token,
                scopeKind: PublicationScopeKind.Policy,
                clientRequestId: "publish-a")),
            service.ConfirmAsync(Confirm(
                preview.Token,
                scopeKind: PublicationScopeKind.Policy,
                clientRequestId: "publish-b")));

        Assert.Single(results, result =>
            result.Outcome == PublicationConfirmationOutcome.Published);
        Assert.Single(results, result =>
            result.Outcome == PublicationConfirmationOutcome.StalePreview);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
        Assert.All(store.LockOrders, order => Assert.Equal(["policy:AI-DS"], order));
    }

    [Fact]
    public async Task Same_key_same_payload_replays_one_version_and_one_audit()
    {
        var store = new InMemoryPublicationStore();
        store.Seed("catalogue:AI-DS", "v1", "hash-1", EmptyDependencies());
        var service = Service(store);
        var preview = service.CreatePreview(PreviewRequest(dependencies: EmptyDependencies()));
        var command = Confirm(preview.Token, dependencies: EmptyDependencies());

        var published = await service.ConfirmAsync(command);
        var replay = await service.ConfirmAsync(command);

        Assert.Equal(PublicationConfirmationOutcome.Published, published.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.Published, replay.Outcome);
        Assert.True(replay.IsReplay);
        Assert.Equal(published.PublishedVersionId, replay.PublishedVersionId);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
    }

    [Fact]
    public async Task Audit_failure_rolls_back_version_audit_and_idempotency_claim()
    {
        var store = new InMemoryPublicationStore { FailBeforeCommit = true };
        store.Seed("catalogue:AI-DS", "v1", "hash-1", EmptyDependencies());
        var service = Service(store);
        var preview = service.CreatePreview(PreviewRequest(dependencies: EmptyDependencies()));
        var command = Confirm(preview.Token, dependencies: EmptyDependencies());

        var failed = await service.ConfirmAsync(command);
        store.FailBeforeCommit = false;
        var retried = await service.ConfirmAsync(command);

        Assert.Equal(PublicationConfirmationOutcome.StorageUnavailable, failed.Outcome);
        Assert.Equal(PublicationConfirmationOutcome.Published, retried.Outcome);
        Assert.False(retried.IsReplay);
        Assert.Equal(1, store.CommitCount);
        Assert.Single(store.AuditEntries);
    }

    private static PublicationConfirmationService Service(
        IPublicationConfirmationStore store,
        DateTimeOffset? now = null) =>
        new(
            store,
            new FixedTimeProvider(now ?? Now),
            PreviewKey);

    private static PublicationPreviewRequest PreviewRequest(
        PublicationScopeKind scopeKind = PublicationScopeKind.Catalogue,
        IReadOnlyDictionary<string, string>? dependencies = null) =>
        new(
            scopeKind,
            "AI-DS",
            Guid.Parse("00000000-0000-0000-0000-000000000009"),
            "admin-1",
            "v1",
            "hash-1",
            dependencies ?? new Dictionary<string, string> { ["term"] = "term-v1" },
            Now.AddMinutes(10));

    private static PublicationConfirmationRequest Confirm(
        string token,
        PublicationScopeKind scopeKind = PublicationScopeKind.Catalogue,
        string clientRequestId = "publish-1",
        IReadOnlyDictionary<string, string>? dependencies = null) =>
        new(
            scopeKind,
            "AI-DS",
            Guid.Parse("00000000-0000-0000-0000-000000000009"),
            "admin-1",
            "v1",
            "hash-1",
            dependencies ?? new Dictionary<string, string> { ["term"] = "term-v1" },
            token,
            clientRequestId,
            "Publish approved draft",
            "SRC-DATA-SCIENCE",
            "correlation-1");

    private static IReadOnlyDictionary<string, string> EmptyDependencies() =>
        new Dictionary<string, string>();

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InMemoryPublicationStore : IPublicationConfirmationStore
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Stored> _idempotency = new(StringComparer.Ordinal);

        public bool FailBeforeCommit { get; set; }
        public int CommitCount { get; private set; }
        public List<string> AuditEntries { get; } = [];
        public List<IReadOnlyList<string>> LockOrders { get; } = [];

        public void Seed(
            string scopeKey,
            string rowVersion,
            string contentHash,
            IReadOnlyDictionary<string, string> dependencies) =>
            _states[scopeKey] = new(rowVersion, contentHash, Copy(dependencies));

        public void Edit(string scopeKey, string rowVersion, string contentHash)
        {
            var state = _states[scopeKey];
            _states[scopeKey] = state with
            {
                RowVersion = rowVersion,
                ContentHash = contentHash,
            };
        }

        public async Task<PublicationConfirmationStoreResult> ConfirmAsync(
            PublicationConfirmationStoreCommand command,
            CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                LockOrders.Add([command.ScopeKey]);
                var idempotencyKey =
                    $"{command.ActorReference}|{command.ScopeKey}|{command.ClientRequestId}";
                if (_idempotency.TryGetValue(idempotencyKey, out var existing))
                {
                    return existing.PayloadHash == command.CanonicalPayloadHash
                        ? existing.Result with { IsReplay = true }
                        : new(PublicationConfirmationStoreOutcome.IdempotencyKeyReused);
                }

                var state = _states[command.ScopeKey];
                if (state.RowVersion != command.ExpectedRowVersion
                    || state.ContentHash != command.ExpectedContentHash
                    || !Equal(state.Dependencies, command.DependencyVersions))
                {
                    var stale = new PublicationConfirmationStoreResult(
                        PublicationConfirmationStoreOutcome.StalePreview,
                        CurrentVersion: state.RowVersion);
                    _idempotency[idempotencyKey] = new(command.CanonicalPayloadHash, stale);
                    return stale;
                }

                if (FailBeforeCommit)
                {
                    return new(PublicationConfirmationStoreOutcome.StorageUnavailable);
                }

                var versionId = Guid.NewGuid();
                _states[command.ScopeKey] = state with
                {
                    RowVersion = $"{state.RowVersion}-next",
                };
                AuditEntries.Add($"{command.ActorReference}|{command.Reason}|{command.Source}");
                CommitCount++;
                var result = new PublicationConfirmationStoreResult(
                    PublicationConfirmationStoreOutcome.Published,
                    versionId);
                _idempotency[idempotencyKey] = new(command.CanonicalPayloadHash, result);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static Dictionary<string, string> Copy(
            IReadOnlyDictionary<string, string> source) =>
            source.ToDictionary(
                pair => pair.Key.Trim().ToUpperInvariant(),
                pair => pair.Value,
                StringComparer.Ordinal);

        private static bool Equal(
            IReadOnlyDictionary<string, string> left,
            IReadOnlyDictionary<string, string> right) =>
            left.Count == right.Count
            && left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);

        private sealed record State(
            string RowVersion,
            string ContentHash,
            IReadOnlyDictionary<string, string> Dependencies);

        private sealed record Stored(
            string PayloadHash,
            PublicationConfirmationStoreResult Result);
    }
}
