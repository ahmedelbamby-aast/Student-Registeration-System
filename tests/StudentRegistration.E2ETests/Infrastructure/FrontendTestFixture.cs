namespace StudentRegistration.E2ETests.Infrastructure;

public sealed record FrontendRouteTestPlan(
    string RouteId,
    IReadOnlyList<string> CoverageFamilies,
    string AccessibilityEvidence,
    string VisualEvidence);

public sealed record FrontendScenario(
    string Name,
    DateTimeOffset ServerTime,
    string TimeZone,
    string CorrelationReference,
    IReadOnlyDictionary<string, string> SingleRoleAccounts);

/// <summary>
/// Deterministic plan metadata shared by future route journeys. This registry
/// does not represent executed evidence.
/// </summary>
public static class FrontendTestFixture
{
    public const string FixtureVersion = "frontend-fixture/2.0";

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
        new("STU-09", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-02", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-03", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-04", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-05", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-06", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-07", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-08", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-09", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("ADM-10", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-02", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-03", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-04", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("STF-05", CoverageFamilies, AccessibilityEvidence, VisualEvidence),
        new("SYS-01", CoverageFamilies, AccessibilityEvidence, VisualEvidence)
    ];

    public static FrontendScenario CanonicalScenario { get; } = new(
        "canonical-success",
        new DateTimeOffset(2026, 7, 20, 9, 0, 0, TimeSpan.FromHours(3)),
        "Africa/Cairo",
        "CORR-FRONTEND-0001",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Student"] = "AI2600001",
            ["Admin"] = "ADM-0001",
            ["Lecturer"] = "LEC-0001",
            ["TeachingAssistant"] = "TA-0001"
        });

    public static IReadOnlyList<string> DeterministicStates { get; } =
    [
        "loading", "empty", "success", "validation-error", "service-error",
        "forbidden", "expired", "stale", "offline", "malformed-response",
        "approval-pending", "capacity-held", "overload-ineligible"
    ];
}
