using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class SystemStatusPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Sys_01_design_and_owner_page_are_available()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/SYS-01.md");
        RepositoryFiles.ContainsAll(
            design,
            "SYS-01-A11Y-T255",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Status page heading");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/Pages/SystemStatusPage.razor"));
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class SystemStatusPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string Healthy = """
        {
          "status": "healthy",
          "version": "demo-1.0",
          "timestampUtc": "2026-07-16T12:00:00Z"
        }
        """;

    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public Task Sys_01_is_accessible_and_reflows(int width) =>
        IdentityRouteAccessibilityAssertions.AssertResponsiveAsync(
            fixture,
            "/status/healthy",
            "System status",
            "Open public gateway",
            width,
            ConfigureAsync,
            requireVisibleLabels: false,
            primaryRole: AriaRole.Link);

    [Fact]
    public Task Sys_01_keyboard_and_400_percent_zoom_remain_operable() =>
        IdentityRouteAccessibilityAssertions.AssertKeyboardAndZoomAsync(
            fixture,
            "/status/healthy",
            "System status",
            "Open public gateway",
            ConfigureAsync,
            expectedMainId: "system-status-main",
            primaryRole: AriaRole.Link);

    private static Task ConfigureAsync(IPage page) =>
        page.RouteAsync("**/api/health", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = Healthy
            }));
}
