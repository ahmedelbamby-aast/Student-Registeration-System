using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec015;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Student_list_contract_is_bounded_self_scoped_and_stably_sorted()
    {
        var contract = RepositoryFiles.Read("specs/015-student-registration-records/contracts/api.md");
        Assert.Contains("GET /api/student/registrations?page=1&pageSize=20", contract);
        Assert.Contains("RegistrationRecords.ReadOwn", contract);
        Assert.Contains("SubmittedAtUtc descending then SubmissionId", contract);
        Assert.Contains("PAGE_SIZE_INVALID", contract);
        Assert.Contains("no response exposes SQL", contract);
        Assert.Contains("RegistrationHistoryRowDto.termState", contract);
        Assert.Contains("archived rows", contract);
    }
}
