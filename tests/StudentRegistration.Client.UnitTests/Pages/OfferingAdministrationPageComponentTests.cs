namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class OfferingAdministrationPageComponentTests
{
    [Fact]
    public void Adm_06_covers_state_editor_validation_dialog_and_duplicate_command_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "ADM-06", "T214", "OfferingAdministrationPage",
            ["LoadingState", "EmptyState", "SuccessState", "ValidationErrorState", "ServiceErrorState",
             "UnauthorizedState", "SessionExpiredState", "StaleState", "OfflineState"],
            "offering-validation-summary", "aria-modal=\"true\"", "disabled=\"@_pending\"");
}
