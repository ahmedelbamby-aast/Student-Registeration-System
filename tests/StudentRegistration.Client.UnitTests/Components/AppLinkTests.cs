using Bunit;
using StudentRegistration.Client.Components.Navigation;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class AppLinkTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Navigation/AppLink.razor.css";

    [Fact]
    public void Renders_a_native_named_anchor_with_active_state_and_callback()
    {
        using var context = new BunitContext();
        var invocations = 0;

        var cut = context.Render<AppLink>(parameters => parameters
            .Add(component => component.Href, "/student/subjects")
            .Add(component => component.IsActive, true)
            .Add(component => component.AccessibleLabel, "Open available subjects")
            .Add(component => component.ChildContent, "Subjects")
            .Add(component => component.OnClick, () => invocations++));

        var link = cut.Find("a");
        Assert.Equal("/student/subjects", link.GetAttribute("href"));
        Assert.Equal("page", link.GetAttribute("aria-current"));
        Assert.Equal("Open available subjects", link.GetAttribute("aria-label"));
        Assert.Contains("Subjects", link.TextContent, StringComparison.Ordinal);

        link.Click();
        Assert.Equal(1, invocations);
    }

    [Fact]
    public void Disabled_and_loading_links_have_no_navigation_target_or_callback()
    {
        using var context = new BunitContext();
        var invocations = 0;

        var disabled = context.Render<AppLink>(parameters => parameters
            .Add(component => component.Href, "/admin")
            .Add(component => component.IsDisabled, true)
            .Add(component => component.ChildContent, "Admin")
            .Add(component => component.OnClick, () => invocations++));
        var disabledLink = disabled.Find("a");
        Assert.Null(disabledLink.GetAttribute("href"));
        Assert.Equal("true", disabledLink.GetAttribute("aria-disabled"));
        Assert.Equal("-1", disabledLink.GetAttribute("tabindex"));
        disabledLink.Click();

        var loading = context.Render<AppLink>(parameters => parameters
            .Add(component => component.Href, "/student")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingAnnouncement, "Loading destination")
            .Add(component => component.ChildContent, "Dashboard")
            .Add(component => component.OnClick, () => invocations++));
        var loadingLink = loading.Find("a");
        Assert.Null(loadingLink.GetAttribute("href"));
        Assert.Equal("true", loadingLink.GetAttribute("aria-busy"));
        Assert.Contains("Loading destination", loading.Markup, StringComparison.Ordinal);
        loadingLink.Click();

        Assert.Equal(0, invocations);
    }

    [Fact]
    public void Error_description_and_interaction_styles_use_only_tokens()
    {
        using var context = new BunitContext();
        var cut = context.Render<AppLink>(parameters => parameters
            .Add(component => component.Href, "/status/503")
            .Add(component => component.HasError, true)
            .Add(component => component.DescribedBy, "link-error")
            .Add(component => component.ChildContent, "Status"));

        var link = cut.Find("a");
        Assert.Equal("link-error", link.GetAttribute("aria-describedby"));
        Assert.Contains("has-error", link.GetAttribute("class"), StringComparison.Ordinal);

        var css = RepositoryFiles.Read(CssPath);
        RepositoryFiles.ContainsAll(
            css,
            ".srs-link:hover:not([aria-disabled='true'])",
            ".srs-link:active:not([aria-disabled='true'])",
            ".srs-link:focus-visible",
            ".srs-link[aria-disabled='true']",
            ".srs-link.is-loading",
            ".srs-link.has-error",
            "var(--srs-");
    }
}
