using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class UserAdministrationPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Adm_03_design_and_owner_page_are_available()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-03.md");
        RepositoryFiles.ContainsAll(
            design,
            "ADM-03-A11Y-T200",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Users heading");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor"));
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class UserAdministrationPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    private const string Users = """
        {
          "items": [
            {
              "id": "00000000-0000-0000-0000-000000007301",
              "displayName": "Synthetic Lecturer",
              "loginIdentifier": "LEC-0001",
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
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public Task Adm_03_is_accessible_and_reflows(int width) =>
        IdentityRouteAccessibilityAssertions.AssertResponsiveAsync(
            fixture,
            "/admin/users",
            "User administration",
            "Search",
            width,
            ConfigureAsync);

    [Fact]
    public Task Adm_03_keyboard_and_400_percent_zoom_remain_operable() =>
        IdentityRouteAccessibilityAssertions.AssertKeyboardAndZoomAsync(
            fixture,
            "/admin/users",
            "User administration",
            "Search",
            ConfigureAsync,
            expectedMainId: "admin-users-main");

    private static Task ConfigureAsync(IPage page) =>
        page.RouteAsync("**/api/admin/users?*", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = Users
            }));
}
