using StudentRegistration.TestSupport;

namespace StudentRegistration.VisualTests.Routes;

public sealed class RegistrationAdministrationPageVisualTests
{
    [Fact]
    public void Adm_08_visual_layout_is_bounded_stacked_and_never_adds_a_command_toolbar()
    {
        var css = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor.css");

        RepositoryFiles.ContainsAll(
            css,
            ".srs-registration-monitor",
            "max-inline-size:",
            "grid-template-columns:",
            "min-block-size: 44px",
            "@media (max-width: 48rem)",
            "overflow-wrap: anywhere");
        Assert.DoesNotContain("command-toolbar", css, StringComparison.OrdinalIgnoreCase);
    }
}
