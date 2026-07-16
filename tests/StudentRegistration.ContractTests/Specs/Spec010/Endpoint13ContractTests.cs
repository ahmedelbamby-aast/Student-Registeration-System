using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint13ContractTests
{
    [Fact]
    public void Alert_revalidation_requires_all_versions_and_durably_records_result()
    {
        var request = Spec010ContractAssertions.ContractType(
            "RevalidateScheduleImpactAlertRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "ExpectedAlertRowVersion", "ExpectedGroupRowVersion",
            "ExpectedRoomRowVersions", "ExpectedStaffTermAvailabilityRowVersions");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("ScheduleImpactValidationSnapshotDto"),
            "Valid", "Reasons", "GroupRowVersion", "RoomRowVersions",
            "StaffTermAvailabilityRowVersions", "ValidatedAtUtc");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "POST",
            "/api/admin/schedule-impact-alerts/{alertId}/revalidate");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.ScheduleImpactAlertDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "locks the alert and submitted group/room/staff-term versions",
            "token-free validation snapshot containing those dependency versions",
            "Revalidation advances the alert rowversion",
            "never silently resolves it",
            "Changed dependencies produce no alert-state update");
    }
}
