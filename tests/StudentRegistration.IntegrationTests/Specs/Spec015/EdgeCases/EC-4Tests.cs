using StudentRegistration.Registration.Application;
using StudentRegistration.IntegrationTests.Registration;

namespace StudentRegistration.IntegrationTests.Specs.Spec015.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Archived_history_remains_projectable_and_has_no_mutation_action()
    {
        var detail = new RegistrationReceiptService().ProjectDetail(RegistrationReceiptTests.AcceptedRecord());
        Assert.NotNull(detail.Receipt);
        Assert.Empty(RegistrationRecordActionPolicy.AllowedActions);
    }
}
