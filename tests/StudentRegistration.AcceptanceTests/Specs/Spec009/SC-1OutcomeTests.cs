using StudentRegistration.Academics.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class SC_1OutcomeTests
{
    [Fact]
    public void Invalid_or_cyclic_catalogue_cannot_reach_a_valid_publication_result()
    {
        var source = CataloguePublicationService.CreateDemoCurriculum();
        var cyclic = source with
        {
            Courses = source.Courses
                .Select(course => course.Code switch
                {
                    "BA101" => course with { PrerequisiteCodes = ["BA102"] },
                    "BA102" => course with { PrerequisiteCodes = ["BA101"] },
                    _ => course,
                })
                .ToArray()
        };

        var result = new CataloguePublicationService().Validate(cyclic);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Code == "PREREQUISITE_CYCLE");
    }
}
