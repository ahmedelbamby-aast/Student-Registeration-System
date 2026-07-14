using System.Security.Cryptography;
using System.Text;

namespace StudentRegistration.IdentityAccess.Application;

/// <summary>
/// Shared abuse-policy names and privacy-safe helpers. The backing store must
/// perform each failure transition atomically in durable shared storage.
/// </summary>
public static class IdentityRateLimitPolicies
{
    public const string StudentLogin = "identity-student-login";
    public const string StaffLogin = "identity-staff-login";
    public const string StudentActivation = "identity-student-activation";
    public const string RecoveryRequest = "identity-recovery-request";
    public const string RecoveryCompletion = "identity-recovery-completion";
    public const string PasswordChange = "identity-password-change";

    public const string RateLimitedCode = "RATE_LIMITED";
    public const string TelemetryModule = "identity";

    private static readonly HashSet<string> AllowedOperations =
        new(StringComparer.Ordinal)
        {
            "activation",
            "login",
            "password-change",
            "recovery"
        };

    public static SubjectKeyHash CreateSubjectKeyHash(
        string operation,
        string subjectScope,
        string? networkScope,
        ReadOnlyMemory<byte> key)
    {
        var canonicalOperation = CanonicalOperation(operation);
        var canonicalSubject = CanonicalScope(subjectScope, nameof(subjectScope), 512);
        var canonicalNetwork = string.IsNullOrWhiteSpace(networkScope)
            ? "none"
            : CanonicalScope(networkScope, nameof(networkScope), 128);

        if (key.Length < 32)
        {
            throw new ArgumentException(
                "The keyed-hash secret must contain at least 256 bits.",
                nameof(key));
        }

        var input = Encoding.UTF8.GetBytes(
            string.Concat(canonicalOperation, "\n", canonicalSubject, "\n", canonicalNetwork));
        using var algorithm = new HMACSHA256(key.ToArray());
        return new SubjectKeyHash(Convert.ToHexString(algorithm.ComputeHash(input)));
    }

    public static async ValueTask<IdentityRateLimitDecision> RecordFailureAsync(
        IIdentityAbuseStateStore store,
        TimeProvider timeProvider,
        IdentitySecurityOptions options,
        string operation,
        string subjectScope,
        string? networkScope,
        ReadOnlyMemory<byte> key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(options);

        var observedAtUtc = timeProvider.GetUtcNow();
        var subjectKey = CreateSubjectKeyHash(
            operation,
            subjectScope,
            networkScope,
            key);
        var transition = new IdentityAbuseFailure(
            subjectKey,
            CanonicalOperation(operation),
            observedAtUtc,
            options.MaximumFailures,
            options.LockoutDuration);
        var state = await store.RecordFailureAsync(transition, cancellationToken);
        var limited = state.BlockedUntilUtc is { } blockedUntil
            && blockedUntil > observedAtUtc;

        return new IdentityRateLimitDecision(
            limited,
            limited ? RateLimitedCode : null,
            state.BlockedUntilUtc,
            limited
                ? new IdentityAbuseSignal(RateLimitedCode, TelemetryModule, transition.Operation)
                : null);
    }

    public static ValueTask ResetAsync(
        IIdentityAbuseStateStore store,
        SubjectKeyHash subjectKey,
        string operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        return store.ResetAsync(
            subjectKey,
            CanonicalOperation(operation),
            cancellationToken);
    }

    private static string CanonicalOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new ArgumentException("An operation is required.", nameof(operation));
        }

        var normalized = operation.Trim().ToLowerInvariant();
        if (!AllowedOperations.Contains(normalized))
        {
            throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "The operation is not allow-listed.");
        }

        return normalized;
    }

    private static string CanonicalScope(string value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
        {
            throw new ArgumentException(
                "A bounded subject scope is required.",
                parameterName);
        }

        return value.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    }
}

public readonly record struct SubjectKeyHash
{
    public SubjectKeyHash(string value)
    {
        if (value is null
            || value.Length != 64
            || value.Any(character => !char.IsAsciiHexDigit(character)))
        {
            throw new ArgumentException(
                "A SHA-256 hexadecimal digest is required.",
                nameof(value));
        }

        Value = value.ToUpperInvariant();
    }

    public string Value { get; }
}

public sealed record IdentityAbuseFailure(
    SubjectKeyHash SubjectKeyHash,
    string Operation,
    DateTimeOffset ObservedAtUtc,
    int MaximumFailures,
    TimeSpan BlockDuration);

public sealed record IdentityAbuseStateSnapshot(
    int FailureCount,
    DateTimeOffset WindowStartedAtUtc,
    DateTimeOffset? BlockedUntilUtc);

public sealed record IdentityRateLimitDecision(
    bool IsRateLimited,
    string? Code,
    DateTimeOffset? RetryAfterUtc,
    IdentityAbuseSignal? Signal);

public sealed record IdentityAbuseSignal(string Code, string Module, string Operation);

public interface IIdentityAbuseStateStore
{
    ValueTask<IdentityAbuseStateSnapshot?> FindAsync(
        SubjectKeyHash subjectKey,
        string operation,
        CancellationToken cancellationToken);

    ValueTask<IdentityAbuseStateSnapshot> RecordFailureAsync(
        IdentityAbuseFailure failure,
        CancellationToken cancellationToken);

    ValueTask ResetAsync(
        SubjectKeyHash subjectKey,
        string operation,
        CancellationToken cancellationToken);
}
