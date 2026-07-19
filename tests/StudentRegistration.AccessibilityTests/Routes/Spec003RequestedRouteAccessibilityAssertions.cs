namespace StudentRegistration.AccessibilityTests.Routes;

public static class Spec003RequestedRouteAccessibilityAssertions
{
    public static TheoryData<int, float> WidthProfiles => new()
    {
        { 320, 4 }, { 375, 1 }, { 768, 1 },
        { 1024, 1 }, { 1280, 1 }, { 1920, 1 }
    };
}
