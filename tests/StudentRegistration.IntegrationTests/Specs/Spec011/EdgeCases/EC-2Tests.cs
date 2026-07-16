using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Specs.Spec011.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public async Task Detail_refresh_reports_a_group_that_became_full_with_a_new_version()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            Capacity = 30,
            EnrolledCount = 29
        };
        var before = Assert.Single((await Spec011ServiceFactory.Service(fixture)
            .EvaluateOfferingAsync(fixture.ApplicationUserId, fixture.OfferingId)).Items);

        fixture.EnrolledCount = 30;
        fixture.GroupRowVersionValue = 7;
        var after = Assert.Single((await Spec011ServiceFactory.Service(fixture)
            .EvaluateOfferingAsync(fixture.ApplicationUserId, fixture.OfferingId)).Items);

        var original = Assert.Single(before.Groups);
        Assert.True(original.Selectable);
        var refreshed = Assert.Single(after.Groups);
        Assert.False(refreshed.Selectable);
        Assert.NotEqual(original.RowVersion, refreshed.RowVersion);
        Assert.Equal(0, refreshed.SeatsRemaining);
        Assert.Contains(refreshed.NonSelectableReasons, reason => reason.Code == "GROUP_FULL");
        Assert.False(after.Eligible);
    }
}
