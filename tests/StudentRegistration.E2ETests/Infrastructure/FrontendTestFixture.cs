namespace StudentRegistration.E2ETests.Infrastructure;

public sealed record FrontendRouteTestPlan(
    string RouteId,
    IReadOnlyList<string> CoverageFamilies,
    string AccessibilityEvidence,
    string VisualEvidence);

/// <summary>
/// Deterministic plan metadata shared by future route journeys. This registry
/// does not represent executed evidence.
/// </summary>
public static class FrontendTestFixture
{
    public const string FixtureVersion = "frontend-fixture/1.0";

    private static readonly IReadOnlyList<string> CoverageFamilies =
        ["component", "contract", "e2e", "accessibility", "visual"];

    private const string AccessibilityEvidence = "axe plus keyboard evidence";
    private const string VisualEvidence = "approved visual baseline";

    public static IReadOnlyList<FrontendRouteTestPlan> RoutePlans { get; } =
    [
        new("AUTH-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("AUTH-02", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("AUTH-03", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("AUTH-04", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("AUTH-05", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-02", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-03", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-04", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-05", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-06", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-07", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STU-08", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-02", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-03", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-04", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-05", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-06", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-07", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-08", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-09", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-02", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-03", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-04", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("SYS-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence)
    ];
}
