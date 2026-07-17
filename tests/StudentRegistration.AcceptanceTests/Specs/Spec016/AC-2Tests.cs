using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_2Tests
{
    [Fact]
    public void Shared_workspace_projects_only_server_authorized_role_context()
    {
        var queries = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        RepositoryFiles.ContainsAll(
            queries,
            "ListAssignmentsAsync",
            "LecturerRole",
            "TeachingAssistantRole",
            "GetAssignmentsAsync");
        RepositoryFiles.ContainsAll(page, "Role context", "StaffAssignmentDto");
    }
}
