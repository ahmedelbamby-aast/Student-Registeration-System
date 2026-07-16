using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class SC_3OutcomeTests
{
    [Fact]
    public async Task Client_filtering_never_changes_the_server_decision()
    {
        var fixture = new Spec011ScenarioBuilder { BlockingHold = true };
        var query = Spec011AcceptanceSupport.Search(fixture);

        var invalidOverride = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, "true", null, null, null, null, 1, 20));
        var serverDecision = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, "all", null, null, null, null, 1, 20));

        Assert.Equal(OfferingSearchOutcome.ValidationError, invalidOverride.Outcome);
        var offering = Assert.Single(serverDecision.Page!.Items);
        Assert.False(offering.Eligible);
        Assert.DoesNotContain(offering.Groups, group =>
            group.Selectable && offering.Eligible);
    }
}
