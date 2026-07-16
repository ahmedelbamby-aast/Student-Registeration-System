using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class SC_1OutcomeTests
{
    [Fact]
    public void Every_overlapping_interval_is_returned_before_review()
    {
        var first = Group(1, "A", "AI401", Meeting(11, 10, 0, 12, 0));
        var second = Group(2, "B", "CS402", Meeting(22, 10, 30, 11, 0));
        var third = Group(3, "C", "SE403", Meeting(33, 11, 30, 12, 30));

        var conflicts = new ScheduleConflictDetector().Detect([third, first, second]);

        Assert.Collection(
            conflicts,
            conflict => AssertOverlap(
                conflict,
                first.GroupId,
                second.GroupId,
                new TimeOnly(10, 30),
                new TimeOnly(11, 0)),
            conflict => AssertOverlap(
                conflict,
                first.GroupId,
                third.GroupId,
                new TimeOnly(11, 30),
                new TimeOnly(12, 0)));
    }

    private static void AssertOverlap(
        ScheduleConflict conflict,
        Guid firstGroupId,
        Guid secondGroupId,
        TimeOnly overlapStart,
        TimeOnly overlapEnd)
    {
        Assert.Equal("MEETING_OVERLAP", conflict.Code);
        Assert.Equal(firstGroupId, conflict.First.GroupId);
        Assert.Equal(secondGroupId, conflict.Second.GroupId);
        Assert.Equal(overlapStart, conflict.OverlapStartLocal);
        Assert.Equal(overlapEnd, conflict.OverlapEndLocal);
        Assert.Equal(4, conflict.Actions.Count);
    }

    private static SelectedScheduleGroup Group(
        int id,
        string groupCode,
        string courseCode,
        params SelectedScheduleMeeting[] meetings) =>
        new(Id(id), groupCode, courseCode, $"{courseCode} subject", meetings);

    private static SelectedScheduleMeeting Meeting(
        int id,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute) =>
        new(
            Id(id),
            DayOfWeek.Monday,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute));

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
