using StudentRegistration.Academics.Application;

namespace StudentRegistration.IntegrationTests.Specs.Spec009.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Case_and_spacing_variants_are_one_duplicate_code()
    {
        var source = CataloguePublicationService.CreateDemoCurriculum();
        var duplicate = source.Courses[0] with { Code = " ba101 " };
        var result = new CataloguePublicationService().Validate(
            source with { Courses = [.. source.Courses, duplicate] });

        var error = Assert.Single(
            result.Errors,
            item => item.Code == "DUPLICATE_COURSE_CODE");
        Assert.Contains("BA101", error.ResourceCodes);
    }
}
