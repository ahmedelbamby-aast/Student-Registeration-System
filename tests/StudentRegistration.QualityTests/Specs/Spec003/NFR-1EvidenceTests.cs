using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class NFR_1EvidenceTests
{
    [Fact]
    public void Every_mvp_route_has_executable_axe_evidence_without_skipped_checks()
    {
        foreach (var page in RoutePages)
        {
            var source = EvidenceSource(page);
            Assert.Contains("AxeAccessibilityFixture", source, StringComparison.Ordinal);
            Assert.Contains("AssertNoSeriousAxeViolationsAsync", source, StringComparison.Ordinal);
            Assert.DoesNotContain("[Fact(Skip", source, StringComparison.Ordinal);
            Assert.DoesNotContain("[Theory(Skip", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Reusable_components_have_semantic_focus_and_live_region_guards()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("src/StudentRegistration.Client/Components/Layout/AppShell.razor"),
            "srs-skip-link", "role=\"banner\"", "<main", "tabindex=\"-1\"", "role=\"contentinfo\"");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("src/StudentRegistration.Client/Components/Forms/AccessibleValidationSummary.razor"),
            "role=\"alert\"", "tabindex=\"-1\"");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("src/StudentRegistration.Client/Components/Scheduling/ConflictPanel.razor"),
            "aria-live=\"polite\"", "aria-hidden=\"true\"");
    }

    internal static readonly string[] RoutePages =
    [
        "RoleGatewayPage", "StudentLoginPage", "StudentActivationPage", "StaffLoginPage",
        "AccountRecoveryPage", "StudentDashboardPage", "SubjectDiscoveryPage", "SubjectDetailsPage",
        "ScheduleBuilderPage", "RegistrationReviewPage", "RegistrationResultPage",
        "RegistrationHistoryPage", "StudentAccountPage", "AdminDashboardPage",
        "TermAdministrationPage", "UserAdministrationPage", "StudentAdministrationPage",
        "CatalogueAdministrationPage", "OfferingAdministrationPage", "ResourceAdministrationPage",
        "RegistrationAdministrationPage", "AuditAdministrationPage", "StaffDashboardPage",
        "StaffTimetablePage", "StaffRosterPage", "StaffAvailabilityPage", "SystemStatusPage",
    ];

    internal static string EvidenceSource(string page)
    {
        var route = RepositoryFiles.Read(
            $"tests/StudentRegistration.AccessibilityTests/Routes/{page}AccessibilityTests.cs");
        var helperDirectory = RepositoryFiles.PathTo(
            "tests/StudentRegistration.AccessibilityTests/Routes");
        var helpers = Directory.EnumerateFiles(helperDirectory, "*Assertions.cs")
            .Select(File.ReadAllText);
        return string.Join('\n', new[] { route }.Concat(helpers));
    }
}
