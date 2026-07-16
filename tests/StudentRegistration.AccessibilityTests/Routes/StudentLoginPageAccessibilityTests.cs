using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StudentLoginPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Auth_02_design_and_owner_page_are_available()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-02.md");
        RepositoryFiles.ContainsAll(
            design,
            "AUTH-02-A11Y-T130",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Student login heading");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/Pages/StudentLoginPage.razor"));
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StudentLoginPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public Task Auth_02_is_accessible_and_reflows(int width) =>
        IdentityRouteAccessibilityAssertions.AssertResponsiveAsync(
            fixture, "/student/login", "Student login", "Sign in", width);

    [Fact]
    public Task Auth_02_keyboard_and_400_percent_zoom_remain_operable() =>
        IdentityRouteAccessibilityAssertions.AssertKeyboardAndZoomAsync(
            fixture, "/student/login", "Student login", "Sign in");
}
