using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RegistrationApprovalPageContractTests
{
    [Fact]
    public void Adm_10_and_stf_05_preserve_separate_owner_endpoints_and_one_decision_contract()
    {
        var workspace = RepositoryFiles.Read("src/StudentRegistration.Client/Components/Approvals/ApprovalWorkspace.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Registration/RegistrationApprovalApiClient.cs");

        RepositoryFiles.ContainsAll(workspace,
            "ListAsync(IsAdmin)", "DecideAsync(IsAdmin", "row.SubmissionVersion",
            "row.Line.RowVersion", "ApproveAsync", "RejectAsync");
        RepositoryFiles.ContainsAll(client,
            "\"/api/admin/registration-approvals\"", "\"/api/staff/registration-approvals\"",
            "X-XSRF-TOKEN", "ExpectedSubmissionRowVersion", "ExpectedLineRowVersion", "ClientRequestId");
    }
}
