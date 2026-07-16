using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint12ContractTests
{
    [Fact]
    public void Schedule_impact_alert_list_is_bounded_filtered_authorized_and_versioned()
    {
        var alert = Spec010ContractAssertions.ContractType("ScheduleImpactAlertDto");
        Spec010ContractAssertions.HasExactProperties(
            alert,
            "Id", "GroupId", "StaffTermAvailabilityId", "RoomId", "ReasonCode",
            "State", "DetectedGroupRowVersion", "DetectedRoomRowVersion",
            "DetectedStaffTermAvailabilityRowVersion", "LastValidation",
            "DetectedAtUtc", "RevalidatedAtUtc", "ResolvedAtUtc", "RowVersion");
        Spec010ContractAssertions.HasPropertyType(
            alert,
            "LastValidation",
            "StudentRegistration.Contracts.Scheduling.ScheduleImpactValidationSnapshotDto");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("ScheduleImpactValidationSnapshotDto"),
            "Valid", "Reasons", "GroupRowVersion", "RoomRowVersions",
            "StaffTermAvailabilityRowVersions", "ValidatedAtUtc");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "GET",
            "/api/admin/schedule-impact-alerts");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "ScheduleImpactAlertDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, false, false);
        Spec010ContractAssertions.ContractContains(
            "Optional `state`, `groupId`, `reasonCode`, `page`, `pageSize`, `sort`",
            "Default sort `detectedAtUtc-desc,id`",
            "token-free validation snapshot",
            "`400 PAGE_SIZE_INVALID`");
    }
}
