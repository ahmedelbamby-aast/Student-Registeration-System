using Bunit;
using StudentRegistration.Client.Components.Data;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class PaginationTests
{
    [Fact]
    public void Renders_labelled_navigation_and_announces_the_current_page()
    {
        using var context = new BunitContext();

        var pagination = RenderPagination(context);
        var nav = pagination.Find("nav");
        Assert.Equal("Group pages", nav.GetAttribute("aria-label"));
        Assert.Equal("page", pagination.Find("button[aria-current=page]").GetAttribute("aria-current"));
        Assert.Equal("Page 2 of 4", pagination.Find("button[aria-current=page]").GetAttribute("aria-label"));
        Assert.Equal("2", pagination.Find("button[aria-current=page]").TextContent);
    }

    [Fact]
    public void Native_previous_page_and_next_buttons_emit_bounded_page_requests()
    {
        using var context = new BunitContext();
        var requests = new List<int>();

        var pagination = RenderPagination(context, pageChanged: page => requests.Add(page));
        pagination.Find("button[data-page-action=previous]").Click();
        pagination.Find("button[data-page=3]").Click();
        pagination.Find("button[data-page-action=next]").Click();

        Assert.Equal([1, 3, 3], requests);
        Assert.All(pagination.FindAll("button"), button => Assert.Equal("button", button.GetAttribute("type")));
    }

    [Fact]
    public void Disabled_loading_and_error_states_block_actions_and_keep_textual_feedback()
    {
        using var context = new BunitContext();
        var requests = 0;

        var pagination = RenderPagination(
            context,
            isDisabled: true,
            isLoading: true,
            errorMessage: "Pages unavailable",
            pageChanged: _ => requests++);

        Assert.Equal("true", pagination.Find("nav").GetAttribute("aria-busy"));
        Assert.Equal("true", pagination.Find("nav").GetAttribute("aria-disabled"));
        Assert.All(pagination.FindAll("button"), button => Assert.True(button.HasAttribute("disabled")));
        pagination.Find("button[data-page-action=next]").Click();
        Assert.Equal(0, requests);
        Assert.Equal("Loading pages", pagination.Find("[role=status]").TextContent);
        Assert.Equal("Pages unavailable", pagination.Find("[role=alert]").TextContent);
    }

    [Fact]
    public void Pagination_styles_cover_hover_active_focus_disabled_loading_and_token_only_states()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Data/Pagination.razor.css");
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

    private static Bunit.IRenderedComponent<Pagination> RenderPagination(
        BunitContext context,
        bool isDisabled = false,
        bool isLoading = false,
        string? errorMessage = null,
        Action<int>? pageChanged = null) =>
        context.Render<Pagination>(parameters => parameters
            .Add(component => component.AccessibleLabel, "Group pages")
            .Add(component => component.PreviousLabel, "Previous")
            .Add(component => component.NextLabel, "Next")
            .Add(component => component.LoadingLabel, "Loading pages")
            .Add(component => component.CurrentPage, 2)
            .Add(component => component.TotalPages, 4)
            .Add(component => component.VisiblePages, [1, 2, 3, 4])
            .Add(component => component.PageLabel, page => page.ToString())
            .Add(component => component.CurrentPageLabel, (page, total) => $"Page {page} of {total}")
            .Add(component => component.IsDisabled, isDisabled)
            .Add(component => component.IsLoading, isLoading)
            .Add(component => component.ErrorMessage, errorMessage)
            .Add(component => component.PageChanged, pageChanged ?? (_ => { })));
}
