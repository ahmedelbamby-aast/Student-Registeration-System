using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class AC_1Tests
{
    [Fact]
    public void Missing_prerequisite_returns_source_row_and_publishes_nothing()
    {
        var source = CataloguePublicationService.CreateDemoCurriculum();
        var content = source with
        {
            Courses =
            [
                .. source.Courses,
                new CatalogueCourseDefinition(
                    "DEMO-MISSING",
                    "Missing reference fixture",
                    3m,
                    true,
                    8,
                    ["IN321"],
                    null,
                    null,
                    new CatalogueFieldProvenance(
                        "synthetic-fixture",
                        new DateOnly(2026, 7, 13),
                        CatalogueSourceKind.SyntheticDemo,
                        [])),
            ]
        };

        var result = new CataloguePublicationService().Validate(content);

        var error = Assert.Single(result.Errors, item => item.Code == "MISSING_REFERENCE");
        Assert.True(error.Row > 0);
        Assert.Contains("IN321", error.ResourceCodes);
        Assert.False(result.IsValid);
    }
}
