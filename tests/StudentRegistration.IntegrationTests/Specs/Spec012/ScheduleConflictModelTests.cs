using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012;

public sealed class ScheduleConflictModelTests
{
    [Fact]
    public void Conflict_preserves_both_groups_courses_intervals_overlap_and_actions()
    {
        var firstGroupId = Guid.NewGuid();
        var secondGroupId = Guid.NewGuid();
        var first = Participant(
            firstGroupId,
            "G-A",
            "CS101",
            "Algorithms",
            new TimeOnly(10, 0),
            new TimeOnly(11, 30));
        var second = Participant(
            secondGroupId,
            "G-B",
            "DS201",
            "Data Science",
            new TimeOnly(11, 0),
            new TimeOnly(12, 0));
        var actions = Actions(firstGroupId, secondGroupId).ToList();

        var conflict = new ScheduleConflict(
            "MEETING_OVERLAP",
            first,
            second,
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "Algorithms overlaps Data Science.",
            actions);
        actions.Clear();

        Assert.Equal("MEETING_OVERLAP", conflict.Code);
        Assert.Equal(firstGroupId, conflict.First.GroupId);
        Assert.Equal("CS101", conflict.First.CourseCode);
        Assert.Equal(new TimeOnly(10, 0), conflict.First.StartLocal);
        Assert.Equal(secondGroupId, conflict.Second.GroupId);
        Assert.Equal("DS201", conflict.Second.CourseCode);
        Assert.Equal(new TimeOnly(12, 0), conflict.Second.EndLocal);
        Assert.Equal(DayOfWeek.Monday, conflict.DayOfWeek);
        Assert.Equal(new TimeOnly(11, 0), conflict.OverlapStartLocal);
        Assert.Equal(new TimeOnly(11, 30), conflict.OverlapEndLocal);
        Assert.Equal("Algorithms overlaps Data Science.", conflict.Message);
        Assert.Equal(4, conflict.Actions.Count);
        Assert.Contains(
            conflict.Actions,
            action => action.Action == "change-group" &&
                action.TargetGroupId == firstGroupId);
        Assert.Contains(
            conflict.Actions,
            action => action.Action == "remove-group" &&
                action.TargetGroupId == secondGroupId);
    }

    [Fact]
    public void Conflict_rejects_invalid_overlap_code_or_incomplete_actions()
    {
        var firstGroupId = Guid.NewGuid();
        var secondGroupId = Guid.NewGuid();
        var first = Participant(
            firstGroupId,
            "G-A",
            "CS101",
            "Algorithms",
            new TimeOnly(10, 0),
            new TimeOnly(11, 30));
        var second = Participant(
            secondGroupId,
            "G-B",
            "DS201",
            "Data Science",
            new TimeOnly(11, 0),
            new TimeOnly(12, 0));

        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "UNKNOWN",
            first,
            second,
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "Conflict.",
            Actions(firstGroupId, secondGroupId)));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP",
            first,
            second,
            DayOfWeek.Monday,
            new TimeOnly(10, 30),
            new TimeOnly(11, 30),
            "Conflict.",
            Actions(firstGroupId, secondGroupId)));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP",
            first,
            second,
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "Conflict.",
            [new ScheduleConflictAction(
                "change-group",
                firstGroupId,
                "Change G-A",
                "/student/subjects/1")]));
    }

    private static ScheduleConflictParticipant Participant(
        Guid groupId,
        string groupCode,
        string courseCode,
        string subjectTitle,
        TimeOnly start,
        TimeOnly end) =>
        new(groupId, groupCode, courseCode, subjectTitle, start, end);

    private static IReadOnlyList<ScheduleConflictAction> Actions(
        Guid firstGroupId,
        Guid secondGroupId) =>
        [
            new("change-group", firstGroupId, "Change G-A", "/student/subjects/1"),
            new("remove-group", firstGroupId, "Remove G-A", "/student/schedule"),
            new("change-group", secondGroupId, "Change G-B", "/student/subjects/2"),
            new("remove-group", secondGroupId, "Remove G-B", "/student/schedule")
        ];
}
