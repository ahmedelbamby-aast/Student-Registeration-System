using Bunit;
using StudentRegistration.Client.Components.Forms;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class FormFieldTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Forms/FormField.razor.css";

    [Fact]
    public void Associates_visible_label_description_and_error_with_the_native_input()
    {
        using var context = new BunitContext();

        var cut = context.Render<FormField>(parameters => parameters
            .Add(component => component.InputId, "university-id")
            .Add(component => component.Name, "universityId")
            .Add(component => component.Label, "University ID")
            .Add(component => component.Description, "Use the issued identifier")
            .Add(component => component.ErrorMessage, "The identifier is required")
            .Add(component => component.Required, true));

        Assert.Equal("university-id", cut.Find("label").GetAttribute("for"));
        var input = cut.Find("input");
        Assert.Equal("university-id", input.Id);
        Assert.Equal("universityId", input.GetAttribute("name"));
        Assert.True(input.HasAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal(
            "university-id-description university-id-error",
            input.GetAttribute("aria-describedby"));
        Assert.Equal("Use the issued identifier", cut.Find("#university-id-description").TextContent);
        Assert.Equal("The identifier is required", cut.Find("#university-id-error").TextContent);
    }

    [Fact]
    public void Emits_parameter_driven_values_and_blocks_changes_while_unavailable()
    {
        using var context = new BunitContext();
        var value = string.Empty;

        var enabled = context.Render<FormField>(parameters => parameters
            .Add(component => component.InputId, "query")
            .Add(component => component.Label, "Query")
            .Add(component => component.ValueChanged, next => value = next));
        enabled.Find("input").Input("artificial intelligence");
        Assert.Equal("artificial intelligence", value);

        value = string.Empty;
        var loading = context.Render<FormField>(parameters => parameters
            .Add(component => component.InputId, "query-loading")
            .Add(component => component.Label, "Query")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingAnnouncement, "Loading field")
            .Add(component => component.ValueChanged, next => value = next));
        var loadingInput = loading.Find("input");
        Assert.True(loadingInput.HasAttribute("disabled"));
        Assert.Equal("true", loading.Find(".srs-form-field").GetAttribute("aria-busy"));
        Assert.Contains("Loading field", loading.Markup, StringComparison.Ordinal);
        loadingInput.Input("ignored");
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void Opt_in_secret_reveal_toggles_type_without_copying_the_value()
    {
        using var context = new BunitContext();

        var cut = context.Render<FormField>(parameters => parameters
            .Add(component => component.InputId, "password")
            .Add(component => component.Name, "password")
            .Add(component => component.Label, "Password")
            .Add(component => component.Type, "password")
            .Add(component => component.Value, "A long demo secret")
            .Add(component => component.AllowSecretReveal, true));

        var input = cut.Find("input");
        var reveal = cut.Find("[data-action='secret-reveal']");
        Assert.Equal("password", input.GetAttribute("type"));
        Assert.Equal("A long demo secret", input.GetAttribute("value"));
        Assert.Equal("false", reveal.GetAttribute("aria-pressed"));
        Assert.Equal("Show Password", reveal.TextContent.Trim());

        reveal.Click();

        Assert.Equal("text", cut.Find("input").GetAttribute("type"));
        Assert.Equal("A long demo secret", cut.Find("input").GetAttribute("value"));
        Assert.Equal("true", cut.Find("[data-action='secret-reveal']").GetAttribute("aria-pressed"));
        Assert.Equal("Hide Password", cut.Find("[data-action='secret-reveal']").TextContent.Trim());
    }

    [Fact]
    public void Interaction_and_state_styles_use_only_approved_tokens()
    {
        var css = RepositoryFiles.Read(CssPath);

        RepositoryFiles.ContainsAll(
            css,
            ".srs-form-field:hover",
            ".srs-form-field:focus-within",
            ".srs-form-field.is-disabled",
            ".srs-form-field.is-loading",
            ".srs-form-field.has-error",
            ".srs-form-field__reveal",
            "min-block-size: var(--srs-sizing-interactive-minimum)",
            "var(--srs-");
    }
}
