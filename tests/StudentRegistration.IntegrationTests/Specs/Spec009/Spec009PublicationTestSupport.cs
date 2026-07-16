using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

internal sealed class Spec009PublicationStore : IPublicationConfirmationStore
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);

    public bool FailBeforeCommit { get; set; }
    public int CommitCount { get; private set; }
    public List<string> AuditEntries { get; } = [];

    public void Seed(string scopeKey) => _states[scopeKey] = new("v1", "hash-1");

    public async Task<PublicationConfirmationStoreResult> ConfirmAsync(
        PublicationConfirmationStoreCommand command,
        CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var state = _states[command.ScopeKey];
            if (state.RowVersion != command.ExpectedRowVersion
                || state.ContentHash != command.ExpectedContentHash)
            {
                return new(
                    PublicationConfirmationStoreOutcome.StalePreview,
                    CurrentVersion: state.RowVersion);
            }

            if (FailBeforeCommit)
            {
                return new(PublicationConfirmationStoreOutcome.StorageUnavailable);
            }

            _states[command.ScopeKey] = state with { RowVersion = "v1-next" };
            AuditEntries.Add(command.Reason);
            CommitCount++;
            return new(
                PublicationConfirmationStoreOutcome.Published,
                Guid.NewGuid());
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record State(string RowVersion, string ContentHash);
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
