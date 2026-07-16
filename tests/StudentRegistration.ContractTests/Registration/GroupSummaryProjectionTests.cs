using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.ContractTests.Registration;

public sealed class GroupSummaryProjectionTests
{
    [Fact]
    public void Complete_projection_preserves_lifecycle_reasons_staff_and_versions()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            Capacity = 30,
            EnrolledCount = 30
        };

        var projection = new GroupSummaryProjection().Project(
            fixture.GroupSnapshot(),
            []);

        Assert.Equal("published", projection.State);
        Assert.False(projection.Selectable);
        Assert.Equal(0, projection.SeatsRemaining);
        Assert.Contains(projection.NonSelectableReasons, item => item.Code == "GROUP_FULL");
        Assert.Equal("Dr. Ada", projection.Meetings[0].Staff[0].Name);
        Assert.Equal("Eng. Noor", projection.Meetings[1].Staff[0].Name);
        Assert.NotEqual(projection.Meetings[0].MeetingId, projection.Meetings[1].MeetingId);
        Assert.Equal(Convert.ToBase64String(Spec011ScenarioBuilder.Bytes(6)), projection.RowVersion);
    }

    [Fact]
    public void Exact_overlap_blocks_the_group_but_adjacent_meetings_do_not()
    {
        var fixture = new Spec011ScenarioBuilder();
        var projector = new GroupSummaryProjection();
        var conflict = projector.Project(
            fixture.GroupSnapshot(),
            [new(DayOfWeek.Monday, new TimeOnly(9, 30), new TimeOnly(10, 30))]);
        var adjacent = projector.Project(
            fixture.GroupSnapshot(),
            [new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(9, 0))]);

        Assert.Contains(conflict.NonSelectableReasons, item => item.Code == "MEETING_CONFLICT");
        Assert.DoesNotContain(adjacent.NonSelectableReasons, item => item.Code == "MEETING_CONFLICT");
        Assert.True(adjacent.Selectable);
    }
}
