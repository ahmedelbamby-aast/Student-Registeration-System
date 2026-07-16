using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class CourseModelTests
{
    [Fact]
    public void Course_preserves_version_ownership_and_field_provenance()
    {
        var provenance = new CatalogueFieldProvenance(
            "SRC-DATA-SCIENCE",
            new DateOnly(2026, 7, 13),
            CatalogueSourceKind.OfficialSource,
            ["Credits", "IsActive"]);
        var course = new Course(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " ds413 ",
            "Project I",
            3m,
            true,
            provenance);

        Assert.Equal("DS413", course.Code);
        Assert.Equal(3m, course.Credits);
        Assert.True(course.IsActive);
        Assert.Equal(CatalogueSourceKind.OfficialSource, course.Provenance.SourceKind);
        Assert.Equal(["Credits", "IsActive"], course.Provenance.SyntheticFields);
        Assert.Empty(course.Version);
    }

    [Fact]
    public void Course_rejects_invalid_identity_text_and_credits()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(catalogueVersionId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(code: " "));
        Assert.Throws<ArgumentException>(() => Create(title: " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(credits: 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(credits: -1m));
    }

    private static Course Create(
        Guid? id = null,
        Guid? catalogueVersionId = null,
        string code = "DS413",
        string title = "Project I",
        decimal credits = 3m) =>
        new(
            id ?? Guid.NewGuid(),
            catalogueVersionId ?? Guid.NewGuid(),
            code,
            title,
            credits,
            true,
            new CatalogueFieldProvenance(
                "SRC-DATA-SCIENCE",
                new DateOnly(2026, 7, 13),
                CatalogueSourceKind.OfficialSource,
                ["Credits"]));
}
