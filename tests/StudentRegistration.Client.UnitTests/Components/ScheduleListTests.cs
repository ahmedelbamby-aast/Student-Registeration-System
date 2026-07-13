using Bunit;
using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ScheduleListTests
{
    [Fact]
    public void List_preserves_the_calendar_collection_order_and_complete_meeting_content()
    {
        using var context = new BunitContext();

        var cut = Render(context);

        var region = cut.Find("section[role=region]");
        Assert.Equal("schedule-list", region.Id);
        Assert.Equal("schedule-calendar", region.GetAttribute("aria-describedby"));
        Assert.Equal("polite", region.GetAttribute("aria-live"));
        Assert.Equal("default", region.GetAttribute("data-state"));

        var items = cut.FindAll("ol > li");
        Assert.Equal(["meeting-2", "meeting-1"],
            items.Select(item => item.GetAttribute("data-meeting-id")).ToArray());
        Assert.Contains("DS221 Data Science", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("G02", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Lecture", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Dr. Salma", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("TA Noor", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Room C201", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Monday", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("10:00", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("11:30", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Africa/Cairo", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Conflict", items[0].TextContent, StringComparison.Ordinal);

        var link = items[0].QuerySelector("a");
        Assert.NotNull(link);
        Assert.Equal("Open Data Science lecture", link.GetAttribute("aria-label"));
        Assert.Null(link.GetAttribute("tabindex"));
    }

    [Fact]
    public void Disabled_loading_and_error_states_are_explicit_and_have_no_false_action()
    {
        using var context = new BunitContext();

        var disabled = Render(context, isDisabled: true, disabledReason: "Schedule is read only");
        Assert.Equal("disabled", disabled.Find("section").GetAttribute("data-state"));
        Assert.Empty(disabled.FindAll("a"));
        Assert.Equal("Schedule is read only", disabled.Find("[data-disabled-reason]").TextContent.Trim());

        var loading = Render(context, isLoading: true, loadingText: "Refreshing schedule");
        Assert.Equal("loading", loading.Find("section").GetAttribute("data-state"));
        Assert.Equal("Refreshing schedule", loading.Find("[role=status]").TextContent.Trim());

        var error = Render(context, errorMessage: "Schedule is unavailable");
        Assert.Equal("error", error.Find("section").GetAttribute("data-state"));
        Assert.Equal("Schedule is unavailable", error.Find("[role=alert]").TextContent.Trim());
    }

    [Fact]
    public void Native_action_styles_cover_hover_active_and_focus_with_tokens()
    {
        var styles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Scheduling/ScheduleList.razor.css");

        RepositoryFiles.ContainsAll(
            styles,
            ":hover",
            ":active",
            ":focus-visible",
            "var(--srs-");
    }

    private static IRenderedComponent<ScheduleList> Render(
        BunitContext context,
        bool isDisabled = false,
        string? disabledReason = null,
        bool isLoading = false,
        string? loadingText = null,
        string? errorMessage = null) =>
        context.Render<ScheduleList>(parameters => parameters
            .Add(component => component.ComponentId, "schedule-list")
            .Add(component => component.EquivalentCalendarId, "schedule-calendar")
            .Add(component => component.Heading, "Schedule list")
            .Add(component => component.GroupLabel, "Group")
            .Add(component => component.ActivityKindLabel, "Activity")
            .Add(component => component.LecturerLabel, "Lecturer")
            .Add(component => component.TeachingAssistantsLabel, "Teaching assistants")
            .Add(component => component.LocationLabel, "Location")
            .Add(component => component.DayLabel, "Day")
            .Add(component => component.StartLabel, "Start")
            .Add(component => component.EndLabel, "End")
            .Add(component => component.TimezoneLabel, "Timezone")
            .Add(component => component.ConflictLabel, "Status")
            .Add(component => component.Meetings, ScheduleCalendarTests.CreateMeetings())
            .Add(component => component.IsDisabled, isDisabled)
            .Add(component => component.DisabledReason, disabledReason)
            .Add(component => component.IsLoading, isLoading)
            .Add(component => component.LoadingText, loadingText)
            .Add(component => component.ErrorMessage, errorMessage));
}
