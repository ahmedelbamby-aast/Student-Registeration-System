using Microsoft.Playwright;
using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StudentAccountPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stu_08_design_and_owner_page_are_available()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-08.md");
        RepositoryFiles.ContainsAll(
            design,
            "STU-08-A11Y-T185",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Student account heading");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/Pages/StudentAccountPage.razor"));
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StudentAccountPageAccessibilityTests(AxeAccessibilityFixture fixture)
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
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public Task Stu_08_is_accessible_and_reflows(int width) =>
        IdentityRouteAccessibilityAssertions.AssertResponsiveAsync(
            fixture,
            "/student/account",
            "Student account",
            "Change password",
            width,
            ConfigureAsync);

    [Fact]
    public Task Stu_08_keyboard_and_400_percent_zoom_remain_operable() =>
        IdentityRouteAccessibilityAssertions.AssertKeyboardAndZoomAsync(
            fixture,
            "/student/account",
            "Student account",
            "Change password",
            ConfigureAsync,
            expectedMainId: "student-account-main");

    private static Task ConfigureAsync(IPage page) =>
        page.RouteAsync("**/api/auth/session", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = Session
            }));
}
