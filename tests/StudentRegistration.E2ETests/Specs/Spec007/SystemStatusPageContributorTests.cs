using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class SystemStatusPageContributorTests
{
    [Fact]
    public void Identity_contribution_to_system_status_is_safe_and_actionable()
    {
        var contribution = RepositoryFiles.Read(
            "specs/007-identity-account-lifecycle/contracts/routes/SYS-01.md");

        RepositoryFiles.ContainsAll(
            contribution,
            "SPEC-003 owns the canonical page",
            "SERVICE_UNAVAILABLE",
            "MAINTENANCE",
            "SESSION_EXPIRED",
            "RATE_LIMITED",
            "correlation ID",
            "/student/login",
            "/staff/login",
            "/account/recovery");
        Assert.DoesNotContain("password hash", contribution, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("security stamp", contribution, StringComparison.OrdinalIgnoreCase);
    }
}
