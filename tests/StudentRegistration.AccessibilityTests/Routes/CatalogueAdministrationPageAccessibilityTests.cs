using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class CatalogueAdministrationPageAccessibilityFrozenContractTests
{
    [Fact] public void Adm_05_accessibility_contract_is_frozen() =>
        Spec003RouteAccessibilityAssertions.AssertFrozenContract("ADM-05", "T210", "CatalogueAdministrationPage");
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class CatalogueAdministrationPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320, 4)] [InlineData(375, 1)] [InlineData(768, 1)]
    [InlineData(1024, 1)] [InlineData(1280, 1)] [InlineData(1920, 1)]
    public Task Adm_05_denied_state_is_keyboard_axe_zoom_and_reflow_safe(int width, float scale) =>
        Spec003RouteAccessibilityAssertions.AssertDeniedStateAsync(fixture, "ADM-05", "/admin/catalogue", width, scale);
}
