namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class CatalogueAdministrationPageComponentTests
{
    [Fact]
    public void Adm_05_covers_all_states_editor_focus_dialog_and_duplicate_command_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "ADM-05", "T209", "CatalogueAdministrationPage",
            ["ADM-05-COMP-STATE-LOADING", "ADM-05-COMP-STATE-EMPTY", "ADM-05-COMP-STATE-SUCCESS",
             "ADM-05-COMP-STATE-VALIDATION-ERROR", "ADM-05-COMP-STATE-SERVICE-ERROR",
             "ADM-05-COMP-STATE-UNAUTHORIZED", "ADM-05-COMP-STATE-SESSION-EXPIRED",
             "ADM-05-COMP-STATE-STALE", "ADM-05-COMP-STATE-OFFLINE"],
            "RestorePublishFocusAsync", "ConfirmationDialog", "_isPublishing");
}
