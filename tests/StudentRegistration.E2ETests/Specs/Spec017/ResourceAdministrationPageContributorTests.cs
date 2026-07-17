using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class ResourceAdministrationPageContributorTests
{
    [Fact]
    public void Adm_07_contribution_freezes_read_only_availability_and_versioned_resources()
    {
        var (contract, page) = ContributorContractAssertions.Load(
            "ADM-07",
            "SPEC-010",
            "ResourceAdministrationPage.razor",
            "/admin/resources",
            "spec017-adm07/1.0");

        RepositoryFiles.ContainsAll(
            contract,
            "GET /api/admin/staff-availability",
            "Offerings.Manage",
            "Room update requires current rowversion and reason",
            "REVALIDATION_REQUIRED",
            "REVALIDATION_FAILED",
            "ALERT_NOT_OPEN",
            "IDEMPOTENCY_KEY_REUSED",
            "read-only complete range projection",
            "does not mutate `StaffTermAvailability`",
            "does not receive `Availability.ManageOwn`");
        RepositoryFiles.ContainsAll(
            page,
            "SchedulingApi.ListRoomsAsync",
            "SchedulingApi.ListAvailabilityAsync",
            "SchedulingApi.ListAlertsAsync",
            "SchedulingApi.UpdateRoomAsync",
            "SchedulingApi.RevalidateAlertAsync",
            "SchedulingApi.ResolveAlertAsync",
            "cannot edit or override any range",
            "Read-only Staff availability selected");
    }

    [Fact]
    public void Adm_07_has_no_admin_availability_correction_surface()
    {
        var contract = ContributorContractAssertions.Contract("ADM-07");
        RepositoryFiles.ContainsAll(
            contract,
            "no Admin availability mutation/correction/override endpoint",
            "editable range control",
            "notification workflow",
            "correction audit",
            "room/conflict override",
            "generic Admin facade");
    }
}
