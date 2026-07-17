using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffDashboardPageAccessibilityTests
{
    [Fact]
    public void Stf_01_preserves_skip_navigation_heading_and_role_context_order()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        RepositoryFiles.ContainsAll(page, "Skip to main content", "Staff role navigation",
            "<h1", "<label", "<select", "Switch context", "aria-live");
    }
}
