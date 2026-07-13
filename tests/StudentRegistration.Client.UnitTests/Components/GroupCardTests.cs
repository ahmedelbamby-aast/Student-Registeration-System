using Bunit;
using Microsoft.AspNetCore.Components;
using StudentRegistration.Client.Components.Registration;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class GroupCardTests
{
    [Fact]
    public void Default_option_uses_native_keyboard_and_named_selected_semantics()
    {
        using var context = new BunitContext();
        var selectedGroupId = string.Empty;

        var cut = context.Render<GroupCard>(parameters => parameters
            .Add(component => component.GroupId, "group-a")
            .Add(component => component.Heading, "Group A")
            .Add(component => component.AccessibleName, "Select Group A")
            .Add(component => component.IsSelected, true)
            .Add(component => component.OnSelected,
                EventCallback.Factory.Create<string>(this, id => selectedGroupId = id))
            .AddChildContent("<span data-testid=\"details\">Lecture details</span>"));

        var option = cut.Find("button");
        Assert.Equal("button", option.GetAttribute("type"));
        Assert.Equal("Select Group A", option.GetAttribute("aria-label"));
        Assert.Equal("true", option.GetAttribute("aria-pressed"));
        Assert.Equal("default", option.GetAttribute("data-state"));
        Assert.Null(option.GetAttribute("tabindex"));

        option.Click();

        Assert.Equal("group-a", selectedGroupId);
        Assert.Contains("Group A", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Lecture details", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Disabled_loading_and_error_states_expose_supplied_text_without_activation()
    {
        using var context = new BunitContext();
        var activationCount = 0;

        var cut = context.Render<GroupCard>(parameters => parameters
            .Add(component => component.GroupId, "group-full")
            .Add(component => component.Heading, "Group Full")
            .Add(component => component.AccessibleName, "Group Full")
            .Add(component => component.IsDisabled, true)
            .Add(component => component.DisabledReason, "No seats remain")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingText, "Refreshing group")
            .Add(component => component.ErrorMessage, "Group details are unavailable")
            .Add(component => component.OnSelected,
                EventCallback.Factory.Create<string>(this, _ => activationCount++)));

        var option = cut.Find("button");
        Assert.True(option.HasAttribute("disabled"));
        Assert.Equal("true", option.GetAttribute("aria-busy"));
        Assert.Equal("loading", option.GetAttribute("data-state"));

        var describedBy = option.GetAttribute("aria-describedby");
        Assert.False(string.IsNullOrWhiteSpace(describedBy));
        Assert.Equal("No seats remain", cut.Find($"#{describedBy}").TextContent.Trim());
        Assert.Equal("Refreshing group", cut.Find("[role=status]").TextContent.Trim());
        Assert.Equal("Group details are unavailable", cut.Find("[role=alert]").TextContent.Trim());

        option.Click();
        Assert.Equal(0, activationCount);
    }

    [Fact]
    public void Isolated_styles_cover_hover_active_focus_and_disabled_with_tokens()
    {
        var styles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/GroupCard.razor.css");

        RepositoryFiles.ContainsAll(
            styles,
            ":hover",
            ":active",
            ":focus-visible",
            ":disabled",
            "var(--srs-");
    }
}
