using StudentRegistration.Academics.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec009;

public sealed class SC_3OutcomeTests
{
    [Fact]
    public void Admin_preview_explains_validation_and_policy_failures_before_publication()
    {
        var source = CataloguePublicationService.CreateDemoCurriculum();
        var invalid = source with
        {
            Courses = source.Courses
                .Select(course => course.Code == "DS421"
                    ? course with { PrerequisiteCodes = ["IN321"] }
                    : course)
                .ToArray()
        };
        var catalogue = new CataloguePublicationService().Validate(invalid);
        var policy = new PolicyAdministrationService().Simulate(
            PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid()),
            new(
                1.99m,
                95m,
                "Active",
                true,
                false,
                13,
                ["DS413"],
                ["GN111", "GN112"],
                true,
                false));

        Assert.Contains(catalogue.Errors, error =>
            error.Code == "MISSING_REFERENCE"
            && error.Message.Contains("IN321", StringComparison.Ordinal));
        Assert.Contains(policy.RuleResults, result =>
            !result.Passed
            && !string.IsNullOrWhiteSpace(result.Explanation)
            && !string.IsNullOrWhiteSpace(result.SourceReference));
    }
}
