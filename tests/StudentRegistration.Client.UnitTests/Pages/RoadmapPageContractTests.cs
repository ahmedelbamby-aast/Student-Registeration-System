using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RoadmapPageContractTests
{
    [Fact]
    public void Stu_09_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        StudentPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("StudentRoadmapPage", "STU-09");

    [Fact]
    public void Student_roadmap_uses_the_unified_accessible_page_patterns()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentRoadmapPage.razor");

        RepositoryFiles.ContainsAll(
            source,
            "@page \"/student/roadmap\"",
            "data-route-id=\"STU-09\"",
            "<AuthenticatedPage",
            "Workspace=\"WorkspaceKind.Student\"",
            "CurrentRouteId=\"STU-09\"",
            "<SurfaceCard",
            "<RouteStatePanel",
            "CatalogueApi.GetStudentRoadmapAsync",
            "Automatic registration",
            "Missing prerequisites",
            "Completed",
            "In progress",
            "Available",
            "Eligible for self-registration",
            "Prerequisite blocked",
            "Pending approval — seat held",
            "Approved",
            "Rejected",
            "Expired — seat released",
            "Retry roadmap",
            "RouteUiState.Offline",
            "ApprovalStatusBadge",
            "Locked");
        Assert.DoesNotContain("StudentNavigation", source, StringComparison.Ordinal);
        Assert.DoesNotContain("<Navigation>", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Stu_09_and_related_student_pages_freeze_registration_ownership_and_equivalent_views()
    {
        var dashboard = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StudentDashboardPage.razor");
        var records = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor");
        var roadmap = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StudentRoadmapPage.razor");

        foreach (var source in new[] { dashboard, records, roadmap })
        {
            RepositoryFiles.ContainsAll(source,
                "Required first-term subjects are registered automatically",
                "Self-service registration starts in term 2");
        }
        RepositoryFiles.ContainsAll(dashboard, "no manual selection is required", "Open current timetable");
        RepositoryFiles.ContainsAll(records, "no manual selection is required", "equivalent views");
        RepositoryFiles.ContainsAll(roadmap, "Automatic registration", "Missing prerequisites");
    }

    [Fact]
    public void Catalogue_page_has_real_draft_and_subject_roadmap_controls_without_demo_ids()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            source,
            "CatalogueApi.CreateDraftAsync",
            "CatalogueApi.UpdateDraftAsync",
            "data-testid=\"catalogue-subject-editor\"",
            "Subject code",
            "Subject title",
            "Recommended term",
            "Prerequisites",
            "Cohort",
            "Exactly 3 credits");
        Assert.DoesNotContain("DemoDraftId", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DemoImportId", source, StringComparison.Ordinal);
    }
}
