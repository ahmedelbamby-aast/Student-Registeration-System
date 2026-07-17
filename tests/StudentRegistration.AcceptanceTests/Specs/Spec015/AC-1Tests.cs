using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec015;
public sealed class AC_1Tests
{
    [Fact] public void Accepted_receipt_projects_every_historical_field()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs");
        foreach (var value in new[] { "Reference", "PolicyVersion", "SubmittedAtUtc", "Groups", "Meetings", "Staff", "Credits" })
            Assert.Contains(value, source);
    }
}
