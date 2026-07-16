using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class AC_3Tests
{
    [Fact]
    public async Task Full_groups_are_hidden_by_default_and_explained_when_unavailable_is_requested()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            Capacity = 30,
            EnrolledCount = 30
        };
        var query = Spec011AcceptanceSupport.Search(fixture);

        var available = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, null, null, null, null, null, 1, 20));
        var unavailable = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, "unavailable", null, null, "full", null, 1, 20));

        Assert.Empty(available.Page!.Items);
        var offering = Assert.Single(unavailable.Page!.Items);
        Assert.False(offering.Eligible);
        var group = Assert.Single(offering.Groups);
        Assert.False(group.Selectable);
        Assert.Equal(0, group.SeatsRemaining);
        Assert.Contains(group.NonSelectableReasons, reason => reason.Code == "GROUP_FULL");
        Assert.Contains(offering.Reasons, reason => reason.Code == "GROUP_FULL");
    }
}
