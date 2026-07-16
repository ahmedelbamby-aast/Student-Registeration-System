using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Duplicate_stable_meeting_rows_are_defensively_de_duplicated()
    {
        var duplicate = Meeting(11, 10, 0, 11, 30);
        var first = Group(1, "A", "AI401", duplicate, duplicate);
        var second = Group(2, "B", "CS402", Meeting(22, 11, 0, 12, 0));

        var conflicts = new ScheduleConflictDetector().Detect([first, second]);

        var conflict = Assert.Single(conflicts);
        Assert.Equal(new TimeOnly(11, 0), conflict.OverlapStartLocal);
        Assert.Equal(new TimeOnly(11, 30), conflict.OverlapEndLocal);
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
