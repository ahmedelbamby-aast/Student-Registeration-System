using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.Academics.Application;

public enum PublicationScopeKind
{
    Catalogue = 1,
    Policy = 2,
}

public sealed record PublicationPreviewRequest(
    PublicationScopeKind ScopeKind,
    string ScopeCode,
    Guid AggregateId,
    string ActorReference,
    string ExpectedRowVersion,
    string ContentHash,
    IReadOnlyDictionary<string, string> DependencyVersions,
    DateTimeOffset ExpiresAtUtc);

public sealed record PublicationPreview(
    string Token,
    DateTimeOffset ExpiresAtUtc);

public sealed record PublicationConfirmationRequest(
    PublicationScopeKind ScopeKind,
    string ScopeCode,
    Guid AggregateId,
    string ActorReference,
    string ExpectedRowVersion,
    string ContentHash,
    IReadOnlyDictionary<string, string> DependencyVersions,
    string PreviewToken,
    string ClientRequestId,
    string Reason,
    string Source,
    string CorrelationId);

public enum PublicationConfirmationOutcome
{
    Published,
    StalePreview,
    IdempotencyKeyReused,
    StorageUnavailable,
}

public sealed record PublicationConfirmationResult(
    PublicationConfirmationOutcome Outcome,
    Guid? PublishedVersionId = null,
    string? CurrentVersion = null,
    bool IsReplay = false);

public sealed class PublicationConfirmationService
{
    private readonly IPublicationConfirmationStore _store;
    private readonly TimeProvider _timeProvider;
    private readonly byte[] _previewKey;

    public PublicationConfirmationService(
        IPublicationConfirmationStore store,
        TimeProvider timeProvider,
        byte[] previewKey)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        ArgumentNullException.ThrowIfNull(previewKey);
        if (previewKey.Length < 32)
        {
            throw new ArgumentException(
                "The preview signing key must contain at least 32 bytes.",
                nameof(previewKey));
        }

        _previewKey = previewKey.ToArray();
    }

    public PublicationPreview CreatePreview(PublicationPreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateScope(request.ScopeKind, request.ScopeCode, request.AggregateId);
        var actor = Required(request.ActorReference, nameof(request.ActorReference));
        var expectedVersion = Required(
            request.ExpectedRowVersion,
            nameof(request.ExpectedRowVersion));
        var contentHash = Required(request.ContentHash, nameof(request.ContentHash));
        var dependencies = NormalizeDependencies(request.DependencyVersions);
        if (request.ExpiresAtUtc.Offset != TimeSpan.Zero
            || request.ExpiresAtUtc <= _timeProvider.GetUtcNow())
        {
            throw new ArgumentException(
                "A future UTC preview expiry is required.",
                nameof(request.ExpiresAtUtc));
        }

        var payload = new PreviewPayload(
            request.ScopeKind,
            NormalizeCode(request.ScopeCode),
            request.AggregateId,
            actor,
            expectedVersion,
            contentHash,
            dependencies,
            request.ExpiresAtUtc);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var signature = HMACSHA256.HashData(_previewKey, bytes);
        return new(
            $"{Base64Url(bytes)}.{Base64Url(signature)}",
            request.ExpiresAtUtc);
    }

    public async Task<PublicationConfirmationResult> ConfirmAsync(
        PublicationConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateScope(request.ScopeKind, request.ScopeCode, request.AggregateId);
        var normalizedScope = NormalizeCode(request.ScopeCode);
        var actor = Required(request.ActorReference, nameof(request.ActorReference));
        var expectedVersion = Required(
            request.ExpectedRowVersion,
            nameof(request.ExpectedRowVersion));
        var contentHash = Required(request.ContentHash, nameof(request.ContentHash));
        var dependencies = NormalizeDependencies(request.DependencyVersions);
        var clientRequestId = Required(
            request.ClientRequestId,
            nameof(request.ClientRequestId));
        var reason = Required(request.Reason, nameof(request.Reason));
        var source = Required(request.Source, nameof(request.Source));
        var correlationId = Required(request.CorrelationId, nameof(request.CorrelationId));

        if (!TryReadPreview(request.PreviewToken, out var preview)
            || preview.ScopeKind != request.ScopeKind
            || !string.Equals(preview.ScopeCode, normalizedScope, StringComparison.Ordinal)
            || preview.AggregateId != request.AggregateId
            || !string.Equals(preview.ActorReference, actor, StringComparison.Ordinal)
            || !string.Equals(
                preview.ExpectedRowVersion,
                expectedVersion,
                StringComparison.Ordinal)
            || !string.Equals(preview.ContentHash, contentHash, StringComparison.Ordinal)
            || !Equal(preview.DependencyVersions, dependencies)
            || preview.ExpiresAtUtc <= _timeProvider.GetUtcNow())
        {
            return new(PublicationConfirmationOutcome.StalePreview);
        }

        var scopeKey = $"{request.ScopeKind.ToString().ToLowerInvariant()}:{normalizedScope}";
        var canonicalPayloadHash = ComputePayloadHash(new
        {
            request.ScopeKind,
            ScopeCode = normalizedScope,
            request.AggregateId,
            ActorReference = actor,
            ExpectedRowVersion = expectedVersion,
            ContentHash = contentHash,
            DependencyVersions = dependencies,
            PreviewToken = request.PreviewToken,
            Reason = reason,
            Source = source,
        });
        var result = await _store.ConfirmAsync(
            new PublicationConfirmationStoreCommand(
                scopeKey,
                actor,
                request.AggregateId,
                expectedVersion,
                contentHash,
                dependencies,
                clientRequestId,
                canonicalPayloadHash,
                reason,
                source,
                correlationId),
            cancellationToken);

        return new(
            result.Outcome switch
            {
                PublicationConfirmationStoreOutcome.Published =>
                    PublicationConfirmationOutcome.Published,
                PublicationConfirmationStoreOutcome.StalePreview =>
                    PublicationConfirmationOutcome.StalePreview,
                PublicationConfirmationStoreOutcome.IdempotencyKeyReused =>
                    PublicationConfirmationOutcome.IdempotencyKeyReused,
                _ => PublicationConfirmationOutcome.StorageUnavailable,
            },
            result.PublishedVersionId,
            result.CurrentVersion,
            result.IsReplay);
    }

    private bool TryReadPreview(string token, out PreviewPayload payload)
    {
        payload = null!;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 2
            || !TryBase64Url(parts[0], out var bytes)
            || !TryBase64Url(parts[1], out var signature))
        {
            return false;
        }

        var expected = HMACSHA256.HashData(_previewKey, bytes);
        if (!CryptographicOperations.FixedTimeEquals(expected, signature))
        {
            return false;
        }

        try
        {
            payload = JsonSerializer.Deserialize<PreviewPayload>(bytes)!;
            return payload is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void ValidateScope(
        PublicationScopeKind scopeKind,
        string scopeCode,
        Guid aggregateId)
    {
        if (!Enum.IsDefined(scopeKind))
        {
            throw new ArgumentOutOfRangeException(nameof(scopeKind));
        }

        _ = NormalizeCode(scopeCode);
        if (aggregateId == Guid.Empty)
        {
            throw new ArgumentException(
                "A publication aggregate identifier is required.",
                nameof(aggregateId));
        }
    }

    private static SortedDictionary<string, string> NormalizeDependencies(
        IReadOnlyDictionary<string, string> dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);
        var normalized = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in dependencies)
        {
            normalized.Add(
                NormalizeCode(key),
                Required(value, nameof(dependencies)));
        }

        return normalized;
    }

    private static bool Equal(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count
        && left.All(pair =>
            right.TryGetValue(pair.Key, out var value)
            && string.Equals(pair.Value, value, StringComparison.Ordinal));

    private static string ComputePayloadHash<T>(T payload) =>
        $"sha256:{Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(payload)))}";

    private static string Required(string? value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A non-empty value is required.", parameterName)
            : value.Trim();

    private static string NormalizeCode(string? value) =>
        Required(value, nameof(value)).Normalize().ToUpperInvariant();

    private static string Base64Url(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static bool TryBase64Url(string value, out byte[] bytes)
    {
        try
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
            bytes = Convert.FromBase64String(padded);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private sealed record PreviewPayload(
        PublicationScopeKind ScopeKind,
        string ScopeCode,
        Guid AggregateId,
        string ActorReference,
        string ExpectedRowVersion,
        string ContentHash,
        IReadOnlyDictionary<string, string> DependencyVersions,
        DateTimeOffset ExpiresAtUtc);
}
