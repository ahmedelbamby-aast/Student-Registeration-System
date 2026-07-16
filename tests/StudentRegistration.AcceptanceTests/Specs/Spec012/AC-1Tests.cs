using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class AC_1Tests
{
    [Fact]
    public void Overlap_returns_exact_details_accessible_actions_and_ui_contract()
    {
        var first = Group(
            1,
            "A",
            "AI401",
            "Artificial Intelligence",
            Meeting(11, 10, 0, 11, 30));
        var second = Group(
            2,
            "B",
            "CS402",
            "Distributed Systems",
            Meeting(22, 11, 0, 12, 0));

        var conflict = Assert.Single(
            new ScheduleConflictDetector().Detect([second, first]));

        Assert.Equal("MEETING_OVERLAP", conflict.Code);
        Assert.Equal(DayOfWeek.Monday, conflict.DayOfWeek);
        Assert.Equal(new TimeOnly(11, 0), conflict.OverlapStartLocal);
        Assert.Equal(new TimeOnly(11, 30), conflict.OverlapEndLocal);
        Assert.Equal("AI401", conflict.First.CourseCode);
        Assert.Equal("Artificial Intelligence", conflict.First.SubjectTitle);
        Assert.Equal("CS402", conflict.Second.CourseCode);
        Assert.Equal("Distributed Systems", conflict.Second.SubjectTitle);
        Assert.Contains("Conflict", conflict.Message, StringComparison.Ordinal);
        Assert.Equal(4, conflict.Actions.Count);
        Assert.Contains(
            conflict.Actions,
            action => action.Action == "change-group" && action.TargetGroupId == first.GroupId);
        Assert.Contains(
            conflict.Actions,
            action => action.Action == "remove-group" && action.TargetGroupId == second.GroupId);

        var accessibleState = ConflictStateMapper.Map(
            [Present(conflict)],
            []);
        Assert.True(accessibleState.ReviewBlocked);
        Assert.Equal("✕", accessibleState.IconText);
        Assert.Equal("Conflict", accessibleState.StatusText);
        var mapped = Assert.Single(accessibleState.Conflicts);
        Assert.Equal(
            ["AI401:A", "CS402:B"],
            mapped.Panel.SubjectGroups.Select(subject => subject.Reference));
        Assert.Equal(
            [
                "Change AI401 group A",
                "Remove AI401 group A",
                "Change CS402 group B",
                "Remove CS402 group B"
            ],
            mapped.Panel.ResolutionLinks.Select(link => link.Label));

        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-04.md");
        RepositoryFiles.ContainsAll(
            design,
            "Red X icon plus visible word Conflict",
            "Named subjects/groups and each exact overlap day, start, and end",
            "Manual Change group and Remove action");
    }

    private static ServerScheduleConflictPresentation Present(
        ScheduleConflict conflict) =>
        new(
            conflict.Code,
            Present(conflict.First),
            Present(conflict.Second),
            conflict.DayOfWeek,
            conflict.OverlapStartLocal,
            conflict.OverlapEndLocal,
            conflict.Message,
            conflict.Actions
                .Select(action => new ServerConflictResolutionActionPresentation(
                    action.Action,
                    action.TargetGroupId.ToString(),
                    action.Label,
                    action.Route))
                .ToArray());

    private static ServerConflictGroupPresentation Present(
        ScheduleConflictParticipant participant) =>
        new(
            participant.GroupId.ToString(),
            participant.GroupCode,
            participant.CourseCode,
            participant.SubjectTitle,
            participant.StartLocal,
            participant.EndLocal);

    private static SelectedScheduleGroup Group(
        int id,
        string groupCode,
        string courseCode,
        string subjectTitle,
        params SelectedScheduleMeeting[] meetings) =>
        new(Id(id), groupCode, courseCode, subjectTitle, meetings);

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
