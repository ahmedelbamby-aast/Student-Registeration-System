using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_2Tests
{
    [Fact]
    public void Prerequisite_cycle_blocks_publication_with_the_cycle_path()
    {
        var provenance = new CatalogueFieldProvenance(
            "synthetic-fixture",
            new DateOnly(2026, 7, 13),
            CatalogueSourceKind.SyntheticDemo,
            []);
        var content = new CatalogueDraftContent(
            "AI-DS",
            [
                new("DEMO-A", "A", 3m, true, 1, ["DEMO-B"], null, null, provenance),
                new("DEMO-B", "B", 3m, true, 1, ["DEMO-A"], null, null, provenance),
            ]);

        var result = new CataloguePublicationService().Validate(content);

        var error = Assert.Single(result.Errors, item => item.Code == "PREREQUISITE_CYCLE");
        Assert.Equal(["DEMO-A", "DEMO-B", "DEMO-A"], error.ResourceCodes);
        Assert.False(result.IsValid);
    }
}
