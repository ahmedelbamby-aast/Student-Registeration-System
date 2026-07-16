using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class CurriculumCourseModelTests
{
    [Fact]
    public void Curriculum_course_stays_inside_one_version_and_preserves_scope_term_and_provenance()
    {
        var item = Create(
            cohortScope: "2026",
            recommendedTerm: 7);

        Assert.NotEqual(Guid.Empty, item.CatalogueVersionId);
        Assert.NotEqual(Guid.Empty, item.ProgramId);
        Assert.NotEqual(Guid.Empty, item.CourseId);
        Assert.Equal("2026", item.CohortScope);
        Assert.Equal(7, item.RecommendedTerm);
        Assert.True(item.IsRequired);
        Assert.Equal(CatalogueSourceKind.OfficialSource, item.Provenance.SourceKind);
    }

    [Fact]
    public void Curriculum_course_requires_identifiers_and_positive_level_and_term()
    {
        Assert.Throws<ArgumentException>(() => Create(catalogueVersionId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(programId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(courseId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(level: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(recommendedTerm: 0));
        Assert.Throws<ArgumentException>(() => Create(cohortScope: " "));
    }

    private static CurriculumCourse Create(
        Guid? catalogueVersionId = null,
        Guid? programId = null,
        Guid? courseId = null,
        int level = 4,
        int? recommendedTerm = 7,
        string? cohortScope = "2026") =>
        new(
            catalogueVersionId ?? Guid.NewGuid(),
            programId ?? Guid.NewGuid(),
            courseId ?? Guid.NewGuid(),
            level,
            recommendedTerm,
            true,
            cohortScope,
            new CatalogueFieldProvenance(
                "SRC-DATA-SCIENCE",
                new DateOnly(2026, 7, 13),
                CatalogueSourceKind.OfficialSource,
                ["IsRequired"]));
}
