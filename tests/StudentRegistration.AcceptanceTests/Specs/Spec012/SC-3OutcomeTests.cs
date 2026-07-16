using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class SC_3OutcomeTests
{
    [Fact]
    public void Stu_04_binds_both_rendered_views_to_the_same_meeting_collection()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor");
        var browserEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs");

        Assert.Equal(2, page.Split("Meetings=\"@_meetingItems\"").Length - 1);
        Assert.Contains(".srs-schedule-calendar [data-meeting-id]", browserEvidence);
        Assert.Contains(".srs-schedule-list [data-meeting-id]", browserEvidence);
        Assert.Contains("Assert.Equal(calendarMeetings, listMeetings)", browserEvidence);
    }

    [Fact]
    public void Calendar_and_chronological_list_receive_the_same_canonical_schedule_content()
    {
        var meetings = Spec012ConflictFixture.Meetings();

        var state = ConflictStateMapper.Map(
            [Spec012ConflictFixture.Overlap()],
            meetings);

        Assert.Same(state.CalendarMeetings, state.ChronologicalMeetings);
        Assert.Equal(
            state.CalendarMeetings.Select(MeetingSemantics),
            state.ChronologicalMeetings.Select(MeetingSemantics));
        Assert.All(state.CalendarMeetings, meeting =>
            Assert.Equal("Conflict", meeting.ConflictStatusText));
    }

    private static object MeetingSemantics(
        StudentRegistration.Client.Components.Scheduling.ScheduleCalendar.ScheduleMeetingItem meeting) =>
        new
        {
            meeting.MeetingId,
            meeting.SubjectCode,
            meeting.SubjectName,
            meeting.GroupCode,
            meeting.ActivityKind,
            meeting.LecturerName,
            Assistants = string.Join('|', meeting.TeachingAssistantNames),
            meeting.Location,
            meeting.Day,
            meeting.StartsAt,
            meeting.EndsAt,
            meeting.Timezone,
            meeting.ConflictStatusText,
            meeting.DetailsHref
        };
}
