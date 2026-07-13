using Bunit;
using StudentRegistration.Client.Components.Forms;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Components;

public sealed class ValidationSummaryTests
{
    private const string CssPath =
        "src/StudentRegistration.Client/Components/Forms/AccessibleValidationSummary.razor.css";

    [Fact]
    public void Renders_a_focusable_named_error_summary_linked_to_invalid_controls()
    {
        using var context = new BunitContext();
        var errors = new[]
        {
            new AccessibleValidationSummary.ValidationItem("university-id", "University ID is required"),
            new AccessibleValidationSummary.ValidationItem("password", "Password is required")
        };

        var cut = context.Render<AccessibleValidationSummary>(parameters => parameters
            .Add(component => component.SummaryId, "login-errors")
            .Add(component => component.Heading, "Check the form")
            .Add(component => component.Errors, errors));

        var summary = cut.Find("section");
        Assert.Equal("alert", summary.GetAttribute("role"));
        Assert.Equal("-1", summary.GetAttribute("tabindex"));
        Assert.Equal("login-errors-heading", summary.GetAttribute("aria-labelledby"));
        Assert.Equal(2, cut.FindAll("li").Count);
        Assert.Equal("#university-id", cut.FindAll("a")[0].GetAttribute("href"));
        Assert.Equal("#password", cut.FindAll("a")[1].GetAttribute("href"));
        Assert.Contains("University ID is required", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Empty_and_loading_states_are_explicit_and_parameter_driven()
    {
        using var context = new BunitContext();

        var empty = context.Render<AccessibleValidationSummary>(parameters => parameters
            .Add(component => component.Heading, "Check the form"));
        Assert.Empty(empty.FindAll("section"));

        var loading = context.Render<AccessibleValidationSummary>(parameters => parameters
            .Add(component => component.Heading, "Check the form")
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingAnnouncement, "Validating form"));
        var status = loading.Find("section");
        Assert.Equal("status", status.GetAttribute("role"));
        Assert.Equal("true", status.GetAttribute("aria-busy"));
        Assert.Contains("Validating form", status.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Error_links_and_focus_styles_use_only_approved_tokens()
    {
        var css = RepositoryFiles.Read(CssPath);

        RepositoryFiles.ContainsAll(
            css,
            ".srs-validation-summary a:hover",
            ".srs-validation-summary a:active",
            ".srs-validation-summary:focus-visible",
            ".srs-validation-summary.is-loading",
            ".srs-validation-summary.has-error",
            "var(--srs-");
    }
}
