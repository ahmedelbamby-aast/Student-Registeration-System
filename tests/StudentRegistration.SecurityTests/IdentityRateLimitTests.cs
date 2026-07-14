using StudentRegistration.TestSupport;

namespace StudentRegistration.SecurityTests;

public sealed class IdentityRateLimitTests
{
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
}
