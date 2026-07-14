namespace StudentRegistration.IdentityAccess.Domain;

public static class IdentityImportStates
{
    public const string Uploaded = "uploaded";
    public const string Invalid = "invalid";
    public const string Validated = "validated";
    public const string Published = "published";
    public const string Failed = "failed";

    public static bool IsValidationResult(string state) =>
        string.Equals(state, Invalid, StringComparison.Ordinal) ||
        string.Equals(state, Validated, StringComparison.Ordinal);
}

/// <summary>
/// Bounded provenance and idempotency record for a pre-provisioned identity import.
/// Raw rows and credentials deliberately remain outside this aggregate.
/// </summary>
public sealed class IdentityImportBatch
{
    private const int MaximumSummaryLength = 16_000;

    private IdentityImportBatch()
    {
    }

    public IdentityImportBatch(
        Guid id,
        Guid requestedByUserId,
        string clientRequestId,
        string sourceName,
        string sourceHash,
        DateTime importedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An import identifier is required.", nameof(id));
        }

        if (requestedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A requesting user identifier is required.",
                nameof(requestedByUserId));
        }

        Id = id;
        RequestedByUserId = requestedByUserId;
        ClientRequestId = IdentityDomainGuard.Required(
            clientRequestId,
            nameof(clientRequestId),
            100);
        SourceName = IdentityDomainGuard.Required(sourceName, nameof(sourceName), 255);
        SourceHash = IdentityDomainGuard.Required(sourceHash, nameof(sourceHash), 200);
        State = IdentityImportStates.Uploaded;
        ErrorSummaryJson = "[]";
        ImportedAtUtc = importedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public string ClientRequestId { get; private set; } = string.Empty;
    public string SourceName { get; private set; } = string.Empty;
    public string SourceHash { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ErrorSummaryJson { get; private set; } = "[]";
    public string? ResultSummaryJson { get; private set; }
    public DateTime ImportedAtUtc { get; private set; }
    public byte[] Version { get; private set; } = [];

    public void RecordValidation(string state, string errorSummaryJson)
    {
        if (!string.Equals(State, IdentityImportStates.Uploaded, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only an uploaded import can be validated.");
        }

        if (!IdentityImportStates.IsValidationResult(state))
        {
            throw new ArgumentException(
                "Validation must produce the invalid or validated state.",
                nameof(state));
        }

        State = state;
        ErrorSummaryJson = IdentityDomainGuard.Required(
            errorSummaryJson,
            nameof(errorSummaryJson),
            MaximumSummaryLength);
    }

    public void Publish(string resultSummaryJson)
    {
        if (!string.Equals(State, IdentityImportStates.Validated, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only a validated import can be published.");
        }

        ResultSummaryJson = IdentityDomainGuard.Required(
            resultSummaryJson,
            nameof(resultSummaryJson),
            MaximumSummaryLength);
        State = IdentityImportStates.Published;
    }

    public void Fail(string errorSummaryJson)
    {
        if (string.Equals(State, IdentityImportStates.Published, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("A published import cannot fail.");
        }

        ErrorSummaryJson = IdentityDomainGuard.Required(
            errorSummaryJson,
            nameof(errorSummaryJson),
            MaximumSummaryLength);
        State = IdentityImportStates.Failed;
    }
}
