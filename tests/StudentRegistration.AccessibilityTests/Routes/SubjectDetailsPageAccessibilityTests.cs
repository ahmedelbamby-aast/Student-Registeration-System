using StudentRegistration.AccessibilityTests.Infrastructure;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class SubjectDetailsPageAccessibilityFrozenContractTests
{
    [Fact]
    public void Stu_03_accessibility_contract_is_frozen() =>
        Spec003RouteAccessibilityAssertions.AssertFrozenContract("STU-03", "T160", "SubjectDetailsPage");
}

[Collection(AxeAccessibilityCollection.CollectionName)]
public sealed class SubjectDetailsPageAccessibilityTests(AxeAccessibilityFixture fixture)
{
    [Theory]
    [InlineData(320, 4)]
    [InlineData(375, 1)]
    [InlineData(768, 1)]
    [InlineData(1024, 1)]
    [InlineData(1280, 1)]
    [InlineData(1920, 1)]
    public Task Stu_03_denied_state_is_keyboard_axe_zoom_and_reflow_safe(int width, float scale) =>
        Spec003RouteAccessibilityAssertions.AssertDeniedStateAsync(
            fixture, "STU-03", "/student/subjects/00000000-0000-0000-0000-000000003003", width, scale);
}
