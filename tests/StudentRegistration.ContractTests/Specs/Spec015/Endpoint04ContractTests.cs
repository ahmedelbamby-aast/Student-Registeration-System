using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec015;

public sealed class Endpoint04ContractTests
{
    [Fact]
    public void Admin_list_contract_requires_explicit_scope_and_minimal_audit()
    {
        var contract = RepositoryFiles.Read("specs/015-student-registration-records/contracts/api.md");
        Assert.Contains("GET /api/admin/students/{studentId}/terms/{termId}/registrations", contract);
        Assert.Contains("RegistrationRecords.Read", contract);
        Assert.Contains("endpoint kind (`list` or `detail`), outcome, correlation ID", contract);
        Assert.Contains("MUST NOT contain University ID", contract);
    }
}
