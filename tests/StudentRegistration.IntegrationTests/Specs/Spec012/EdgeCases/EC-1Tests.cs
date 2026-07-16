using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void One_overlap_in_a_multi_slot_group_is_a_hard_group_conflict()
    {
        var first = Group(
            1,
            "A",
            "AI401",
            Meeting(11, DayOfWeek.Monday, 8, 0, 9, 0),
            Meeting(12, DayOfWeek.Wednesday, 10, 0, 11, 30));
        var second = Group(
            2,
            "B",
            "CS402",
            Meeting(21, DayOfWeek.Monday, 12, 0, 13, 0),
            Meeting(22, DayOfWeek.Wednesday, 11, 0, 12, 0));

        var conflict = Assert.Single(
            new ScheduleConflictDetector().Detect([first, second]));

        Assert.Equal("MEETING_OVERLAP", conflict.Code);
        Assert.Equal(DayOfWeek.Wednesday, conflict.DayOfWeek);
        Assert.Equal(new TimeOnly(11, 0), conflict.OverlapStartLocal);
        Assert.Equal(new TimeOnly(11, 30), conflict.OverlapEndLocal);
        Assert.Contains(conflict.Actions, action => action.TargetGroupId == first.GroupId);
        Assert.Contains(conflict.Actions, action => action.TargetGroupId == second.GroupId);
    }

    private static SelectedScheduleGroup Group(
        int id,
        string groupCode,
        string courseCode,
        params SelectedScheduleMeeting[] meetings) =>
        new(Id(id), groupCode, courseCode, $"{courseCode} subject", meetings);

    private static SelectedScheduleMeeting Meeting(
        int id,
        DayOfWeek day,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute) =>
        new(
            Id(id),
            day,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute));

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
