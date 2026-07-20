using StudentRegistration.TestSupport;

namespace StudentRegistration.VisualTests.Routes;

public sealed class RegistrationRecordsPageVisualContractTests
{
    [Theory]
    [InlineData("STU-06", "RegistrationResultPage")]
    [InlineData("STU-07", "RegistrationHistoryPage")]
    public void Route_has_frozen_responsive_widths_and_scoped_reflow_styles(string routeId, string pageName)
    {
        var design = RepositoryFiles.Read($"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");
        var styles = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{pageName}.razor.css");
        RepositoryFiles.ContainsAll(design, "320,", "375,", "768,", "1024,", "1280,", "1920");
        RepositoryFiles.ContainsAll(styles, "@media (max-width: 47.99rem)", "min-inline-size: 0", "var(--srs-spacing-");
    }
}
