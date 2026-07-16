using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class AC_2Tests
{
    [Fact]
    public void Adjacent_meetings_do_not_overlap_or_emit_disabled_travel_conflicts()
    {
        var first = Group(1, "A", "AI401", Meeting(11, 10, 0, 11, 0));
        var second = Group(2, "B", "CS402", Meeting(22, 11, 0, 12, 0));

        var conflicts = new ScheduleConflictDetector().Detect([first, second]);

        Assert.Empty(conflicts);
        Assert.DoesNotContain(conflicts, conflict => conflict.Code == "TRAVEL_BUFFER");
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
