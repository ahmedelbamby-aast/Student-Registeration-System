using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Specs.Spec011.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public async Task Missing_decision_data_fails_closed_with_support()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            IncludePolicy = false
        };

        var offering = Assert.Single((await Spec011ServiceFactory.Service(fixture)
            .EvaluateTermAsync(fixture.ApplicationUserId, fixture.TermId)).Items);

        Assert.False(offering.Eligible);
        var unavailable = Assert.Single(
            offering.Reasons,
            reason => reason.Code == "DECISION_DATA_UNAVAILABLE");
        Assert.True(unavailable.Blocking);
        Assert.False(string.IsNullOrWhiteSpace(unavailable.SupportReferencePath));
    }
}
