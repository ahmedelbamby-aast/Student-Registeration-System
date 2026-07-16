using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class ProgramModelTests
{
    [Fact]
    public void Program_is_scoped_to_one_catalogue_version_and_normalizes_its_code()
    {
        var versionId = Guid.NewGuid();
        var provenance = Provenance();
        var program = new StudentRegistration.Academics.Domain.Program(
            Guid.NewGuid(),
            versionId,
            " ai-ds ",
            "Data Science",
            true,
            provenance);

        Assert.Equal(versionId, program.CatalogueVersionId);
        Assert.Equal("AI-DS", program.Code);
        Assert.Equal("Data Science", program.DisplayName);
        Assert.True(program.IsActive);
        Assert.Same(provenance, program.Provenance);
        Assert.Empty(program.Version);
    }

    [Fact]
    public void Program_requires_identity_scope_and_complete_text()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(catalogueVersionId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(code: " "));
        Assert.Throws<ArgumentException>(() => Create(displayName: " "));
    }

    private static StudentRegistration.Academics.Domain.Program Create(
        Guid? id = null,
        Guid? catalogueVersionId = null,
        string code = "AI-DS",
        string displayName = "Data Science") =>
        new(
            id ?? Guid.NewGuid(),
            catalogueVersionId ?? Guid.NewGuid(),
            code,
            displayName,
            true,
            Provenance());

    private static CatalogueFieldProvenance Provenance() =>
        new(
            "https://aast.edu/catalogue",
            new DateOnly(2026, 7, 13),
            CatalogueSourceKind.OfficialSource,
            ["Credits"]);
}
