using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec015;

public sealed class Endpoint05ContractTests
{
    [Fact]
    public void Admin_detail_contract_hides_every_scope_mismatch_and_audits_inspection()
    {
        var contract = RepositoryFiles.Read("specs/015-student-registration-records/contracts/api.md");
        Assert.Contains("GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}", contract);
        Assert.Contains("any student/term/submission mismatch", contract);
        Assert.Contains("RegistrationRecords.Read", contract);
        Assert.Contains("receipt JSON", contract);
    }
}
