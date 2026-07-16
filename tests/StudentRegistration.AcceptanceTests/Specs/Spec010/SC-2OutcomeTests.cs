using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class SC_2OutcomeTests
{
    [Fact]
    public async Task Visible_group_exposes_capacity_and_per_activity_details()
    {
        var store = new OfferingStoreFake
        {
            Snapshot = Spec010Scenario.CompleteOffering(state: "published")
        };

        var detail = await new OfferingService(store)
            .GetStudentDetailAsync(store.Snapshot.Id);

        var group = Assert.Single(detail!.Groups);
        Assert.Equal(30, group.Capacity);
        Assert.Equal(2, group.Meetings.Count);
        Assert.All(group.Meetings, meeting =>
        {
            Assert.NotEmpty(meeting.Staff);
            Assert.False(string.IsNullOrWhiteSpace(meeting.RoomCode));
            Assert.False(string.IsNullOrWhiteSpace(meeting.Location));
            Assert.InRange(meeting.DayOfWeek, 0, 6);
            Assert.True(meeting.EndLocal > meeting.StartLocal);
        });
    }
}
