using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Historical_course_is_deactivated_only_in_a_superseding_version()
    {
        var service = new CataloguePublicationService();
        var originalContent = CataloguePublicationService.CreateDemoCurriculum();
        var original = Prepare(service, originalContent, "2026.1", null);
        var revisedContent = originalContent with
        {
            Courses = originalContent.Courses
                .Select(course => course.Code == "BA101"
                    ? course with { IsActive = false }
                    : course)
                .ToArray()
        };
        var revised = Prepare(service, revisedContent, "2026.2", original.Version.Id);
        original.Version.MarkSuperseded();

        Assert.True(Assert.Single(original.Courses, item => item.Code == "BA101").IsActive);
        Assert.False(Assert.Single(revised.Courses, item => item.Code == "BA101").IsActive);
        Assert.Equal(original.Version.Id, revised.Version.SupersedesId);
        Assert.Equal(CatalogueVersionState.Superseded, original.Version.State);
    }

    private static PreparedCataloguePublication Prepare(
        CataloguePublicationService service,
        CatalogueDraftContent content,
        string versionCode,
        Guid? supersedesId)
    {
        var draft = new CatalogueDraft(
            Guid.NewGuid(), "AI-DS", supersedesId, "initial", "{}", string.Empty,
            CatalogueDraftState.Editing);
        var import = new ImportBatch(
            Guid.NewGuid(), draft.Id, "SRC-DATA-SCIENCE",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            "source-hash", 38, ImportBatchState.Uploaded);
        Assert.True(service.ApplyValidation(draft, import, content).IsValid);
        return service.PreparePublication(
            draft, import, content, versionCode, supersedesId, "admin-1",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc));
    }
}
