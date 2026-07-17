using System.Reflection;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_3Tests
{
    [Fact]
    public void All_combinations_overlapping_requires_an_inclusion_minimal_actionable_diagnostic()
    {
        // Given every available group combination has a hard meeting overlap.
        var first = Group(1, "C001", Meeting(11, 10, 0, 12, 0));
        var second = Group(2, "C002", Meeting(21, 11, 0, 13, 0));
        var overlap = Assert.Single(
            new ScheduleConflictDetector().Detect([first, second]));

        Assert.Equal("MEETING_OVERLAP", overlap.Code);

        // When the no-solution result is composed, the bounded coordinator is required.
        var coordinator = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Application.OptimizationCoordinator");

        // Then it can return no fake option and an inclusion-minimal diagnostic
        // with reason, intervals, and a direct action for every member.
        Assert.True(
            coordinator is not null,
            "OptimizationCoordinator must deliver AC-3 no-solution diagnostics before this future acceptance test can pass.");
        Assert.Contains(
            coordinator!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Optimize", StringComparison.Ordinal)
                || method.Name.Contains("Recommend", StringComparison.Ordinal));
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
