using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint05ContractTests
{
    [Fact]
    public void Group_update_requires_parent_resource_versions_and_capacity_invariant()
    {
        var request = Spec010ContractAssertions.ContractType("UpdateGroupRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "ExpectedOfferingRowVersion", "ExpectedGroupRowVersion",
            "ExpectedRoomRowVersions", "ExpectedStaffTermAvailabilityRowVersions",
            "GroupCode", "Capacity", "RegistrationPaused", "State", "Meetings",
            "StaffAssignments", "Reason");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("MeetingMutation"),
            "RequestMeetingKey", "Id", "ActivityType", "DayOfWeek",
            "StartLocal", "EndLocal", "RoomId");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("StaffAssignmentMutation"),
            "RequestMeetingKey", "StaffId", "Role");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "PUT",
            "/api/admin/groups/{groupId}");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.GroupDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "complete group graph is validated and changed atomically",
            "CAPACITY_BELOW_ENROLLED",
            "GROUP_CHANGED",
            "requestMeetingKey",
            "meetingSlotId`/`meetingOrdinal` are not accepted mutation references",
            "cannot transition a group or offering into Published",
            "exclusive publisher",
            "append their privacy-safe audit in the same transaction",
            "No child, state, capacity, alert, or audit effect commits partially");
    }
}
