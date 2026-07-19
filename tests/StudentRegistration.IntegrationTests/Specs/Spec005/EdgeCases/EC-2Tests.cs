using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Import_with_a_missing_prerequisite_is_rejected_in_preview_and_cannot_be_published()
    {
        var service = new CataloguePublicationService();
        var approved = CataloguePublicationService.CreateDemoCurriculum();
        var invalid = approved with
        {
            Courses =
            [
                .. approved.Courses,
                new CatalogueCourseDefinition(
                    "DEMO-MISSING",
                    "Missing prerequisite fixture",
                    3m,
                    true,
                    1,
                    ["IN321"],
                    null,
                    null,
                    new CatalogueFieldProvenance(
                        "SPEC-005-EC-2",
                        new DateOnly(2026, 7, 19),
                        CatalogueSourceKind.SyntheticDemo,
                        [])),
            ]
        };
        var draft = new CatalogueDraft(
            Guid.NewGuid(),
            "AI-DS",
            null,
            "SPEC-005-EC-2",
            "{}",
            string.Empty,
            CatalogueDraftState.Editing);
        var import = new ImportBatch(
            Guid.NewGuid(),
            draft.Id,
            "SPEC-005-EC-2",
            new DateTime(2026, 7, 19, 0, 0, 0, DateTimeKind.Utc),
            "synthetic-content-hash",
            0,
            ImportBatchState.Uploaded);

        var preview = service.ApplyValidation(draft, import, invalid);

        Assert.False(preview.IsValid);
        var error = Assert.Single(
            preview.Errors,
            candidate => candidate.Code == "MISSING_REFERENCE");
        Assert.Contains("IN321", error.ResourceCodes);
        Assert.Equal(CatalogueDraftState.Editing, draft.State);
        Assert.Equal(ImportBatchState.Invalid, import.State);
        Assert.Null(import.PublishedVersionId);
        Assert.Throws<InvalidOperationException>(() =>
            service.PreparePublication(
                draft,
                import,
                invalid,
                "2026.1",
                null,
                "admin-ec-2",
                new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 19, 1, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(CatalogueDraftState.Editing, draft.State);
        Assert.Equal(ImportBatchState.Invalid, import.State);
        Assert.Null(import.PublishedVersionId);
    }
}
