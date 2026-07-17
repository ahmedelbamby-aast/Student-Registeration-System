using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec016;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public void Every_direct_staff_object_route_has_server_scope_evidence()
    {
        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Endpoints/Spec016Endpoints.cs");
        var queries = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        var reader = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Staff/SqlStaffWorkspaceAdapter.cs");
        RepositoryFiles.ContainsAll(
            endpoints,
            "Context.Read",
            "GetRosterAsync",
            "GetAssignmentsAsync",
            "GetTimetableAsync");
        RepositoryFiles.ContainsAll(
            queries,
            "Context(principal)",
            "ReadRosterIfAssignedAsync",
            "STAFF_GROUP_NOT_FOUND");
        RepositoryFiles.ContainsAll(
            reader,
            "ApplicationUserId",
            "assignment.GroupId == groupId",
            "staff.IsActive");
    }
}
