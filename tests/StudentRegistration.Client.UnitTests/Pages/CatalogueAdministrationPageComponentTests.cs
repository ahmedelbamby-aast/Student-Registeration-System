using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class CatalogueAdministrationPageComponentTests
{
    [Fact]
    public void Adm_05_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        AdminPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("CatalogueAdministrationPage", "ADM-05");

    [Fact]
    public void Adm_05_covers_all_states_editor_focus_dialog_and_duplicate_command_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "ADM-05", "T209", "CatalogueAdministrationPage",
            ["ADM-05-COMP-STATE-LOADING", "ADM-05-COMP-STATE-EMPTY", "ADM-05-COMP-STATE-SUCCESS",
             "ADM-05-COMP-STATE-VALIDATION-ERROR", "ADM-05-COMP-STATE-SERVICE-ERROR",
             "ADM-05-COMP-STATE-UNAUTHORIZED", "ADM-05-COMP-STATE-SESSION-EXPIRED",
             "ADM-05-COMP-STATE-STALE", "ADM-05-COMP-STATE-OFFLINE"],
            "RestorePublishFocusAsync", "ConfirmationDialog", "_isPublishing");

    [Fact]
    public void Adm_05_version_table_and_compact_list_expose_equivalent_fields()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor");
        RepositoryFiles.ContainsAll(source, "Catalogue versions", "Catalogue versions, compact view",
            "data-testid=\"catalogue-version-list\"", "@version.Version", "@version.Scope",
            "@version.State", "@version.Source", "@version.PublishedAtUtc");
        Assert.True(Occurrences(source, "@version.Scope") >= 2);
        Assert.True(Occurrences(source, "@version.PublishedAtUtc") >= 2);
    }

    private static int Occurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
