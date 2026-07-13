using Bunit;
using StudentRegistration.Client.Components.Overlay;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ConfirmationDialogTests
{
    [Fact]
    public void Closed_dialog_renders_nothing_and_open_dialog_is_named_and_modal()
    {
        using var context = new BunitContext();

        var closed = RenderDialog(context, isOpen: false);
        Assert.Empty(closed.Markup);

        var open = RenderDialog(context, isOpen: true);
        var dialog = open.Find("dialog");
        Assert.True(dialog.HasAttribute("open"));
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("dialog-title", dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("dialog-description", dialog.GetAttribute("aria-describedby"));
        Assert.Equal("Confirm registration", open.Find("#dialog-title").TextContent);
        Assert.Equal("Review the selected groups.", open.Find("#dialog-description").TextContent);
    }

    [Fact]
    public void Native_buttons_invoke_confirm_cancel_escape_and_focus_restoration_contracts()
    {
        using var context = new BunitContext();
        var confirmed = 0;
        var cancelled = 0;
        var restoreRequests = 0;

        var dialog = RenderDialog(
            context,
            isOpen: true,
            onConfirm: () => confirmed++,
            onCancel: () => cancelled++,
            onRestore: () => restoreRequests++);

        dialog.Find("button[data-action=confirm]").Click();
        dialog.Find("button[data-action=cancel]").Click();
        dialog.Find("dialog").KeyDown("Escape");

        Assert.Equal(1, confirmed);
        Assert.Equal(2, cancelled);
        Assert.Equal(3, restoreRequests);
        Assert.True(dialog.Find("button[data-action=cancel]").HasAttribute("autofocus"));
        Assert.Equal(2, dialog.FindAll("[data-focus-boundary]").Count);
    }

    [Fact]
    public void Busy_disabled_and_error_states_block_duplicate_confirmation_and_keep_error_text()
    {
        using var context = new BunitContext();
        var confirmed = 0;

        var dialog = RenderDialog(
            context,
            isOpen: true,
            isBusy: true,
            isConfirmDisabled: true,
            errorMessage: "Registration could not be submitted",
            onConfirm: () => confirmed++);

        Assert.Equal("true", dialog.Find("dialog").GetAttribute("aria-busy"));
        Assert.True(dialog.Find("button[data-action=confirm]").HasAttribute("disabled"));
        dialog.Find("button[data-action=confirm]").Click();
        Assert.Equal(0, confirmed);
        Assert.Equal("alert", dialog.Find("[role=alert]").GetAttribute("role"));
        Assert.Equal("Registration could not be submitted", dialog.Find("[role=alert]").TextContent);
        Assert.Equal("Submitting registration", dialog.Find("[role=status]").TextContent);
    }

    [Fact]
    public void Dialog_styles_cover_hover_active_focus_disabled_loading_and_token_only_states()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Overlay/ConfirmationDialog.razor.css");
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

    private static Bunit.IRenderedComponent<ConfirmationDialog> RenderDialog(
        BunitContext context,
        bool isOpen,
        bool isBusy = false,
        bool isConfirmDisabled = false,
        string? errorMessage = null,
        Action? onConfirm = null,
        Action? onCancel = null,
        Action? onRestore = null) =>
        context.Render<ConfirmationDialog>(parameters => parameters
            .Add(component => component.IsOpen, isOpen)
            .Add(component => component.DialogId, "registration-dialog")
            .Add(component => component.TitleId, "dialog-title")
            .Add(component => component.DescriptionId, "dialog-description")
            .Add(component => component.Title, "Confirm registration")
            .Add(component => component.Message, "Review the selected groups.")
            .Add(component => component.ConfirmLabel, "Confirm")
            .Add(component => component.CancelLabel, "Cancel")
            .Add(component => component.BusyLabel, "Submitting registration")
            .Add(component => component.IsBusy, isBusy)
            .Add(component => component.IsConfirmDisabled, isConfirmDisabled)
            .Add(component => component.ErrorMessage, errorMessage)
            .Add(component => component.OnConfirm, onConfirm ?? (() => { }))
            .Add(component => component.OnCancel, onCancel ?? (() => { }))
            .Add(component => component.FocusRestoreRequested, onRestore ?? (() => { })));
}
