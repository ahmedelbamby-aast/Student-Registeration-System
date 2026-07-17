using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec015;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Current_timetable_contract_defines_every_safe_outcome()
    {
        var contract = RepositoryFiles.Read("specs/015-student-registration-records/contracts/api.md");
        Assert.Contains("RegistrationTimetableDto", contract);
        Assert.Contains("CURRENT_TERM_AMBIGUOUS", contract);
        Assert.Contains("RegistrationRecords.ReadOwn", contract);
        Assert.Contains("canonical rate limit", contract);
        Assert.Contains("Archived terms never become current", contract);
        Assert.Contains("/student/subjects", contract);
        Assert.Contains("(OpensAtUtc, Id)", contract);
        Assert.Contains("(ClosesAtUtc DESC, Id)", contract);
    }
}
