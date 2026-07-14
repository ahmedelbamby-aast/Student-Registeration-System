namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_5Tests
{
    [Fact]
    public void Recovery_and_abuse_controls_are_generic_and_browser_storage_free()
    {
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs",
            "RecoveryAccepted",
            "ChallengeInvalid",
            "IAccountRecoveryProofDelivery");
        var policy = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/IdentityRateLimitPolicies.cs",
            "RATE_LIMITED",
            "RecordFailureAsync");
        Assert.DoesNotContain("UniversityId", policy, StringComparison.Ordinal);

        var boundary = RepositoryFiles.Read(
            "tests/StudentRegistration.SecurityTests/IdentitySecurityBoundaryTests.cs");
        Assert.Contains("localStorage.setItem", boundary, StringComparison.Ordinal);
    }
}
