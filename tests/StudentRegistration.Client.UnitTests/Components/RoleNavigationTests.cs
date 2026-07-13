using Bunit;
using StudentRegistration.Client.Components.Navigation;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class RoleNavigationTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Navigation/RoleNavigation.razor.css";

    [Fact]
    public void Renders_a_labelled_native_navigation_with_server_supplied_items()
    {
        using var context = new BunitContext();
        var items = new[]
        {
            new RoleNavigation.NavigationItem("Dashboard", "/student", true),
            new RoleNavigation.NavigationItem("Subjects", "/student/subjects"),
            new RoleNavigation.NavigationItem("Unavailable", "/student/unavailable", false, true)
        };

        var cut = context.Render<RoleNavigation>(parameters => parameters
            .Add(component => component.AccessibleName, "Student navigation")
            .Add(component => component.Items, items));

        var nav = cut.Find("nav");
        Assert.Equal("Student navigation", nav.GetAttribute("aria-label"));
        Assert.Equal(3, cut.FindAll("li").Count);

        var links = cut.FindAll("a");
        Assert.Equal(2, links.Count);
        Assert.Equal("/student", links[0].GetAttribute("href"));
        Assert.Equal("page", links[0].GetAttribute("aria-current"));
        Assert.Equal("/student/subjects", links[1].GetAttribute("href"));
        Assert.Null(links[1].GetAttribute("aria-current"));

        var disabled = cut.Find("[aria-disabled='true']");
        Assert.Equal("-1", disabled.GetAttribute("tabindex"));
        Assert.Null(disabled.GetAttribute("href"));
        Assert.Contains("Unavailable", disabled.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Styling_declares_token_based_default_hover_active_focus_and_disabled_states()
    {
        var css = RepositoryFiles.Read(CssPath);

        RepositoryFiles.ContainsAll(
            css,
            ".srs-role-navigation__link:hover",
            ".srs-role-navigation__link:active",
            ".srs-role-navigation__link:focus-visible",
            ".srs-role-navigation__link--active",
            ".srs-role-navigation__link--disabled",
            "var(--srs-");
    }

    [Fact]
    public void Loading_and_error_states_are_named_without_changing_authorized_items()
    {
        using var context = new BunitContext();
        var items = new[] { new RoleNavigation.NavigationItem("Dashboard", "/student") };

        var loading = context.Render<RoleNavigation>(parameters => parameters
            .Add(component => component.AccessibleName, "Student navigation")
            .Add(component => component.Items, items)
            .Add(component => component.IsLoading, true)
            .Add(component => component.StatusText, "Refreshing navigation"));

        Assert.Equal("true", loading.Find("nav").GetAttribute("aria-busy"));
        Assert.Equal("status", loading.Find("[data-navigation-status]").GetAttribute("role"));
        Assert.Single(loading.FindAll("a"));

        var failed = context.Render<RoleNavigation>(parameters => parameters
            .Add(component => component.AccessibleName, "Student navigation")
            .Add(component => component.Items, items)
            .Add(component => component.HasError, true)
            .Add(component => component.StatusText, "Navigation unavailable"));

        Assert.Equal("alert", failed.Find("[data-navigation-status]").GetAttribute("role"));
        Assert.Contains("Navigation unavailable", failed.Markup, StringComparison.Ordinal);
        Assert.Single(failed.FindAll("a"));
    }
}
