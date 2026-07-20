using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RoadmapPageContractTests
{
    [Fact]
    public void Student_roadmap_uses_the_unified_accessible_page_patterns()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentRoadmapPage.razor");

        RepositoryFiles.ContainsAll(
            source,
            "@page \"/student/roadmap\"",
            "data-route-id=\"STU-09\"",
            "<AppShell",
            "<PageHeader",
            "<SurfaceCard",
            "<RouteStatePanel",
            "CatalogueApi.GetStudentRoadmapAsync",
            "Automatic registration",
            "Missing prerequisites",
            "Completed",
            "In progress",
            "Available",
            "Locked");
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
