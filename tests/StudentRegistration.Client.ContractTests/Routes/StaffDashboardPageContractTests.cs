using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StaffDashboardPageContractTests
{
    [Fact]
    public void Stf_01_uses_only_server_context_and_assignment_contracts()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Staff/StaffApiClient.cs");

        RepositoryFiles.ContainsAll(page,
            "@page \"/staff\"", "data-route-id=\"STF-01\"",
            "GetAppContextAsync", "GetAssignmentsAsync", "ROLE_CONTEXT_INVALID",
            "AuthenticatedPage", "WorkspaceKind.Staff", "Open assigned roster", "Open availability");
        RepositoryFiles.ContainsAll(client,
            "GetAssignmentsAsync", "\"/api/staff/assignments\"");
        Assert.DoesNotContain("role=", client, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("staffId", client, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SelectRoleContextAsync", page, StringComparison.Ordinal);
    }
}
