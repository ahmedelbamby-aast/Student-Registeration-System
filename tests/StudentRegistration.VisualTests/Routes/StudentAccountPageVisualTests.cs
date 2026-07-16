using Microsoft.Playwright;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class StudentAccountPageVisualFrozenContractTests
{
    [Fact]
    public void Stu_08_visual_contract_is_approved() =>
        IdentityRouteVisualAssertions.AssertFrozenContract(
            "STU-08",
            "specs/003-ux-storyboard-accessibility/design/pages/STU-08.md",
            "STU-08-VIS-T186",
            "src/StudentRegistration.Client/Pages/StudentAccountPage.razor",
            "tests/StudentRegistration.VisualTests/Baselines/Spec007/STU-08/baseline-targets.json");
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class StudentAccountPageVisualTests(VisualRegressionFixture fixture)
{
    private const string Session = """
        {
          "displayName": "Synthetic Student",
          "roles": ["Student"],
          "activeRole": "Student",
          "sessionState": "active",
          "expiresAtUtc": "2026-07-16T18:00:00Z"
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
    public Task Stu_08_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width) =>
        IdentityRouteVisualAssertions.AssertBaselineAsync(
            fixture,
            "STU-08",
            "/student/account",
            "Student account",
            browserName,
            width,
            ConfigureAsync);

    private static Task ConfigureAsync(IPage page) =>
        page.RouteAsync("**/api/auth/session", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = Session
            }));
}
