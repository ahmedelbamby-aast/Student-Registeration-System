using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Shared_group_status_contract_announces_changes_without_moving_focus_or_selection()
    {
        var groupCard = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/GroupCard.razor");
        var capacityIndicator = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Registration/CapacityIndicator.razor");
        var catalogue = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/components/catalogue.md");

        RepositoryFiles.ContainsAll(
            groupCard,
            "aria-pressed=\"@IsSelected.ToString().ToLowerInvariant()\"",
            "@onclick=\"SelectAsync\"",
            "OnSelected.InvokeAsync(GroupId)",
            "role=\"status\"");
        RepositoryFiles.ContainsAll(
            capacityIndicator,
            "role=\"status\"",
            "aria-live=\"polite\"",
            "aria-atomic=\"true\"");
        RepositoryFiles.ContainsAll(
            catalogue,
            "update announced politely",
            "Focus remains visible and is not moved for background refresh");

        Assert.DoesNotContain("autofocus", groupCard, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FocusAsync", groupCard, StringComparison.Ordinal);
        Assert.DoesNotContain("IsSelected =", groupCard, StringComparison.Ordinal);
    }
}
