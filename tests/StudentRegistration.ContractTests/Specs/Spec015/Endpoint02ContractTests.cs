using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec015;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Student_detail_contract_is_private_and_snapshot_based()
    {
        var contract = RepositoryFiles.Read("specs/015-student-registration-records/contracts/api.md");
        Assert.Contains("GET /api/student/registrations/{submissionId}", contract);
        Assert.Contains("cross-owner submission", contract);
        Assert.Contains("privacy-safe 404", contract);
        Assert.Contains("RegistrationTermSnapshotDto", contract);
        Assert.Contains("noPartialRegistration: true", contract);
    }
}
