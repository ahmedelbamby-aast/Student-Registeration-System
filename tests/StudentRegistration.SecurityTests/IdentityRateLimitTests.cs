using System.Collections.Concurrent;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SecurityTests;

public sealed class IdentityRateLimitTests
{
    [Fact]
    public async Task Two_replicas_share_the_same_hmac_subject_lockout_state()
    {
        var store = new SharedAbuseStore();
        var key = new FixedAbuseKeyProvider();
        var time = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero));
        var options = new IdentitySecurityOptions();
        var replicas = new[]
        {
            new IdentityAbuseControl(store, key, options, time),
            new IdentityAbuseControl(store, key, options, time)
        };

        for (var attempt = 0; attempt < 4; attempt++)
        {
            var decision = await replicas[attempt % 2].RecordFailureAsync(
                "login",
                "AI2600001",
                "192.0.2.10");
            Assert.False(decision.IsRateLimited);
        }

        var blocked = await replicas[0].RecordFailureAsync(
            "login",
            "AI2600001",
            "192.0.2.10");
        Assert.True(blocked.IsRateLimited);
        Assert.True(await replicas[1].IsBlockedAsync(
            "login",
            "AI2600001",
            "192.0.2.10"));
        Assert.All(store.StoredKeys, value =>
            Assert.DoesNotContain("AI2600001", value, StringComparison.Ordinal));

        time.Advance(options.LockoutDuration.Add(TimeSpan.FromSeconds(1)));
        Assert.False(await replicas[1].IsBlockedAsync(
            "login",
            "AI2600001",
            "192.0.2.10"));
    }

    [Fact]
    public void Identity_security_options_pin_the_demo_password_and_proof_baseline()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/IdentitySecurityOptions.cs");

        RepositoryFiles.ContainsAll(
            source,
            "PasswordHasherCompatibilityMode.IdentityV3",
            "100_000",
            "MinimumPasswordLength = 15",
            "MaximumPasswordLength = 128",
            "MaximumFailures = 5",
            "TimeSpan.FromMinutes(5)",
            "TimeSpan.FromMinutes(15)",
            "PasswordBlocklistVersion",
            "Production");
        Assert.DoesNotContain("RequireDigit = true", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireUppercase = true", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireNonAlphanumeric = true", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Password_policy_executes_length_blocklist_context_and_no_composition_rules()
    {
        var options = new IdentitySecurityOptions();

        Assert.True(options.ValidatePassword("all lowercase words with spaces").IsAccepted);
        Assert.True(options.ValidatePassword("رمز مرور تجريبي طويل وآمن").IsAccepted);
        Assert.Equal(
            "PASSWORD_TOO_SHORT",
            options.ValidatePassword(new string('x', 14)).ErrorCode);
        Assert.Equal(
            "PASSWORD_TOO_LONG",
            options.ValidatePassword(new string('x', 129)).ErrorCode);
        Assert.Equal(
            "PASSWORD_BLOCKED",
            options.ValidatePassword("passwordpassword").ErrorCode);
        Assert.Equal(
            "PASSWORD_CONTEXT_SPECIFIC",
            options.ValidatePassword(
                "student-20260001-secure",
                ["20260001"]).ErrorCode);
    }

    [Fact]
    public void Abuse_policy_uses_shared_hashed_keys_and_generic_bounded_signals()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/IdentityRateLimitPolicies.cs");

        RepositoryFiles.ContainsAll(
            source,
            "IdentityRateLimitPolicies",
            "SubjectKeyHash",
            "HMACSHA256",
            "MaximumFailures",
            "RecordFailureAsync",
            "RATE_LIMITED",
            "identity");
        Assert.DoesNotContain("UniversityId", source, StringComparison.Ordinal);
        Assert.DoesNotContain("UserName", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IPAddress", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ILogger", source, StringComparison.Ordinal);
    }

    private sealed class FixedAbuseKeyProvider : IIdentityAbuseKeyProvider
    {
        private static readonly byte[] Key = Enumerable.Range(1, 32)
            .Select(value => (byte)value)
            .ToArray();

        public ReadOnlyMemory<byte> GetKey() => Key;
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }

    private sealed class SharedAbuseStore : IIdentityAbuseStateStore
    {
        private readonly ConcurrentDictionary<string, IdentityAbuseStateSnapshot> _states =
            new(StringComparer.Ordinal);

        public IReadOnlyCollection<string> StoredKeys => _states.Keys.ToArray();

        public ValueTask<IdentityAbuseStateSnapshot?> FindAsync(
            SubjectKeyHash subjectKey,
            string operation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(
                _states.TryGetValue(StorageKey(subjectKey, operation), out var state)
                    ? state
                    : null);
        }

        public ValueTask<IdentityAbuseStateSnapshot> RecordFailureAsync(
            IdentityAbuseFailure failure,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var storageKey = StorageKey(failure.SubjectKeyHash, failure.Operation);
            var state = _states.AddOrUpdate(
                storageKey,
                _ => Snapshot(1, failure),
                (_, current) => Snapshot(current.FailureCount + 1, failure));
            return ValueTask.FromResult(state);
        }

        public ValueTask ResetAsync(
            SubjectKeyHash subjectKey,
            string operation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _states.TryRemove(StorageKey(subjectKey, operation), out _);
            return ValueTask.CompletedTask;
        }

        private static IdentityAbuseStateSnapshot Snapshot(
            int failureCount,
            IdentityAbuseFailure failure) =>
            new(
                failureCount,
                failure.ObservedAtUtc,
                failureCount >= failure.MaximumFailures
                    ? failure.ObservedAtUtc.Add(failure.BlockDuration)
                    : null);

        private static string StorageKey(SubjectKeyHash key, string operation) =>
            $"{operation}:{key.Value}";
    }
}
