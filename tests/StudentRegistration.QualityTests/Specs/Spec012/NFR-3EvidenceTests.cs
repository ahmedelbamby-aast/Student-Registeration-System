using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec012;

public sealed class NFR_3EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-012-NFR-3.md";

    [Fact]
    public void Rendered_stu_04_proves_calendar_list_meeting_id_equivalence()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor");
        var browserEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs");

        Assert.Equal(2, page.Split("Meetings=\"@_meetingItems\"").Length - 1);
        RepositoryFiles.ContainsAll(
            browserEvidence,
            ".srs-schedule-calendar [data-meeting-id]",
            ".srs-schedule-list [data-meeting-id]",
            "Assert.Equal(calendarMeetings, listMeetings)",
            "Assert.Equal(2, calendarMeetings.Length)");
    }

    [Fact]
    public void Calendar_and_list_share_one_canonical_meeting_collection()
    {
        var meetings = Meetings();

        var state = ConflictStateMapper.Map([Conflict()], meetings);

        Assert.Same(state.CalendarMeetings, state.ChronologicalMeetings);
        Assert.Equal(meetings.Count, state.CalendarMeetings.Count);
        Assert.Equal(
            meetings.Select(meeting => meeting.MeetingId),
            state.CalendarMeetings.Select(meeting => meeting.MeetingId));
    }

    [Fact]
    public void Every_required_meeting_semantic_is_identical_in_both_views()
    {
        var state = ConflictStateMapper.Map([Conflict()], Meetings());

        var calendar = state.CalendarMeetings.Select(Semantics).ToArray();
        var chronological = state.ChronologicalMeetings.Select(Semantics).ToArray();

        Assert.Equal(calendar, chronological);
        Assert.All(calendar, meeting => Assert.Equal("Conflict", meeting.ConflictStatusText));
        Assert.Contains(calendar, meeting => meeting.LecturerName == "Dr. Salma");
        Assert.Contains(calendar, meeting => meeting.TeachingAssistants == "TA Noor");
        Assert.Contains(calendar, meeting => meeting.Timezone == "Africa/Cairo");
    }

    [Fact]
    public void Evidence_records_the_bounded_semantic_equivalence_scope()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-012 NFR-3 Calendar/List Semantic Equivalence Evidence",
            "NFR-3",
            "ConflictStateMapper",
            "same canonical meeting collection",
            "meeting ID",
            "subject code and title",
            "group code",
            "activity kind",
            "Lecturer",
            "Teaching Assistant",
            "room/location",
            "day",
            "start/end",
            "timezone",
            "conflict text",
            "ScheduleBuilderPage",
            "rendered STU-04 browser",
            "proof reads every `data-meeting-id`",
            "same `_meetingItems` instance",
            "Policy and final-submission behavior remain separate evidence",
            "**Focused verification:**");
    }

    private static MeetingSemantics Semantics(
        ScheduleCalendar.ScheduleMeetingItem meeting) =>
        new(
            meeting.MeetingId,
            meeting.SubjectCode,
            meeting.SubjectName,
            meeting.GroupCode,
            meeting.ActivityKind,
            meeting.LecturerName,
            string.Join('|', meeting.TeachingAssistantNames),
            meeting.Location,
            meeting.Day,
            meeting.StartsAt,
            meeting.EndsAt,
            meeting.Timezone,
            meeting.ConflictStatusText,
            meeting.DetailsHref,
            meeting.DetailsAccessibleName);

    private static ServerScheduleConflictPresentation Conflict() =>
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

    private static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> Meetings() =>
    [
        new(
            "meeting-ds221-lecture",
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
            "meeting-ai301-tutorial",
            "AI301",
            "Machine Learning",
            "G02",
            "Tutorial",
            null,
            ["TA Noor"],
            "Lab A101",
            "Monday",
            "11:00",
            "12:00",
            "Africa/Cairo",
            "Conflict",
            "/student/subjects/offering-ai301",
            "Open Machine Learning group details")
    ];

    private sealed record MeetingSemantics(
        string MeetingId,
        string SubjectCode,
        string SubjectTitle,
        string GroupCode,
        string ActivityKind,
        string? LecturerName,
        string TeachingAssistants,
        string Location,
        string Day,
        string StartsAt,
        string EndsAt,
        string Timezone,
        string ConflictStatusText,
        string DetailsHref,
        string DetailsAccessibleName);
}
