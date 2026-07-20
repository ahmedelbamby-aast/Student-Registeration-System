using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffDashboardPageComponentTests
{
    [Fact]
    public void Stf_01_declares_loading_empty_success_denied_stale_and_safe_error_states()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        RepositoryFiles.ContainsAll(source,
            "AuthenticatedPage", "WorkspaceKind.Staff", "UiDensity.Compact", "STF-01-COMP-STATE-EMPTY",
            "STF-01-COMP-STATE-SUCCESS", "ROLE_CONTEXT_INVALID",
            "STF-01-COMP-STATE-UNAUTHORIZED", "STF-01-COMP-STATE-STALE",
            "STF-01-COMP-STATE-SERVICE-ERROR", "Refresh assignments");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectRoleContextAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Switch context", source, StringComparison.Ordinal);
    }
}
