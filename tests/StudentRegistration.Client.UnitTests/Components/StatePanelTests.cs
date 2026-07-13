using Bunit;
using StudentRegistration.Client.Components.Feedback;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.UX;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class StatePanelTests
{
    [Fact]
    public void Renders_status_content_reference_and_native_next_actions()
    {
        using var context = new BunitContext();
        var status = new UiStatus(
            "PLAN_CHANGED",
            "Plan changed",
            "Review the latest plan before continuing.",
            UiStatusSeverity.Warning,
            [new UiStatusAction("review", "Review plan", "/student/schedule")],
            "REF-42");

        var panel = context.Render<RouteStatePanel>(parameters => parameters
            .Add(component => component.HeadingId, "state-heading")
            .Add(component => component.State, RouteUiState.Stale)
            .Add(component => component.Status, status)
            .Add(component => component.ReferenceLabel, "Reference")
            .Add(component => component.Announcement, RouteStatePanel.AnnouncementMode.Polite));

        var section = panel.Find("section");
        Assert.Equal("state-heading", section.GetAttribute("aria-labelledby"));
        Assert.Equal("status", section.GetAttribute("role"));
        Assert.Equal("polite", section.GetAttribute("aria-live"));
        Assert.Equal("stale", section.GetAttribute("data-state"));
        Assert.Equal("Plan changed", panel.Find("#state-heading").TextContent);
        Assert.Equal("/student/schedule", panel.Find("a").GetAttribute("href"));
        Assert.Contains("REF-42", panel.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Loading_error_and_disabled_action_states_preserve_safe_text()
    {
        using var context = new BunitContext();
        var status = new UiStatus(
            "SERVICE_UNAVAILABLE",
            "Service unavailable",
            "Try the request again later.",
            UiStatusSeverity.Error,
            [new UiStatusAction("retry", "Retry", "/student")],
            null);

        var panel = context.Render<RouteStatePanel>(parameters => parameters
            .Add(component => component.HeadingId, "error-heading")
            .Add(component => component.State, RouteUiState.Loading)
            .Add(component => component.Status, status)
            .Add(component => component.ActionsDisabled, true)
            .Add(component => component.Announcement, RouteStatePanel.AnnouncementMode.Assertive));

        var section = panel.Find("section");
        Assert.Equal("true", section.GetAttribute("aria-busy"));
        Assert.Equal("alert", section.GetAttribute("role"));
        Assert.Equal("assertive", section.GetAttribute("aria-live"));
        Assert.Empty(panel.FindAll("a"));
        Assert.Equal("true", panel.Find("[aria-disabled=true]").GetAttribute("aria-disabled"));
        Assert.Contains("Try the request again later.", panel.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Action_styles_cover_hover_active_focus_disabled_and_token_only_states()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Feedback/RouteStatePanel.razor.css");

        RepositoryFiles.ContainsAll(
            css,
            ":hover",
            ":active",
            ":focus-visible",
            "[aria-disabled=\"true\"]",
            "[aria-busy=\"true\"]",
            "var(--srs-");
        Assert.DoesNotContain("#", css, StringComparison.Ordinal);
    }
}
