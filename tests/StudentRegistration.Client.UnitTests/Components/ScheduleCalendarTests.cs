using Bunit;
using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ScheduleCalendarTests
{
    [Fact]
    public void Calendar_renders_supplied_meetings_in_order_with_labelled_native_links()
    {
        using var context = new BunitContext();
        var meetings = CreateMeetings();

        var cut = context.Render<ScheduleCalendar>(parameters => parameters
            .Add(component => component.Heading, "Weekly schedule")
            .Add(component => component.EquivalentListId, "schedule-list")
            .Add(component => component.DayColumnLabel, "Day")
            .Add(component => component.TimeColumnLabel, "Time")
            .Add(component => component.MeetingColumnLabel, "Meeting")
            .Add(component => component.GroupLabel, "Group")
            .Add(component => component.ActivityKindLabel, "Activity")
            .Add(component => component.LecturerLabel, "Lecturer")
            .Add(component => component.TeachingAssistantsLabel, "Teaching assistants")
            .Add(component => component.LocationLabel, "Location")
            .Add(component => component.ConflictLabel, "Status")
            .Add(component => component.Meetings, meetings));

        var region = cut.Find("section[role=region]");
        Assert.Equal("schedule-list", region.GetAttribute("aria-describedby"));
        Assert.Equal("polite", region.GetAttribute("aria-live"));
        Assert.Equal("default", region.GetAttribute("data-state"));
        Assert.Equal("Weekly schedule", cut.Find("h2").TextContent.Trim());
        Assert.Equal(["Day", "Time", "Meeting"],
            cut.FindAll("th").Select(cell => cell.TextContent.Trim()).ToArray());

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(["meeting-2", "meeting-1"],
            rows.Select(row => row.GetAttribute("data-meeting-id") ?? string.Empty).ToArray());
        Assert.Contains("Data Science", rows[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Dr. Salma", rows[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("TA Noor", rows[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Room C201", rows[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Conflict", rows[0].TextContent, StringComparison.Ordinal);

        var links = cut.FindAll("tbody a");
        Assert.Equal(2, links.Count);
        Assert.Equal("Open Data Science lecture", links[0].GetAttribute("aria-label"));
        Assert.Null(links[0].GetAttribute("tabindex"));
    }

    [Fact]
    public void Disabled_loading_and_error_states_are_explicit_and_do_not_expose_actions()
    {
        using var context = new BunitContext();

        var disabled = Render(context, isDisabled: true, disabledReason: "Schedule is read only");
        Assert.Equal("disabled", disabled.Find("section").GetAttribute("data-state"));
        Assert.Equal("true", disabled.Find("section").GetAttribute("aria-disabled"));
        Assert.Empty(disabled.FindAll("a"));
        Assert.Equal("Schedule is read only", disabled.Find("[data-disabled-reason]").TextContent.Trim());

        var loading = Render(context, isLoading: true, loadingText: "Refreshing schedule");
        Assert.Equal("loading", loading.Find("section").GetAttribute("data-state"));
        Assert.Equal("true", loading.Find("section").GetAttribute("aria-busy"));
        Assert.Equal("Refreshing schedule", loading.Find("[role=status]").TextContent.Trim());

        var error = Render(context, errorMessage: "Schedule is unavailable");
        Assert.Equal("error", error.Find("section").GetAttribute("data-state"));
        Assert.Equal("Schedule is unavailable", error.Find("[role=alert]").TextContent.Trim());
    }

    [Fact]
    public void Action_styles_cover_hover_active_and_focus_with_tokens()
    {
        var styles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Scheduling/ScheduleCalendar.razor.css");

        RepositoryFiles.ContainsAll(
            styles,
            ":hover",
            ":active",
            ":focus-visible",
            "var(--srs-");
    }

    internal static IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> CreateMeetings() =>
    [
        new(
            "meeting-2",
            "DS221",
            "Data Science",
            "G02",
            "Lecture",
            "Dr. Salma",
            ["TA Noor"],
            "Room C201",
            "Monday",
            "10:00",
            "11:30",
            "Africa/Cairo",
            "Conflict",
            "/student/schedule/meeting-2",
            "Open Data Science lecture"),
        new(
            "meeting-1",
            "GN223",
            "Software Engineering",
            "G01",
            "Section",
            null,
            ["TA Ahmed", "TA Dina"],
            "Lab A12",
            "Tuesday",
            "08:30",
            "10:00",
            "Africa/Cairo",
            "Available",
            "/student/schedule/meeting-1",
            "Open Software Engineering section")
    ];

    private static IRenderedComponent<ScheduleCalendar> Render(
        BunitContext context,
        bool isDisabled = false,
        string? disabledReason = null,
        bool isLoading = false,
        string? loadingText = null,
        string? errorMessage = null) =>
        context.Render<ScheduleCalendar>(parameters => parameters
            .Add(component => component.Heading, "Weekly schedule")
            .Add(component => component.EquivalentListId, "schedule-list")
            .Add(component => component.DayColumnLabel, "Day")
            .Add(component => component.TimeColumnLabel, "Time")
            .Add(component => component.MeetingColumnLabel, "Meeting")
            .Add(component => component.GroupLabel, "Group")
            .Add(component => component.ActivityKindLabel, "Activity")
            .Add(component => component.LecturerLabel, "Lecturer")
            .Add(component => component.TeachingAssistantsLabel, "Teaching assistants")
            .Add(component => component.LocationLabel, "Location")
            .Add(component => component.ConflictLabel, "Status")
            .Add(component => component.Meetings, CreateMeetings())
            .Add(component => component.IsDisabled, isDisabled)
            .Add(component => component.DisabledReason, disabledReason)
            .Add(component => component.IsLoading, isLoading)
            .Add(component => component.LoadingText, loadingText)
            .Add(component => component.ErrorMessage, errorMessage));
}
