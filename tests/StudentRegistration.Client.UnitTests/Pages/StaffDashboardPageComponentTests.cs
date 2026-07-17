using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffDashboardPageComponentTests
{
    [Fact]
    public void Stf_01_declares_loading_empty_success_denied_stale_and_safe_error_states()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        RepositoryFiles.ContainsAll(source,
            "STF-01-COMP-STATE-LOADING", "STF-01-COMP-STATE-EMPTY",
            "STF-01-COMP-STATE-SUCCESS", "STF-01-COMP-STATE-VALIDATION-ERROR",
            "STF-01-COMP-STATE-UNAUTHORIZED", "STF-01-COMP-STATE-STALE",
            "STF-01-COMP-STATE-SERVICE-ERROR", "Refresh assignments");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
    }
}
