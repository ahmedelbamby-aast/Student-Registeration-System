using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class ResourceAdministrationPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Adm_07_accessibility_contract_is_frozen() =>
        Spec003RouteAccessibilityAssertions.AssertFrozenContract("ADM-07", "T220", "ResourceAdministrationPage");
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class ResourceAdministrationPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320, 4)]
    [InlineData(375, 1)]
    [InlineData(768, 1)]
    [InlineData(1024, 1)]
    [InlineData(1280, 1)]
    [InlineData(1920, 1)]
    public Task Adm_07_denied_state_is_keyboard_axe_zoom_and_reflow_safe(int width, float scale) =>
        Spec003RouteAccessibilityAssertions.AssertDeniedStateAsync(fixture, "ADM-07", "/admin/resources", width, scale);
}
