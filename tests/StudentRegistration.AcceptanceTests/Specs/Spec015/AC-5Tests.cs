using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec015;
public sealed class AC_5Tests
{
    [Fact] public void Current_and_history_are_read_only_and_share_one_group_collection()
    {
        var contracts = RepositoryFiles.Read("src/StudentRegistration.Contracts/Registration/RegistrationRecordContracts.cs");
        var policy = RepositoryFiles.Read("src/StudentRegistration.Registration/Application/RegistrationRecordActionPolicy.cs");
        Assert.Contains("RegistrationTimetableDto", contracts);
        Assert.Contains("Groups", contracts);
        Assert.Contains("Drop", policy);
        Assert.Contains("Correction", policy);
    }
}
