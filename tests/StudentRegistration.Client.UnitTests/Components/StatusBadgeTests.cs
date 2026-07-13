using Bunit;
using StudentRegistration.Client.Components.Feedback;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class StatusBadgeTests
{
    [Fact]
    public void Presents_status_with_visible_text_and_a_decorative_icon()
    {
        using var context = new BunitContext();

        var badge = context.Render<StatusBadge>(parameters => parameters
            .Add(component => component.Text, "Available")
            .Add(component => component.Icon, "OK")
            .Add(component => component.Tone, StatusBadge.BadgeTone.Success));

        var root = badge.Find("span.srs-status-badge");
        Assert.Contains("srs-status-badge--success", root.ClassList);
        Assert.Equal("Available", badge.Find(".srs-status-badge__text").TextContent);
        Assert.Equal("true", badge.Find(".srs-status-badge__icon").GetAttribute("aria-hidden"));
        Assert.Null(root.GetAttribute("style"));
    }

    [Fact]
    public void Loading_error_and_disabled_states_are_textual_and_announced()
    {
        using var context = new BunitContext();

        var loading = context.Render<StatusBadge>(parameters => parameters
            .Add(component => component.Text, "Capacity")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingText, "Checking capacity"));
        Assert.Equal("status", loading.Find(".srs-status-badge").GetAttribute("role"));
        Assert.Equal("polite", loading.Find(".srs-status-badge").GetAttribute("aria-live"));
        Assert.Equal("Checking capacity", loading.Find(".srs-status-badge__text").TextContent);

        var error = context.Render<StatusBadge>(parameters => parameters
            .Add(component => component.Text, "Capacity")
            .Add(component => component.ErrorText, "Capacity is unavailable"));
        Assert.Equal("alert", error.Find(".srs-status-badge").GetAttribute("role"));
        Assert.Equal("Capacity is unavailable", error.Find(".srs-status-badge__text").TextContent);

        var disabled = context.Render<StatusBadge>(parameters => parameters
            .Add(component => component.Text, "Unavailable")
            .Add(component => component.IsDisabled, true));
        Assert.Equal("true", disabled.Find(".srs-status-badge").GetAttribute("aria-disabled"));
        Assert.Contains("srs-status-badge--disabled", disabled.Find(".srs-status-badge").ClassList);
    }

    [Fact]
    public void Badge_is_noninteractive_and_uses_token_classes_for_every_visual_state()
    {
        using var context = new BunitContext();
        var badge = context.Render<StatusBadge>(parameters => parameters
            .Add(component => component.Text, "Open"));

        Assert.Empty(badge.FindAll("button, a, [tabindex]"));

        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Feedback/StatusBadge.razor.css");
        RepositoryFiles.ContainsAll(
            css,
            "--information",
            "--success",
            "--warning",
            "--danger",
            "--pending",
            "--disabled",
            "var(--srs-");
        Assert.DoesNotContain("#", css, StringComparison.Ordinal);
    }
}
