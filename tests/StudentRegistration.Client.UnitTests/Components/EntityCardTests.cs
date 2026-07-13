using Bunit;
using StudentRegistration.Client.Components.Display;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class EntityCardTests
{
    [Fact]
    public void Renders_a_heading_named_article_with_native_interactive_descendants()
    {
        using var context = new BunitContext();

        var card = context.Render<EntityCard>(parameters => parameters
            .Add(component => component.HeadingId, "course-heading")
            .Add(component => component.Heading, "Machine Learning")
            .Add(component => component.ChildContent, "<button type=\"button\">Open groups</button>"));

        var article = card.Find("article");
        Assert.Equal("course-heading", article.GetAttribute("aria-labelledby"));
        Assert.Equal("Machine Learning", card.Find("#course-heading").TextContent);
        Assert.Equal("button", card.Find("button").TagName.ToLowerInvariant());
        Assert.Null(article.GetAttribute("tabindex"));
        Assert.Null(article.GetAttribute("role"));
    }

    [Fact]
    public void Exposes_disabled_loading_and_error_states_without_hiding_the_heading()
    {
        using var context = new BunitContext();

        var card = context.Render<EntityCard>(parameters => parameters
            .Add(component => component.HeadingId, "offering-heading")
            .Add(component => component.Heading, "Data Structures")
            .Add(component => component.IsDisabled, true)
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingContent, "Refreshing groups")
            .Add(component => component.ErrorMessage, "Groups could not be refreshed")
            .Add(component => component.ChildContent, "Existing details"));

        var article = card.Find("article");
        Assert.Equal("true", article.GetAttribute("aria-disabled"));
        Assert.Equal("true", article.GetAttribute("aria-busy"));
        Assert.Equal("loading-error-disabled", article.GetAttribute("data-state"));
        Assert.Equal("status", card.Find("[role=status]").GetAttribute("role"));
        Assert.Equal("alert", card.Find("[role=alert]").GetAttribute("role"));
        Assert.Equal("Data Structures", card.Find("#offering-heading").TextContent);
    }

    [Fact]
    public void Isolated_styles_cover_hover_active_focus_and_token_only_state_styling()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Display/EntityCard.razor.css");

        RepositoryFiles.ContainsAll(
            css,
            ":hover",
            ":active",
            ":focus-within",
            "[aria-disabled=\"true\"]",
            "[aria-busy=\"true\"]",
            "var(--srs-");
        Assert.DoesNotContain("#", css, StringComparison.Ordinal);
        Assert.DoesNotContain("rgb(", css, StringComparison.OrdinalIgnoreCase);
    }
}
