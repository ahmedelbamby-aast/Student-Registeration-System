using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.IdentityAccess.Application;

/// <summary>
/// Applies the shared durable abuse policy without exposing raw identifiers to
/// the persistence adapter. The ASP.NET limiter remains a separate coarse
/// network flood-control boundary.
/// </summary>
public sealed class IdentityAbuseControl
{
    private readonly IIdentityAbuseStateStore _store;
    private readonly IIdentityAbuseKeyProvider _keyProvider;
    private readonly IdentitySecurityOptions _options;
    private readonly TimeProvider _timeProvider;

    public IdentityAbuseControl(
        IIdentityAbuseStateStore store,
        IIdentityAbuseKeyProvider keyProvider,
        IdentitySecurityOptions options,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<bool> IsBlockedAsync(
        string operation,
        string subjectScope,
        string? networkScope,
        CancellationToken cancellationToken = default)
    {
        var subjectKey = CreateKey(operation, subjectScope, networkScope);
        var state = await _store.FindAsync(subjectKey, operation, cancellationToken);
        return state?.BlockedUntilUtc is { } blockedUntil
            && blockedUntil > _timeProvider.GetUtcNow();
    }

    public ValueTask<IdentityRateLimitDecision> RecordFailureAsync(
        string operation,
        string subjectScope,
        string? networkScope,
        CancellationToken cancellationToken = default) =>
        IdentityRateLimitPolicies.RecordFailureAsync(
            _store,
            _timeProvider,
            _options,
            operation,
            subjectScope,
            networkScope,
            _keyProvider.GetKey(),
            cancellationToken);

    public ValueTask ResetAsync(
        string operation,
        string subjectScope,
        string? networkScope,
        CancellationToken cancellationToken = default) =>
        IdentityRateLimitPolicies.ResetAsync(
            _store,
            CreateKey(operation, subjectScope, networkScope),
            operation,
            cancellationToken);

    private SubjectKeyHash CreateKey(
        string operation,
        string subjectScope,
        string? networkScope) =>
        IdentityRateLimitPolicies.CreateSubjectKeyHash(
            operation,
            subjectScope,
            networkScope,
            _keyProvider.GetKey());
}
