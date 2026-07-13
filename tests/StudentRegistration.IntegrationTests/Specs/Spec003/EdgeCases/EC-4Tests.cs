using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Reduced_motion_projection_removes_nonessential_duration_but_keeps_semantic_feedback()
    {
        var tokenProjection = RepositoryFiles.Read(
            "src/StudentRegistration.Client/wwwroot/css/design-tokens.css");
        var tokenContract = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/tokens/contract.md");
        var groupCardStyles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/GroupCard.razor.css");
        var capacityIndicator = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/CapacityIndicator.razor");

        RepositoryFiles.ContainsAll(
            tokenProjection,
            "@media (prefers-reduced-motion: reduce)",
            "--srs-motion-duration-feedback: 0ms",
            "--srs-motion-duration-overlay: 0ms");
        RepositoryFiles.ContainsAll(
            tokenContract,
            "reduced-motion equivalents",
            "no required information carried only by animation");
        Assert.Contains(
            "transition: border-color var(--srs-motion-duration-feedback)",
            groupCardStyles,
            StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(
            capacityIndicator,
            "role=\"status\"",
            "aria-live=\"polite\"",
            "@Occupied",
            "@Total",
            "@Remaining");
    }
}
