using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Scheduling;

namespace StudentRegistration.Client.UnitTests.Scheduling;

public sealed class ConflictStateMapperTests
{
    [Fact]
    public void Maps_server_overlap_to_accessible_blocking_state_without_recalculating_it()
    {
        var conflict = Conflict();

        var state = ConflictStateMapper.Map([conflict], Meetings());

        Assert.True(state.ReviewBlocked);
        Assert.Equal("✕", state.IconText);
        Assert.Equal("Conflict", state.StatusText);
        Assert.Equal([conflict.Message], state.BlockingReasons);

        var mapped = Assert.Single(state.Conflicts);
        Assert.Same(conflict, mapped.Source);
        Assert.Equal(2, mapped.Panel.SubjectGroups.Count);
        Assert.Equal(new TimeOnly(11, 0), Assert.Single(mapped.Panel.OverlapSlots).StartsAt);
        Assert.Equal(new TimeOnly(11, 30), Assert.Single(mapped.Panel.OverlapSlots).EndsAt);
        Assert.Equal(4, mapped.Panel.ResolutionLinks.Count);
        Assert.Empty(mapped.Panel.Alternatives);
    }

    [Fact]
    public void Preserves_server_actions_and_one_canonical_calendar_list_collection()
    {
        var meetings = Meetings();

        var state = ConflictStateMapper.Map([Conflict()], meetings);

        Assert.Same(state.CalendarMeetings, state.ChronologicalMeetings);
        Assert.Equal(meetings, state.CalendarMeetings);
        Assert.Equal(
            ["Change DS221 group", "Remove DS221 group", "Change AI301 group", "Remove AI301 group"],
            Assert.Single(state.Conflicts).Panel.ResolutionLinks.Select(link => link.Label));
        Assert.Collection(
            Assert.Single(state.Conflicts).Panel.ResolutionLinks,
            action => AssertCommand(action, "change-group", "group-1"),
            action => AssertCommand(action, "remove-group", "group-1"),
            action => AssertCommand(action, "change-group", "group-2"),
            action => AssertCommand(action, "remove-group", "group-2"));
    }

    [Fact]
    public void Rejects_reserved_travel_conflicts_and_incomplete_resolution_actions()
    {
        var travel = Conflict() with { Code = "TRAVEL_BUFFER" };
        Assert.Throws<ArgumentException>(() =>
            ConflictStateMapper.Map([travel], Meetings()));

        var missingRemove = Conflict() with
        {
            Actions = Conflict().Actions
                .Where(action => action.Action == "change-group")
                .ToArray()
        };
        Assert.Throws<ArgumentException>(() =>
            ConflictStateMapper.Map([missingRemove], Meetings()));
    }

    private static ServerScheduleConflictPresentation Conflict() =>
        new(
            "MEETING_OVERLAP",
            new("group-1", "G01", "DS221", "Data Science", new TimeOnly(10, 0), new TimeOnly(11, 30)),
            new("group-2", "G02", "AI301", "Machine Learning", new TimeOnly(11, 0), new TimeOnly(12, 0)),
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "DS221 G01 overlaps AI301 G02 on Monday from 11:00 to 11:30.",
            [
                new("change-group", "group-1", "Change DS221 group", "/student/subjects/offering-1"),
                new("remove-group", "group-1", "Remove DS221 group", "/student/schedule"),
                new("change-group", "group-2", "Change AI301 group", "/student/subjects/offering-2"),
                new("remove-group", "group-2", "Remove AI301 group", "/student/schedule")
            ]);

    private static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> Meetings() =>
    [
        new(
            "meeting-1", "DS221", "Data Science", "G01", "Lecture", "Dr. Salma", [],
            "Room C201", "Monday", "10:00", "11:30", "Africa/Cairo", "Conflict",
            "/student/subjects/offering-1", "Open Data Science group details"),
        new(
            "meeting-2", "AI301", "Machine Learning", "G02", "Lecture", "Dr. Nadia", [],
            "Room A101", "Monday", "11:00", "12:00", "Africa/Cairo", "Conflict",
            "/student/subjects/offering-2", "Open Machine Learning group details")
    ];

    private static void AssertCommand(
        StudentRegistration.Client.Features.Frontend.Models.ConflictResolutionLinkView action,
        string expectedAction,
        string expectedGroupId)
    {
        Assert.True(action.IsCommand);
        Assert.Equal(expectedAction, action.Action);
        Assert.Equal(expectedGroupId, action.TargetGroupId);
    }
}
