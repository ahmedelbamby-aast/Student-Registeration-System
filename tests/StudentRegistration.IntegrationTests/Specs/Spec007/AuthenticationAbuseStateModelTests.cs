using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class AuthenticationAbuseStateModelTests
{
    [Fact]
    public void Abuse_state_has_operation_scoped_hash_and_shared_lockout_state()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/AuthenticationAbuseState.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class AuthenticationAbuseState",
            "public string SubjectKeyHash { get;",
            "public string Operation { get;",
            "public int FailureCount { get;",
            "public DateTime WindowStartedAtUtc { get;",
            "public DateTime? LockedUntilUtc { get;",
            "public byte[] Version { get;",
            "RecordFailure");
        Assert.DoesNotContain("UniversityId", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IPAddress", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Active_lock_is_immutable_and_expiry_starts_a_fresh_counter()
    {
        var startedAt = new DateTime(2026, 7, 14, 9, 0, 0, DateTimeKind.Utc);
        var state = new AuthenticationAbuseState(
            Guid.NewGuid(),
            new string('A', 64),
            "login",
            startedAt);
        var duration = TimeSpan.FromMinutes(5);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            Assert.Equal(
                attempt == 5,
                state.RecordFailure(startedAt, 5, duration, duration));
        }

        var originalLockoutEnd = state.LockedUntilUtc;
        Assert.True(state.RecordFailure(startedAt.AddMinutes(1), 5, duration, duration));
        Assert.Equal(5, state.FailureCount);
        Assert.Equal(originalLockoutEnd, state.LockedUntilUtc);

        var afterExpiry = originalLockoutEnd!.Value.AddSeconds(1);
        Assert.False(state.RecordFailure(afterExpiry, 5, duration, duration));
        Assert.Equal(1, state.FailureCount);
        Assert.Equal(afterExpiry, state.WindowStartedAtUtc);
        Assert.Null(state.LockedUntilUtc);

        for (var attempt = 2; attempt <= 5; attempt++)
        {
            Assert.Equal(
                attempt == 5,
                state.RecordFailure(afterExpiry, 5, duration, duration));
        }

        Assert.Equal(afterExpiry.Add(duration), state.LockedUntilUtc);
    }
}
