using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class RegistrationResultPageAccessibilityFrozenContractTests
{
    [Fact] public void Stu_06_accessibility_contract_is_frozen() =>
        Spec003RouteAccessibilityAssertions.AssertFrozenContract("STU-06", "T175", "RegistrationResultPage");
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class RegistrationResultPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320, 4)] [InlineData(375, 1)] [InlineData(768, 1)]
    [InlineData(1024, 1)] [InlineData(1280, 1)] [InlineData(1920, 1)]
    public Task Stu_06_denied_state_is_keyboard_axe_zoom_and_reflow_safe(int width, float scale) =>
        Spec003RouteAccessibilityAssertions.AssertDeniedStateAsync(
            fixture, "STU-06", "/student/registration/result/00000000-0000-0000-0000-000000003006", width, scale);
}
