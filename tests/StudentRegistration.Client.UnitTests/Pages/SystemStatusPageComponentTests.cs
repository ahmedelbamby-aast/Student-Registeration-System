using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class SystemStatusPageComponentTests
{
    [Fact]
    public void Sys_01_maps_every_approved_state_to_safe_content_and_actions()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SystemStatusPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "Systems available",
            "Service degraded",
            "Service unavailable",
            "Access unavailable",
            "Page not found",
            "Session expired",
            "Maintenance in progress",
            "You are offline",
            "Unexpected error",
            "Open public gateway",
            "Student login",
            "Staff login",
            "Retry status");
    }

    [Fact]
    public void Sys_01_has_one_pending_health_read_and_never_renders_raw_diagnostics()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SystemStatusPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "_isLoading",
            "if (_isLoading)",
            "OperationsApi.GetHealthAsync",
            "data-route-id=\"SYS-01\"",
            "Announcement=\"@_announcement\"",
            "tabindex=\"-1\"");
        Assert.DoesNotContain("InnerException", page, StringComparison.Ordinal);
        Assert.DoesNotContain("SqlException", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", page, StringComparison.Ordinal);
    }
}
