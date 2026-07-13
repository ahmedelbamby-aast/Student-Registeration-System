using Bunit;
using StudentRegistration.Client.Components.Forms;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ButtonTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Forms/AppButton.razor.css";

    [Fact]
    public void Uses_a_native_named_button_and_invokes_the_supplied_callback_once()
    {
        using var context = new BunitContext();
        var invocations = 0;

        var cut = context.Render<AppButton>(parameters => parameters
            .Add(component => component.Type, "submit")
            .Add(component => component.AccessibleLabel, "Submit registration")
            .Add(component => component.ChildContent, "Submit")
            .Add(component => component.OnClick, () => invocations++));

        var button = cut.Find("button");
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Equal("Submit registration", button.GetAttribute("aria-label"));
        Assert.Contains("Submit", button.TextContent, StringComparison.Ordinal);

        button.Click();
        Assert.Equal(1, invocations);
    }

    [Fact]
    public void Disabled_and_loading_states_prevent_duplicate_callbacks()
    {
        using var context = new BunitContext();
        var invocations = 0;

        var disabled = context.Render<AppButton>(parameters => parameters
            .Add(component => component.Disabled, true)
            .Add(component => component.ChildContent, "Disabled action")
            .Add(component => component.OnClick, () => invocations++));
        var disabledButton = disabled.Find("button");
        Assert.True(disabledButton.HasAttribute("disabled"));
        disabledButton.Click();

        var loading = context.Render<AppButton>(parameters => parameters
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingAnnouncement, "Submitting")
            .Add(component => component.ChildContent, "Submit")
            .Add(component => component.OnClick, () => invocations++));
        var loadingButton = loading.Find("button");
        Assert.True(loadingButton.HasAttribute("disabled"));
        Assert.Equal("true", loadingButton.GetAttribute("aria-busy"));
        Assert.Contains("Submitting", loading.Markup, StringComparison.Ordinal);
        loadingButton.Click();

        Assert.Equal(0, invocations);
    }

    [Fact]
    public void Error_description_and_all_interactive_styles_are_token_based()
    {
        using var context = new BunitContext();
        var cut = context.Render<AppButton>(parameters => parameters
            .Add(component => component.HasError, true)
            .Add(component => component.DescribedBy, "submit-error")
            .Add(component => component.ChildContent, "Submit"));

        var button = cut.Find("button");
        Assert.Equal("submit-error", button.GetAttribute("aria-describedby"));
        Assert.Contains("has-error", button.GetAttribute("class"), StringComparison.Ordinal);

        var css = RepositoryFiles.Read(CssPath);
        RepositoryFiles.ContainsAll(
            css,
            ".srs-button:hover:not(:disabled)",
            ".srs-button:active:not(:disabled)",
            ".srs-button:focus-visible",
            ".srs-button:disabled",
            ".srs-button.is-loading",
            ".srs-button.has-error",
            "var(--srs-");
    }
}
