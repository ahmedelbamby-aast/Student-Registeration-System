using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public void Static_startup_fallback_is_recoverable_and_never_claims_success()
    {
        var appShell = RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/index.html");

        RepositoryFiles.ContainsAll(
            appShell,
            "data-startup-recovery",
            "role=\"status\"",
            "aria-live=\"polite\"",
            "aria-labelledby=\"startup-recovery-heading\"",
            "If the application does not start, check your connection and retry.",
            "href=\"/\"",
            ">Retry loading</a>");
        Assert.DoesNotContain("success", appShell, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("registration complete", appShell, StringComparison.OrdinalIgnoreCase);
    }
}
