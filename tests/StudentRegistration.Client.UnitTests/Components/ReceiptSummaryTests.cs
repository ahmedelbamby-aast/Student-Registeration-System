using Bunit;
using StudentRegistration.Client.Components.Registration;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ReceiptSummaryTests
{
    [Fact]
    public void Receipt_has_a_named_region_complete_items_reference_and_native_print_action()
    {
        using var context = new BunitContext();
        var printRequests = 0;

        var cut = Render(context, onPrint: () => printRequests++);
        var region = cut.Find("section[role=region]");

        Assert.Equal("registration-receipt-heading", region.GetAttribute("aria-labelledby"));
        Assert.Equal("default", region.GetAttribute("data-state"));
        Assert.Equal("Registration receipt", cut.Find("h2").TextContent.Trim());

        var items = cut.FindAll("[data-receipt-item]");
        Assert.Equal(3, items.Count);
        Assert.Contains("Term", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("Spring 2027", items[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("AI301", items[1].TextContent, StringComparison.Ordinal);
        Assert.Contains("18", items[2].TextContent, StringComparison.Ordinal);

        var reference = cut.Find("[data-receipt-reference]").TextContent;
        Assert.Contains("Registration reference", reference, StringComparison.Ordinal);
        Assert.Contains("REG-2027-00042", reference, StringComparison.Ordinal);

        var printButton = cut.Find("button[data-print-receipt]");
        Assert.Equal("Print receipt", printButton.TextContent.Trim());
        printButton.Click();
        Assert.Equal(1, printRequests);
    }

    [Fact]
    public void Disabled_loading_and_error_states_block_printing_and_preserve_truthful_feedback()
    {
        using var context = new BunitContext();
        var printRequests = 0;

        var disabled = Render(
            context,
            isDisabled: true,
            disabledReason: "Printing is unavailable",
            onPrint: () => printRequests++);
        Assert.Equal("disabled", disabled.Find("section").GetAttribute("data-state"));
        Assert.True(disabled.Find("button[data-print-receipt]").HasAttribute("disabled"));
        disabled.Find("button[data-print-receipt]").Click();
        Assert.Equal(0, printRequests);
        Assert.Equal(
            "Printing is unavailable",
            disabled.Find("[data-disabled-reason]").TextContent.Trim());

        var loading = Render(context, isLoading: true, loadingText: "Loading receipt");
        Assert.Equal("loading", loading.Find("section").GetAttribute("data-state"));
        Assert.Equal("true", loading.Find("section").GetAttribute("aria-busy"));
        Assert.Equal("Loading receipt", loading.Find("[role=status]").TextContent.Trim());
        Assert.Empty(loading.FindAll("[data-receipt-item]"));
        Assert.Empty(loading.FindAll("button"));

        var error = Render(context, errorMessage: "Receipt could not be loaded");
        Assert.Equal("error", error.Find("section").GetAttribute("data-state"));
        Assert.Equal("Receipt could not be loaded", error.Find("[role=alert]").TextContent.Trim());
        Assert.Empty(error.FindAll("[data-receipt-reference]"));
        Assert.Empty(error.FindAll("button"));
    }

    [Fact]
    public void Receipt_styles_cover_print_hover_active_focus_disabled_and_token_only_states()
    {
        var styles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/ReceiptSummary.razor.css");

        RepositoryFiles.ContainsAll(
            styles,
            "@media print",
            ":hover",
            ":active",
            ":focus-visible",
            ":disabled",
            "var(--srs-",
            "min-height: var(--srs-sizing-interactive-minimum)");
        Assert.DoesNotContain("#", styles, StringComparison.Ordinal);
    }

    private static IRenderedComponent<ReceiptSummary> Render(
        BunitContext context,
        bool isDisabled = false,
        string? disabledReason = null,
        bool isLoading = false,
        string? loadingText = null,
        string? errorMessage = null,
        Action? onPrint = null) =>
        context.Render<ReceiptSummary>(parameters => parameters
            .Add(component => component.ComponentId, "registration-receipt")
            .Add(component => component.Heading, "Registration receipt")
            .Add(component => component.ReferenceLabel, "Registration reference")
            .Add(component => component.Reference, "REG-2027-00042")
            .Add(component => component.PrintLabel, "Print receipt")
            .Add(
                component => component.Items,
                [
                    new ReceiptSummary.ReceiptItem("Term", "Spring 2027"),
                    new ReceiptSummary.ReceiptItem("Registered subjects", "AI301, AI302"),
                    new ReceiptSummary.ReceiptItem("Registered credits", "18")
                ])
            .Add(component => component.IsDisabled, isDisabled)
            .Add(component => component.DisabledReason, disabledReason)
            .Add(component => component.IsLoading, isLoading)
            .Add(component => component.LoadingText, loadingText)
            .Add(component => component.ErrorMessage, errorMessage)
            .Add(component => component.OnPrint, onPrint ?? (() => { })));
}
