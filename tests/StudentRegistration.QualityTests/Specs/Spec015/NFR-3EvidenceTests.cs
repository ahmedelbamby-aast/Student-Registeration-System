using System.Text.Json;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec015;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public void Delivered_calendar_list_and_browser_print_share_one_accessible_semantic_model()
    {
        var group = SyntheticGroup();
        var semanticMeetings = RegistrationPageSupport.ToMeetingItems(
            [group],
            "Africa/Cairo");
        var meeting = Assert.Single(semanticMeetings);

        Assert.Equal(group.Meetings[0].MeetingId.ToString("D"), meeting.MeetingId);
        Assert.Equal(group.CourseCode, meeting.SubjectCode);
        Assert.Equal(group.SubjectTitle, meeting.SubjectName);
        Assert.Equal(group.GroupCode, meeting.GroupCode);
        Assert.Equal("Monday", meeting.Day);
        Assert.Equal("09:00", meeting.StartsAt);
        Assert.Equal("10:30", meeting.EndsAt);
        Assert.Equal("Africa/Cairo", meeting.Timezone);
        Assert.Equal("Synthetic lecturer", meeting.LecturerName);
        Assert.Equal(["Synthetic teaching assistant"], meeting.TeachingAssistantNames);

        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor");
        var calendar = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Scheduling/ScheduleCalendar.razor");
        var list = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Scheduling/ScheduleList.razor");
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor.css");

        RepositoryFiles.ContainsAll(
            page,
            "<main id=\"main-content\" tabindex=\"-1\">",
            "<ScheduleCalendar",
            "EquivalentListId=\"registration-history-list\"",
            "Meetings=\"@_meetings\"",
            "<ScheduleList",
            "EquivalentCalendarId=\"registration-history-list-calendar-heading\"",
            "<caption>Current and archived registration outcomes</caption>",
            "<th scope=\"col\">Reference</th>",
            "aria-label=\"Registration records table\"",
            "private Task PrintAsync() => JavaScript.InvokeVoidAsync(\"window.print\")");
        RepositoryFiles.ContainsAll(
            calendar,
            "aria-describedby=\"@EquivalentListId\"",
            "<th scope=\"col\">@DayColumnLabel</th>",
            "data-meeting-id=\"@meeting.MeetingId\"");
        RepositoryFiles.ContainsAll(
            list,
            "aria-describedby=\"@EquivalentCalendarId\"",
            "data-meeting-id=\"@meeting.MeetingId\"",
            "<time>@meeting.StartsAt</time>");
        RepositoryFiles.ContainsAll(
            css,
            "@media print",
            ".srs-registration-history__intro button",
            "display: none");
        Assert.DoesNotContain("Export", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("csv", page, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Printed_record_contract_is_pii_minimized_and_contains_no_student_identity_fields()
    {
        var term = new RegistrationTermSnapshotDto(
            Guid.Parse("15000000-0000-0000-0000-000000000601"),
            "2040-FALL",
            "Fall 2040",
            "Africa/Cairo");
        var page = new Page<RegistrationHistoryRowDto>(
            [new(
                Guid.Parse("15000000-0000-0000-0000-000000000602"),
                "REG-SYNTHETIC",
                term,
                "accepted",
                new DateTime(2040, 7, 17, 8, 0, 0, DateTimeKind.Utc),
                1,
                3m,
                "archived")],
            1,
            20,
            1,
            "submittedAtUtc:desc,submissionId:asc");
        var json = JsonSerializer.Serialize(page, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var allowedHistoryFields = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(RegistrationHistoryRowDto.SubmissionId),
            nameof(RegistrationHistoryRowDto.Reference),
            nameof(RegistrationHistoryRowDto.Term),
            nameof(RegistrationHistoryRowDto.Status),
            nameof(RegistrationHistoryRowDto.SubmittedAtUtc),
            nameof(RegistrationHistoryRowDto.GroupCount),
            nameof(RegistrationHistoryRowDto.TotalCredits),
            nameof(RegistrationHistoryRowDto.TermState)
        };
        Assert.Equal(
            allowedHistoryFields.OrderBy(value => value),
            typeof(RegistrationHistoryRowDto).GetProperties()
                .Select(property => property.Name)
                .OrderBy(value => value));

        string[] forbidden =
        [
            "studentId", "applicationUserId", "universityId", "studentName",
            "email", "phone", "address", "currentGpa", "earnedCredits",
            "password", "credential", "securityStamp", "cookie"
        ];
        Assert.All(forbidden, field =>
            Assert.DoesNotContain(field, json, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("REG-SYNTHETIC", json, StringComparison.Ordinal);
        Assert.Contains("2040-FALL", json, StringComparison.Ordinal);
    }

    private static RegistrationRecordGroupDto SyntheticGroup() => new(
        Guid.Parse("15000000-0000-0000-0000-000000000611"),
        "CS015",
        "Synthetic receipt quality",
        Guid.Parse("15000000-0000-0000-0000-000000000612"),
        "L1",
        3m,
        [new(
            Guid.Parse("15000000-0000-0000-0000-000000000613"),
            "Lecture",
            1,
            "09:00",
            "10:30",
            "R-015",
            "Synthetic room",
            [
                new("Lecturer", "Synthetic lecturer"),
                new("TeachingAssistant", "Synthetic teaching assistant")
            ])]);
}
