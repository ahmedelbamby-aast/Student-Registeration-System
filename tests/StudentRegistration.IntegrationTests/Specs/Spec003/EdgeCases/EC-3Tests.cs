using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Shared_reflow_contract_keeps_critical_actions_and_blocking_text_available()
    {
        var responsiveContract = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/responsive-layout-contract.md");
        var appShell = RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/index.html");
        var statePanelStyles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Feedback/RouteStatePanel.razor.css");
        var groupCardStyles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/GroupCard.razor.css");

        RepositoryFiles.ContainsAll(
            responsiveContract,
            "At 400% zoom, content reflows to an experience equivalent to the 320 CSS px",
            "Blocking reason text, validation messages, and recovery actions do",
            "not truncate or require two-dimensional page scrolling",
            "primary action last in reading order and reachable without horizontal scroll");
        RepositoryFiles.ContainsAll(
            appShell,
            "name=\"viewport\"",
            "width=device-width, initial-scale=1.0");
        RepositoryFiles.ContainsAll(
            statePanelStyles,
            "flex-wrap: wrap",
            "min-block-size: var(--srs-sizing-interactive-minimum)");
        Assert.Contains("width: 100%", groupCardStyles, StringComparison.Ordinal);

        var criticalStyles = statePanelStyles + groupCardStyles;
        Assert.DoesNotContain("text-overflow:", criticalStyles, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("white-space: nowrap", criticalStyles, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("line-clamp", criticalStyles, StringComparison.OrdinalIgnoreCase);
    }
}
