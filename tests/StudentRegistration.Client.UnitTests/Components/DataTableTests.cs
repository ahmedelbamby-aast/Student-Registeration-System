using Bunit;
using Microsoft.AspNetCore.Components;
using StudentRegistration.Client.Components.Data;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class DataTableTests
{
    [Fact]
    public void Renders_native_caption_headers_and_body_rows()
    {
        using var context = new BunitContext();

        var table = RenderTable(context);

        Assert.Equal("Available groups", table.Find("caption").TextContent);
        Assert.Equal(2, table.FindAll("thead th[scope=col]").Count);
        Assert.Equal("G1", table.Find("tbody th[scope=row]").TextContent);
        Assert.Equal("Open", table.Find("tbody td").TextContent);
        Assert.Equal("available-groups", table.Find("table").GetAttribute("data-table-id"));
    }

    [Fact]
    public void Sortable_header_is_a_named_native_button_with_direction_and_callback()
    {
        using var context = new BunitContext();
        string? requestedKey = null;

        var table = RenderTable(context, sortRequested: key => requestedKey = key);
        var sortedHeader = table.Find("th[aria-sort=ascending]");
        var sortButton = sortedHeader.QuerySelector("button[type=button]");

        Assert.NotNull(sortButton);
        Assert.Equal("Group", sortButton.TextContent);
        sortButton.Click();
        Assert.Equal("group", requestedKey);
    }

    [Fact]
    public void Loading_empty_error_and_disabled_states_preserve_table_structure()
    {
        using var context = new BunitContext();

        var loading = RenderTable(context, isLoading: true, isDisabled: true);
        Assert.Equal("true", loading.Find("table").GetAttribute("aria-busy"));
        Assert.Equal("true", loading.Find("table").GetAttribute("aria-disabled"));
        Assert.True(loading.Find("thead button").HasAttribute("disabled"));
        Assert.Equal("Loading groups", loading.Find("tbody [role=status]").TextContent);

        var empty = RenderTable(context, isEmpty: true);
        Assert.Equal("No groups", empty.Find("tbody [data-table-state=empty]").TextContent);

        var error = RenderTable(context, errorMessage: "Groups unavailable");
        Assert.Equal("Groups unavailable", error.Find("tbody [role=alert]").TextContent);
        Assert.Equal("error", error.Find("table").GetAttribute("data-state"));
    }

    [Fact]
    public void Overflow_region_is_named_and_keyboard_reachable_with_an_equivalent_narrow_alternative()
    {
        using var context = new BunitContext();
        RenderFragment narrowAlternative = builder =>
            builder.AddMarkupContent(0, "<article><h3>Group G1</h3><p>Open</p></article>");

        var table = RenderTable(
            context,
            narrowAlternative: narrowAlternative,
            narrowAlternativeLabel: "Available groups stacked list");

        var viewport = table.Find(".srs-data-table__viewport");
        Assert.Equal("region", viewport.GetAttribute("role"));
        Assert.Equal("Available groups", viewport.GetAttribute("aria-label"));
        Assert.Equal("0", viewport.GetAttribute("tabindex"));

        var alternative = table.Find("[data-table-alternative]");
        Assert.Equal("region", alternative.GetAttribute("role"));
        Assert.Equal("Available groups stacked list", alternative.GetAttribute("aria-label"));
        Assert.Contains("Group G1", alternative.TextContent, StringComparison.Ordinal);
        Assert.Contains("Open", alternative.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Table_styles_cover_hover_active_focus_disabled_loading_and_token_only_states()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Data/DataTable.razor.css");
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

    private static Bunit.IRenderedComponent<DataTable> RenderTable(
        BunitContext context,
        Action<string>? sortRequested = null,
        bool isLoading = false,
        bool isEmpty = false,
        bool isDisabled = false,
        string? errorMessage = null,
        RenderFragment? narrowAlternative = null,
        string? narrowAlternativeLabel = null)
    {
        RenderFragment rows = builder =>
        {
            builder.OpenElement(0, "tr");
            builder.OpenElement(1, "th");
            builder.AddAttribute(2, "scope", "row");
            builder.AddContent(3, "G1");
            builder.CloseElement();
            builder.OpenElement(4, "td");
            builder.AddContent(5, "Open");
            builder.CloseElement();
            builder.CloseElement();
        };

        return context.Render<DataTable>(parameters => parameters
            .Add(component => component.TableId, "available-groups")
            .Add(component => component.Caption, "Available groups")
            .Add(component => component.Columns,
            [
                new DataTable.ColumnDefinition("group", "Group", sortable: true),
                new DataTable.ColumnDefinition("state", "State", sortable: false)
            ])
            .Add(component => component.SortedColumnKey, "group")
            .Add(component => component.Direction, DataTable.SortDirection.Ascending)
            .Add(component => component.BodyContent, rows)
            .Add(component => component.NarrowAlternative, narrowAlternative)
            .Add(component => component.NarrowAlternativeLabel, narrowAlternativeLabel)
            .Add(component => component.LoadingContent, "Loading groups")
            .Add(component => component.EmptyContent, "No groups")
            .Add(component => component.IsLoading, isLoading)
            .Add(component => component.IsEmpty, isEmpty)
            .Add(component => component.IsDisabled, isDisabled)
            .Add(component => component.ErrorMessage, errorMessage)
            .Add(component => component.SortRequested, sortRequested ?? (_ => { })));
    }
}
