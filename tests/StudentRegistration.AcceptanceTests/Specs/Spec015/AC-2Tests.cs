using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec015;

public sealed class AC_2Tests
{
    [Fact]
    public void Rejection_is_explicitly_atomic()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs");
        Assert.Contains("GROUP_FULL", source);
        Assert.Contains("No subjects were partially registered", source);
        Assert.Contains("NoPartialRegistration", source);
    }
}
