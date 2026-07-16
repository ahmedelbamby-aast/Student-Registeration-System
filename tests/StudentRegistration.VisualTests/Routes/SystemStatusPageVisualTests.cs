using Microsoft.Playwright;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class SystemStatusPageVisualFrozenContractTests
{
    [Fact]
    public void Sys_01_visual_contract_is_approved() =>
        IdentityRouteVisualAssertions.AssertFrozenContract(
            "SYS-01",
            "specs/003-ux-storyboard-accessibility/design/pages/SYS-01.md",
            "SYS-01-VIS-T256",
            "src/StudentRegistration.Client/Pages/SystemStatusPage.razor",
            "tests/StudentRegistration.VisualTests/Baselines/Spec003/SYS-01/baseline-targets.json");
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class SystemStatusPageVisualTests(VisualRegressionFixture fixture)
{
    private const string Healthy = """
        {
          "status": "healthy",
          "version": "demo-1.0",
          "timestampUtc": "2026-07-16T12:00:00Z"
        }
        """;

    [Theory]
    [InlineData("chrome", 375)]
    [InlineData("chrome", 768)]
    [InlineData("chrome", 1280)]
    [InlineData("chrome", 1920)]
    [InlineData("edge", 375)]
    [InlineData("edge", 768)]
    [InlineData("edge", 1280)]
    [InlineData("edge", 1920)]
    [InlineData("firefox", 375)]
    [InlineData("firefox", 768)]
    [InlineData("firefox", 1280)]
    [InlineData("firefox", 1920)]
    [InlineData("webkit", 375)]
    [InlineData("webkit", 768)]
    [InlineData("webkit", 1280)]
    [InlineData("webkit", 1920)]
    public Task Sys_01_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width) =>
        IdentityRouteVisualAssertions.AssertBaselineAsync(
            fixture,
            "SYS-01",
            "/status/healthy",
            "System status",
            browserName,
            width,
            ConfigureAsync,
            baselineOwner: "Spec003");

    private static Task ConfigureAsync(IPage page) =>
        page.RouteAsync("**/api/health", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = Healthy
            }));
}
