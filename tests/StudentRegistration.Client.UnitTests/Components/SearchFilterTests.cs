using Bunit;
using StudentRegistration.Client.Components.Forms;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class SearchFilterTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Forms/SearchFilter.razor.css";

    [Fact]
    public void Renders_a_labelled_search_landmark_with_native_submit_clear_and_filter_controls()
    {
        using var context = new BunitContext();

        var cut = context.Render<SearchFilter>(parameters => parameters
            .Add(component => component.AccessibleName, "Subject search")
            .Add(component => component.InputId, "subject-query")
            .Add(component => component.SearchLabel, "Search subjects")
            .Add(component => component.SubmitLabel, "Search")
            .Add(component => component.ClearLabel, "Clear")
            .Add(component => component.FilterContent, "<select aria-label=\"Availability\"><option>Available</option></select>"));

        var form = cut.Find("form");
        Assert.Equal("search", form.GetAttribute("role"));
        Assert.Equal("Subject search", form.GetAttribute("aria-label"));
        Assert.Equal("subject-query", cut.Find("label").GetAttribute("for"));
        Assert.Equal("subject-query", cut.Find("input").Id);
        Assert.Equal("submit", cut.Find("button[data-action='search']").GetAttribute("type"));
        Assert.Equal("button", cut.Find("button[data-action='clear']").GetAttribute("type"));
        Assert.NotNull(cut.Find("select"));
    }

    [Fact]
    public void Emits_query_search_and_clear_callbacks_once()
    {
        using var context = new BunitContext();
        var query = string.Empty;
        var searches = 0;
        var clears = 0;

        var cut = context.Render<SearchFilter>(parameters => parameters
            .Add(component => component.AccessibleName, "Subject search")
            .Add(component => component.InputId, "subject-query")
            .Add(component => component.SearchLabel, "Search subjects")
            .Add(component => component.SubmitLabel, "Search")
            .Add(component => component.ClearLabel, "Clear")
            .Add(component => component.QueryChanged, next => query = next)
            .Add(component => component.OnSearch, () => searches++)
            .Add(component => component.OnClear, () => clears++));

        cut.Find("input").Input("machine learning");
        cut.Find("form").Submit();
        cut.Find("button[data-action='clear']").Click();

        Assert.Equal(string.Empty, query);
        Assert.Equal(1, searches);
        Assert.Equal(1, clears);
    }

    [Fact]
    public void Loading_disabled_and_error_states_are_accessible_and_block_commands()
    {
        using var context = new BunitContext();
        var searches = 0;
        var clears = 0;

        var cut = context.Render<SearchFilter>(parameters => parameters
            .Add(component => component.AccessibleName, "Subject search")
            .Add(component => component.InputId, "subject-query")
            .Add(component => component.SearchLabel, "Search subjects")
            .Add(component => component.SubmitLabel, "Search")
            .Add(component => component.ClearLabel, "Clear")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingAnnouncement, "Loading results")
            .Add(component => component.ErrorMessage, "Search is unavailable")
            .Add(component => component.OnSearch, () => searches++)
            .Add(component => component.OnClear, () => clears++));

        var form = cut.Find("form");
        var input = cut.Find("input");
        Assert.Equal("true", form.GetAttribute("aria-busy"));
        Assert.True(input.HasAttribute("disabled"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("subject-query-error", input.GetAttribute("aria-describedby"));
        Assert.Contains("Loading results", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Search is unavailable", cut.Markup, StringComparison.Ordinal);
        form.Submit();
        cut.Find("button[data-action='clear']").Click();
        Assert.Equal(0, searches);
        Assert.Equal(0, clears);
    }

    [Fact]
    public void Default_hover_active_focus_disabled_loading_and_error_styles_use_tokens()
    {
        var css = RepositoryFiles.Read(CssPath);

        RepositoryFiles.ContainsAll(
            css,
            ".srs-search-filter button:hover:not(:disabled)",
            ".srs-search-filter button:active:not(:disabled)",
            ".srs-search-filter :focus-visible",
            ".srs-search-filter.is-disabled",
            ".srs-search-filter.is-loading",
            ".srs-search-filter.has-error",
            "var(--srs-");
    }
}
