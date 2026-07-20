using StudentRegistration.Registration.Application;
using StudentRegistration.IntegrationTests.Registration;

namespace StudentRegistration.IntegrationTests.Specs.Spec015.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Lost_response_retry_projects_the_same_receipt_reference()
    {
        var service = new RegistrationReceiptService();
        var record = RegistrationReceiptTests.AcceptedRecord();
        Assert.Equal(service.ProjectDetail(record).Receipt!.Reference, service.ProjectDetail(record).Receipt!.Reference);
    }
}
