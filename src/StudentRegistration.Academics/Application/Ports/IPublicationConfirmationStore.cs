namespace StudentRegistration.Academics.Application.Ports;

public enum PublicationConfirmationStoreOutcome
{
    Published,
    StalePreview,
    IdempotencyKeyReused,
    StorageUnavailable,
}

public sealed record PublicationConfirmationStoreCommand(
    string ScopeKey,
    string ActorReference,
    Guid AggregateId,
    string ExpectedRowVersion,
    string ExpectedContentHash,
    IReadOnlyDictionary<string, string> DependencyVersions,
    string ClientRequestId,
    string CanonicalPayloadHash,
    string Reason,
    string Source,
    string CorrelationId);

public sealed record PublicationConfirmationStoreResult(
    PublicationConfirmationStoreOutcome Outcome,
    Guid? PublishedVersionId = null,
    string? CurrentVersion = null,
    bool IsReplay = false);

public interface IPublicationConfirmationStore
{
    Task<PublicationConfirmationStoreResult> ConfirmAsync(
        PublicationConfirmationStoreCommand command,
        CancellationToken cancellationToken);
}
