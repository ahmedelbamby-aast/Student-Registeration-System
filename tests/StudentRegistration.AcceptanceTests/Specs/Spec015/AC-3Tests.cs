using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec015;
public sealed class AC_3Tests
{
    [Fact] public void Ownership_and_admin_audit_are_explicit()
    {
        var endpoints = RepositoryFiles.Read("src/StudentRegistration.Registration/Endpoints/Spec015Endpoints.cs");
        var adapter = RepositoryFiles.Read("src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationRecordReader.cs");
        Assert.Contains("RegistrationRecords.ReadOwn", endpoints);
        Assert.Contains("RegistrationRecords.Read", endpoints);
        Assert.Contains("Status404NotFound", endpoints);
        Assert.Contains("RegistrationRecordInspected", adapter);
    }
}
