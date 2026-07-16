using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

internal sealed class Spec009PublicationStore : IPublicationConfirmationStore
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Stored> _idempotency = new(StringComparer.Ordinal);

    public bool FailBeforeCommit { get; set; }
    public int CommitCount { get; private set; }
    public List<string> AuditEntries { get; } = [];

    public void Seed(
        string scopeKey,
        string rowVersion = "v1",
        string contentHash = "hash-1") =>
        _states[scopeKey] = new(rowVersion, contentHash);

    public void Edit(string scopeKey, string rowVersion, string contentHash) =>
        _states[scopeKey] = new(rowVersion, contentHash);

    public async Task<PublicationConfirmationStoreResult> ConfirmAsync(
        PublicationConfirmationStoreCommand command,
        CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var key = $"{command.ActorReference}|{command.ScopeKey}|{command.ClientRequestId}";
            if (_idempotency.TryGetValue(key, out var stored))
            {
                return stored.PayloadHash == command.CanonicalPayloadHash
                    ? stored.Result with { IsReplay = true }
                    : new(PublicationConfirmationStoreOutcome.IdempotencyKeyReused);
            }

            var state = _states[command.ScopeKey];
            if (state.RowVersion != command.ExpectedRowVersion
                || state.ContentHash != command.ExpectedContentHash)
            {
                var stale = new PublicationConfirmationStoreResult(
                    PublicationConfirmationStoreOutcome.StalePreview,
                    CurrentVersion: state.RowVersion);
                _idempotency[key] = new(command.CanonicalPayloadHash, stale);
                return stale;
            }

            if (FailBeforeCommit)
            {
                return new(PublicationConfirmationStoreOutcome.StorageUnavailable);
            }

            var versionId = Guid.NewGuid();
            _states[command.ScopeKey] = state with { RowVersion = $"{state.RowVersion}-next" };
            AuditEntries.Add(
                $"{command.ActorReference}|{command.Reason}|{command.Source}|{command.CorrelationId}");
            CommitCount++;
            var result = new PublicationConfirmationStoreResult(
                PublicationConfirmationStoreOutcome.Published,
                versionId);
            _idempotency[key] = new(command.CanonicalPayloadHash, result);
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record State(string RowVersion, string ContentHash);
    private sealed record Stored(
        string PayloadHash,
        PublicationConfirmationStoreResult Result);
}

internal static class Spec009PublicationScenario
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly byte[] PreviewKey =
        "spec-009-local-preview-key-32-bytes"u8.ToArray();

    public static PublicationConfirmationService Service(
        IPublicationConfirmationStore store) =>
        new(store, new FixedTimeProvider(Now), PreviewKey);

    public static PublicationPreviewRequest PreviewRequest(
        PublicationScopeKind scopeKind = PublicationScopeKind.Catalogue) =>
        new(
            scopeKind,
            "AI-DS",
            Guid.Parse("00000000-0000-0000-0000-000000000009"),
            "admin-1",
            "v1",
            "hash-1",
            new Dictionary<string, string>(),
            Now.AddMinutes(10));

    public static PublicationConfirmationRequest ConfirmationRequest(
        string token,
        PublicationScopeKind scopeKind = PublicationScopeKind.Catalogue,
        string clientRequestId = "publish-1") =>
        new(
            scopeKind,
            "AI-DS",
            Guid.Parse("00000000-0000-0000-0000-000000000009"),
            "admin-1",
            "v1",
            "hash-1",
            new Dictionary<string, string>(),
            token,
            clientRequestId,
            "Publish approved draft",
            "SRC-DATA-SCIENCE",
            "correlation-1");

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
