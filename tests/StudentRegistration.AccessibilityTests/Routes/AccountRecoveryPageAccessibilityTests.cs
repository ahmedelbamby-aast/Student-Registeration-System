using StudentRegistration.AccessibilityTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class AccountRecoveryPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Auth_05_design_and_owner_page_are_available()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-05.md");
        RepositoryFiles.ContainsAll(
            design,
            "AUTH-05-A11Y-T145",
            "320,",
            "375,",
            "768,",
            "1024,",
            "1280,",
            "1920",
            "Skip link",
            "Account recovery heading");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor"));
    }
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class AccountRecoveryPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320)]
    [InlineData(375)]
    [InlineData(768)]
    [InlineData(1024)]
    [InlineData(1280)]
    [InlineData(1920)]
    public Task Auth_05_is_accessible_and_reflows(int width) =>
        IdentityRouteAccessibilityAssertions.AssertResponsiveAsync(
            fixture, "/account/recovery", "Account recovery", "Request recovery", width);

    [Fact]
    public Task Auth_05_keyboard_and_400_percent_zoom_remain_operable() =>
        IdentityRouteAccessibilityAssertions.AssertKeyboardAndZoomAsync(
            fixture, "/account/recovery", "Account recovery", "Request recovery");
}
