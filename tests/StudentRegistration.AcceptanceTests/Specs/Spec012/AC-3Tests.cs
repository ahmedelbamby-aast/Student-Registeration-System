using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Scheduling;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class AC_3Tests
{
    [Fact]
    public void Unresolved_hard_conflict_blocks_review_with_visible_reason_and_resolution_links()
    {
        var state = ConflictStateMapper.Map(
            [Spec012ConflictFixture.Overlap()],
            Spec012ConflictFixture.Meetings());

        Assert.True(state.ReviewBlocked);
        Assert.Equal("Conflict", state.StatusText);
        Assert.Equal("✕", state.IconText);
        Assert.Contains(
            "DS221 G01 overlaps AI301 G02 on Monday from 11:00 to 11:30.",
            state.BlockingReasons);

        var links = Assert.Single(state.Conflicts).Panel.ResolutionLinks;
        Assert.Equal(4, links.Count);
        Assert.Contains(links, link => link.Label == "Change DS221 group");
        Assert.Contains(links, link => link.Label == "Remove AI301 group");
    }
}

internal static class Spec012ConflictFixture
{
    internal static ServerScheduleConflictPresentation Overlap() =>
        new(
            "MEETING_OVERLAP",
            new(
                "group-ds221-g01",
                "G01",
                "DS221",
                "Data Science",
                new TimeOnly(10, 0),
                new TimeOnly(11, 30)),
            new(
                "group-ai301-g02",
                "G02",
                "AI301",
                "Machine Learning",
                new TimeOnly(11, 0),
                new TimeOnly(12, 0)),
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "DS221 G01 overlaps AI301 G02 on Monday from 11:00 to 11:30.",
            [
                new("change-group", "group-ds221-g01", "Change DS221 group", "/student/subjects/offering-ds221"),
                new("remove-group", "group-ds221-g01", "Remove DS221 group", "/student/schedule"),
                new("change-group", "group-ai301-g02", "Change AI301 group", "/student/subjects/offering-ai301"),
                new("remove-group", "group-ai301-g02", "Remove AI301 group", "/student/schedule")
            ]);

    internal static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> Meetings() =>
    [
        new(
            "meeting-ds221",
            "DS221",
            "Data Science",
            "G01",
            "Lecture",
            "Dr. Salma",
            [],
            "Room C201",
            "Monday",
            "10:00",
            "11:30",
            "Africa/Cairo",
            "Conflict",
            "/student/subjects/offering-ds221",
            "Open Data Science group details"),
        new(
            "meeting-ai301",
            "AI301",
            "Machine Learning",
            "G02",
            "Lecture",
            "Dr. Nadia",
            [],
            "Room A101",
            "Monday",
            "11:00",
            "12:00",
            "Africa/Cairo",
            "Conflict",
            "/student/subjects/offering-ai301",
            "Open Machine Learning group details")
    ];
}
