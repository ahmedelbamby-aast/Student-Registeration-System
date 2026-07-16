using Microsoft.Playwright;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class UserAdministrationPageVisualFrozenContractTests
{
    [Fact]
    public void Adm_03_visual_contract_is_approved() =>
        IdentityRouteVisualAssertions.AssertFrozenContract(
            "ADM-03",
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-03.md",
            "ADM-03-VIS-T201",
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor",
            "tests/StudentRegistration.VisualTests/Baselines/Spec007/ADM-03/baseline-targets.json");
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class UserAdministrationPageVisualTests(VisualRegressionFixture fixture)
{
    private const string Users = """
        {
          "items": [
            {
              "id": "00000000-0000-0000-0000-000000007301",
              "displayName": "Synthetic Lecturer",
              "loginIdentifier": "lecturer.demo",
              "enabled": true,
              "roles": ["Lecturer"],
              "rowVersion": "AQIDBA=="
            }
          ],
          "pageNumber": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "displayName,id"
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
    public Task Adm_03_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width) =>
        IdentityRouteVisualAssertions.AssertBaselineAsync(
            fixture,
            "ADM-03",
            "/admin/users",
            "User administration",
            browserName,
            width,
            ConfigureAsync);

    private static Task ConfigureAsync(IPage page) =>
        page.RouteAsync("**/api/admin/users?*", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = Users
            }));
}
