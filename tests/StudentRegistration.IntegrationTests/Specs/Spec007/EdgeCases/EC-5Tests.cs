using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public void Repeated_recovery_is_rate_limited_with_the_same_accepted_response()
    {
        var session = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs");
        var abuse = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/IdentityRateLimitPolicies.cs");
        RepositoryFiles.ContainsAll(session, "RequestRecoveryAsync", "RecoveryAccepted");
        RepositoryFiles.ContainsAll(abuse, "recovery", "RATE_LIMITED", "RecordFailureAsync");
    }
}
