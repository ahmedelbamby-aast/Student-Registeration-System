using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class CataloguePublicationTests
{
    [Fact]
    public void Demo_snapshot_is_the_exact_19_course_sourced_subset_with_synthetic_credits()
    {
        var content = CataloguePublicationService.CreateDemoCurriculum();

        Assert.Equal("AI-DS", content.ScopeCode);
        Assert.Equal(19, content.Courses.Count);
        Assert.Equal(
            [
                "BA101", "BA102", "BA113", "BA203", "DS221", "DS312",
                "DS322", "DS324", "DS413", "DS421", "GN111", "GN112",
                "GN121", "GN123", "GN211", "GN223", "IN211", "IN221",
                "IN311",
            ],
            content.Courses.Select(course => course.Code).Order().ToArray());
        Assert.All(content.Courses, course =>
        {
            Assert.Equal(3m, course.Credits);
            Assert.Equal(CatalogueSourceKind.OfficialSource, course.Provenance.SourceKind);
            Assert.Contains("Credits", course.Provenance.SyntheticFields);
            Assert.Equal(new DateOnly(2026, 7, 13), course.Provenance.AccessedOn);
        });

        var project = Assert.Single(content.Courses, course => course.Code == "DS413");
        Assert.Equal(2.0m, project.MinimumGpa);
        Assert.Equal(96m, project.MinimumEarnedCredits);
    }

    [Fact]
    public void Complete_demo_snapshot_validates_and_has_a_stable_canonical_hash()
    {
        var service = new CataloguePublicationService();
        var content = CataloguePublicationService.CreateDemoCurriculum();

        var first = service.Validate(content);
        var second = service.Validate(content);

        Assert.True(first.IsValid);
        Assert.Empty(first.Errors);
        Assert.StartsWith("sha256:", first.CanonicalContentHash, StringComparison.Ordinal);
        Assert.Equal(first.CanonicalContentHash, second.CanonicalContentHash);
    }

    [Fact]
    public void Missing_reference_duplicate_code_invalid_credit_and_cycle_block_the_whole_graph()
    {
        var service = new CataloguePublicationService();
        var source = CataloguePublicationService.CreateDemoCurriculum();

        var missing = source with
        {
            Courses =
            [
                .. source.Courses,
                Course("DEMO-MISSING", ["IN321"]),
            ]
        };
        var duplicate = source with
        {
            Courses =
            [
                .. source.Courses,
                Course(" ds413 ", []),
            ]
        };
        var invalidCredit = source with
        {
            Courses = source.Courses
                .Select(course => course.Code == "BA101" ? course with { Credits = 4m } : course)
                .ToArray()
        };
        var cycle = new CatalogueDraftContent(
            "AI-DS",
            [Course("DEMO-A", ["DEMO-B"]), Course("DEMO-B", ["DEMO-A"])]);

        Assert.Contains(service.Validate(missing).Errors, error =>
            error.Code == "MISSING_REFERENCE" && error.ResourceCodes.Contains("IN321"));
        Assert.Contains(service.Validate(duplicate).Errors, error =>
            error.Code == "DUPLICATE_COURSE_CODE" && error.ResourceCodes.Contains("DS413"));
        Assert.Contains(service.Validate(invalidCredit).Errors, error =>
            error.Code == "INVALID_CREDITS" && error.ResourceCodes.Contains("BA101"));
        var cycleError = Assert.Single(
            service.Validate(cycle).Errors,
            error => error.Code == "PREREQUISITE_CYCLE");
        Assert.Equal(["DEMO-A", "DEMO-B", "DEMO-A"], cycleError.ResourceCodes);
    }

    [Fact]
    public void Validation_updates_draft_and_import_without_partial_publication()
    {
        var service = new CataloguePublicationService();
        var content = CataloguePublicationService.CreateDemoCurriculum() with
        {
            Courses =
            [
                .. CataloguePublicationService.CreateDemoCurriculum().Courses,
                Course("DEMO-BAD", ["IN321"]),
            ]
        };
        var draft = new CatalogueDraft(
            Guid.NewGuid(),
            "AI-DS",
            null,
            "initial",
            "{}",
            string.Empty,
            CatalogueDraftState.Editing);
        var import = new ImportBatch(
            Guid.NewGuid(),
            draft.Id,
            "SRC-DATA-SCIENCE",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            "source-hash",
            38,
            ImportBatchState.Uploaded);

        var result = service.ApplyValidation(draft, import, content);

        Assert.False(result.IsValid);
        Assert.Equal(CatalogueDraftState.Editing, draft.State);
        Assert.Equal(ImportBatchState.Invalid, import.State);
        Assert.Null(import.PublishedVersionId);
        Assert.Contains(import.Errors, error => error.Code == "MISSING_REFERENCE");
    }

    [Fact]
    public void Validated_draft_prepares_one_immutable_version_and_same_version_children()
    {
        var service = new CataloguePublicationService();
        var content = CataloguePublicationService.CreateDemoCurriculum();
        var draft = new CatalogueDraft(
            Guid.NewGuid(),
            "AI-DS",
            null,
            "initial",
            "{}",
            string.Empty,
            CatalogueDraftState.Editing);
        var import = new ImportBatch(
            Guid.NewGuid(),
            draft.Id,
            "SRC-DATA-SCIENCE",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            "source-hash",
            38,
            ImportBatchState.Uploaded);
        var validation = service.ApplyValidation(draft, import, content);

        var publication = service.PreparePublication(
            draft,
            import,
            content,
            "2026.1",
            null,
            "admin-1",
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc));

        Assert.True(validation.IsValid);
        Assert.Equal(CatalogueDraftState.Validated, draft.State);
        Assert.Equal(ImportBatchState.Validated, import.State);
        Assert.Equal(19, publication.Courses.Count);
        Assert.All(
            publication.Courses,
            course => Assert.Equal(publication.Version.Id, course.CatalogueVersionId));
        Assert.All(
            publication.Prerequisites,
            edge => Assert.Equal(publication.Version.Id, edge.CatalogueVersionId));
        Assert.Equal(CatalogueVersionState.Published, publication.Version.State);
    }

    private static CatalogueCourseDefinition Course(
        string code,
        IReadOnlyList<string> prerequisites) =>
        new(
            code,
            code,
            3m,
            true,
            1,
            prerequisites,
            null,
            null,
            new CatalogueFieldProvenance(
                "synthetic-test",
                new DateOnly(2026, 7, 13),
                CatalogueSourceKind.SyntheticDemo,
                []));
}
