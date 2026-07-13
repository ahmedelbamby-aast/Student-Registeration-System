using Bunit;
using StudentRegistration.Client.Components.Feedback;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class AlertTests
{
    [Fact]
    public void Uses_status_or_alert_semantics_according_to_urgency()
    {
        using var context = new BunitContext();

        var polite = context.Render<Alert>(parameters => parameters
            .Add(component => component.Message, "Schedule updated")
            .Add(component => component.Urgency, Alert.AlertUrgency.Polite));
        Assert.Equal("status", polite.Find(".srs-alert").GetAttribute("role"));
        Assert.Equal("polite", polite.Find(".srs-alert").GetAttribute("aria-live"));

        var assertive = context.Render<Alert>(parameters => parameters
            .Add(component => component.Message, "Submission failed")
            .Add(component => component.Urgency, Alert.AlertUrgency.Assertive));
        Assert.Equal("alert", assertive.Find(".srs-alert").GetAttribute("role"));
        Assert.Equal("assertive", assertive.Find(".srs-alert").GetAttribute("aria-live"));
    }

    [Fact]
    public void Dismiss_is_a_named_native_button_and_honors_disabled_loading_and_error_states()
    {
        using var context = new BunitContext();
        var dismissed = 0;

        var alert = context.Render<Alert>(parameters => parameters
            .Add(component => component.Message, "Saved")
            .Add(component => component.IsDismissible, true)
            .Add(component => component.DismissLabel, "Dismiss message")
            .Add(component => component.OnDismiss, () => dismissed++));
        var button = alert.Find("button[type=button]");
        Assert.Equal("Dismiss message", button.TextContent);
        button.Click();
        Assert.Equal(1, dismissed);

        var pending = context.Render<Alert>(parameters => parameters
            .Add(component => component.Message, "Saved")
            .Add(component => component.IsDismissible, true)
            .Add(component => component.DismissLabel, "Dismiss message")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingMessage, "Saving")
            .Add(component => component.IsDisabled, true)
            .Add(component => component.ErrorMessage, "Save failed"));
        Assert.Equal("true", pending.Find(".srs-alert").GetAttribute("aria-busy"));
        Assert.True(pending.Find("button").HasAttribute("disabled"));
        Assert.Equal("Save failed", pending.Find(".srs-alert__message").TextContent);
        Assert.Equal("alert", pending.Find(".srs-alert").GetAttribute("role"));
    }

    [Fact]
    public void Dismiss_styles_cover_hover_active_focus_disabled_and_token_only_states()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Feedback/Alert.razor.css");
        RepositoryFiles.ContainsAll(
            css,
            ":hover",
            ":active",
            ":focus-visible",
            ":disabled",
            "[aria-busy=\"true\"]",
            "var(--srs-");
        Assert.DoesNotContain("#", css, StringComparison.Ordinal);
    }
}
