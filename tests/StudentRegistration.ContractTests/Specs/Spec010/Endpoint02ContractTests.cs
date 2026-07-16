using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Group_detail_exposes_capacity_state_version_and_complete_activity_details()
    {
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("GroupDto"),
            "Id", "OfferingId", "GroupCode", "Capacity", "EnrolledCount",
            "RegistrationPaused", "State", "Selectable", "NonSelectableReasons",
            "Staff", "Meetings", "RowVersion");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("MeetingDto"),
            "Id", "ActivityType", "DayOfWeek", "StartLocal", "EndLocal",
            "RoomId", "RoomCode", "Location");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("GroupStaffDto"),
            "MeetingSlotId", "ActivityType", "StaffId", "Role", "Name");

        var endpoint = Spec010ContractAssertions.Endpoint("GET", "/api/groups/{groupId}");
        Spec010ContractAssertions.RequiresOfferingDetailsRead(endpoint);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.GroupDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, false);
        Spec010ContractAssertions.ContractContains(
            "`selectable` is server-authored",
            "`Tutorial` is the stored and serialized activity value",
            "GROUP_FULL",
            "`GROUP_CHANGED` is a write/registration concurrency result",
            "never emitted by an ordinary offering or group GET",
            "They do not evaluate SPEC-011");
    }
}
