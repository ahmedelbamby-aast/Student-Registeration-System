using StudentRegistration.Registration.Domain;

namespace StudentRegistration.ApplicationTests.Registration;

public sealed class ScheduleConflictDetectorTests
{
    [Fact]
    public void Strict_half_open_overlap_returns_complete_deterministic_conflicts()
    {
        var first = Group(
            1,
            "A",
            "AI401",
            "Artificial Intelligence",
            Meeting(11, DayOfWeek.Monday, 10, 0, 11, 30),
            Meeting(12, DayOfWeek.Wednesday, 8, 0, 9, 0));
        var second = Group(
            2,
            "B",
            "CS402",
            "Distributed Systems",
            Meeting(22, DayOfWeek.Monday, 11, 0, 12, 0));
        var adjacent = Group(
            3,
            "C",
            "SE403",
            "Software Engineering",
            Meeting(33, DayOfWeek.Wednesday, 9, 0, 10, 0));

        var forward = new ScheduleConflictDetector().Detect([second, adjacent, first]);
        var reverse = new ScheduleConflictDetector().Detect([first, adjacent, second]);

        var conflict = Assert.Single(forward);
        Assert.Equal(forward.Select(Signature), reverse.Select(Signature));
        Assert.Equal("MEETING_OVERLAP", conflict.Code);
        Assert.Equal(DayOfWeek.Monday, conflict.DayOfWeek);
        Assert.Equal(new TimeOnly(11, 0), conflict.OverlapStartLocal);
        Assert.Equal(new TimeOnly(11, 30), conflict.OverlapEndLocal);
        Assert.Equal("A", conflict.First.GroupCode);
        Assert.Equal("AI401", conflict.First.CourseCode);
        Assert.Equal("Artificial Intelligence", conflict.First.SubjectTitle);
        Assert.Equal(new TimeOnly(10, 0), conflict.First.StartLocal);
        Assert.Equal(new TimeOnly(11, 30), conflict.First.EndLocal);
        Assert.Equal("B", conflict.Second.GroupCode);
        Assert.Equal("CS402", conflict.Second.CourseCode);
        Assert.Equal("Distributed Systems", conflict.Second.SubjectTitle);
        Assert.Equal(new TimeOnly(11, 0), conflict.Second.StartLocal);
        Assert.Equal(new TimeOnly(12, 0), conflict.Second.EndLocal);
        Assert.Contains("Conflict", conflict.Message, StringComparison.Ordinal);
        Assert.Collection(
            conflict.Actions,
            action => AssertAction(action, "change-group", first.GroupId, "/student/subjects"),
            action => AssertAction(action, "remove-group", first.GroupId, "/student/schedule"),
            action => AssertAction(action, "change-group", second.GroupId, "/student/subjects"),
            action => AssertAction(action, "remove-group", second.GroupId, "/student/schedule"));
    }

    [Fact]
    public void Duplicate_slots_are_removed_before_pair_evaluation()
    {
        var duplicate = Meeting(11, DayOfWeek.Monday, 10, 0, 11, 30);
        var first = Group(1, "A", "AI401", "AI", duplicate, duplicate);
        var second = Group(
            2,
            "B",
            "CS402",
            "DS",
            Meeting(22, DayOfWeek.Monday, 11, 0, 12, 0));

        Assert.Single(new ScheduleConflictDetector().Detect([first, second]));
    }

    [Fact]
    public void Null_input_is_rejected_and_empty_input_is_valid()
    {
        var detector = new ScheduleConflictDetector();

        Assert.Throws<ArgumentNullException>(() => detector.Detect(null!));
        Assert.Empty(detector.Detect([]));
    }

    private static void AssertAction(
        ScheduleConflictAction action,
        string expectedAction,
        Guid expectedGroupId,
        string expectedRoute)
    {
        Assert.Equal(expectedAction, action.Action);
        Assert.Equal(expectedGroupId, action.TargetGroupId);
        Assert.False(string.IsNullOrWhiteSpace(action.Label));
        Assert.Equal(expectedRoute, action.Route);
    }

    private static string Signature(ScheduleConflict conflict) =>
        string.Join(
            "|",
            conflict.Code,
            conflict.First.GroupId,
            conflict.Second.GroupId,
            conflict.DayOfWeek,
            conflict.First.StartLocal,
            conflict.First.EndLocal,
            conflict.Second.StartLocal,
            conflict.Second.EndLocal,
            conflict.OverlapStartLocal,
            conflict.OverlapEndLocal,
            string.Join(
                ",",
                conflict.Actions.Select(action =>
                    $"{action.Action}:{action.TargetGroupId}:{action.Route}")));

    private static SelectedScheduleGroup Group(
        int id,
        string groupCode,
        string courseCode,
        string subjectTitle,
        params SelectedScheduleMeeting[] meetings) =>
        new(Id(id), groupCode, courseCode, subjectTitle, meetings);

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
