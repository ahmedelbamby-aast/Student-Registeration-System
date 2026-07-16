using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class StaffLoginPageVisualFrozenContractTests
{
    [Fact]
    public void Auth_04_visual_contract_is_approved() =>
        IdentityRouteVisualAssertions.AssertFrozenContract(
            "AUTH-04",
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-04.md",
            "AUTH-04-VIS-T141",
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor",
            "tests/StudentRegistration.VisualTests/Baselines/Spec007/AUTH-04/baseline-targets.json");
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class StaffLoginPageVisualTests(VisualRegressionFixture fixture)
{
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
    public Task Auth_04_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width) =>
        IdentityRouteVisualAssertions.AssertBaselineAsync(
            fixture, "AUTH-04", "/staff/login", "Staff login", browserName, width);
}
