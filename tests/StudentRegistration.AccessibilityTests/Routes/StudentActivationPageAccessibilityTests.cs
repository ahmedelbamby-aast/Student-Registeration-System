using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StudentActivationPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Auth_03_design_and_owner_page_are_available()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-03.md");
        RepositoryFiles.ContainsAll(
            design,
            "AUTH-03-A11Y-T135",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Student activation heading");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor"));
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class StudentActivationPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public Task Auth_03_is_accessible_and_reflows(int width) =>
        IdentityRouteAccessibilityAssertions.AssertResponsiveAsync(
            fixture, "/student/activate", "Activate Student account", "Activate account", width);

    [Fact]
    public Task Auth_03_keyboard_and_400_percent_zoom_remain_operable() =>
        IdentityRouteAccessibilityAssertions.AssertKeyboardAndZoomAsync(
            fixture, "/student/activate", "Activate Student account", "Activate account");
}
