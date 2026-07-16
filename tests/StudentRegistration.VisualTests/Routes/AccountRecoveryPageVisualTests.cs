using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class AccountRecoveryPageVisualFrozenContractTests
{
    [Fact]
    public void Auth_05_visual_contract_is_approved() =>
        IdentityRouteVisualAssertions.AssertFrozenContract(
            "AUTH-05",
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-05.md",
            "AUTH-05-VIS-T146",
            "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor",
            "tests/StudentRegistration.VisualTests/Baselines/Spec007/AUTH-05/baseline-targets.json");
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class AccountRecoveryPageVisualTests(VisualRegressionFixture fixture)
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
    public Task Auth_05_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width) =>
        IdentityRouteVisualAssertions.AssertBaselineAsync(
            fixture, "AUTH-05", "/account/recovery", "Account recovery", browserName, width);
}
