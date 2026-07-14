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
}
