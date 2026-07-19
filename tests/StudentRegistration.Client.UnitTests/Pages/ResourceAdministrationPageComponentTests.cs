namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class ResourceAdministrationPageComponentTests
{
    [Fact]
    public void Adm_07_covers_state_forms_read_only_availability_and_pending_command_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "ADM-07", "T219", "ResourceAdministrationPage",
            ["LoadingState", "EmptyState", "SuccessState", "ValidationErrorState", "ServiceErrorState",
             "UnauthorizedState", "SessionExpiredState", "StaleState", "OfflineState"],
            "read-only-staff-availability", "Revalidate impact", "disabled=\"@_pending\"");
}
