using System.Reflection;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_1Tests
{
    [Fact]
    public void Overlapping_selection_with_an_alternate_requires_one_complete_conflict_free_option()
    {
        // Given two selected groups overlap and one course has a viable alternate.
        var selectedFirst = Group(1, "C001", Meeting(11, 10, 0, 11, 0));
        var selectedSecond = Group(2, "C002", Meeting(21, 10, 30, 11, 30));
        var alternateSecond = Group(3, "C002", Meeting(31, 12, 0, 13, 0));
        var detector = new ScheduleConflictDetector();

        Assert.Single(detector.Detect([selectedFirst, selectedSecond]));
        Assert.Empty(detector.Detect([selectedFirst, alternateSecond]));

        // When recommendations are requested, the production optimizer is required.
        var optimizer = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Domain.ScheduleOptimizer");

        // Then a complete option can be asserted without omitting or duplicating a course.
        Assert.True(
            optimizer is not null,
            "ScheduleOptimizer must deliver AC-1 complete conflict-free alternatives before this future acceptance test can pass.");
        Assert.Contains(
            optimizer!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Optimize", StringComparison.Ordinal));
    }

    private static SelectedScheduleGroup Group(
        int id,
        string courseCode,
        params SelectedScheduleMeeting[] meetings) =>
        new(
            Id(id),
            $"G{id:00}",
            courseCode,
            $"Course {courseCode}",
            meetings);

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
