using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class StudentActivationPageVisualFrozenContractTests
{
    [Fact]
    public void Auth_03_visual_contract_is_approved() =>
        IdentityRouteVisualAssertions.AssertFrozenContract(
            "AUTH-03",
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-03.md",
            "AUTH-03-VIS-T136",
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor",
            "tests/StudentRegistration.VisualTests/Baselines/Spec007/AUTH-03/baseline-targets.json");
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class StudentActivationPageVisualTests(VisualRegressionFixture fixture)
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
    public Task Auth_03_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width) =>
        IdentityRouteVisualAssertions.AssertBaselineAsync(
            fixture, "AUTH-03", "/student/activate", "Activate Student account", browserName, width);
}
