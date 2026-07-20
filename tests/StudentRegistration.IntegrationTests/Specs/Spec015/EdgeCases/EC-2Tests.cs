using StudentRegistration.Registration.Application;
using StudentRegistration.IntegrationTests.Registration;

namespace StudentRegistration.IntegrationTests.Specs.Spec015.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Render_failure_does_not_mutate_and_a_retry_can_project_the_canonical_record()
    {
        var service = new RegistrationReceiptService();
        var valid = RegistrationReceiptTests.AcceptedRecord();
        var invalid = valid with { ReceiptSnapshotJson = "{" };
        Assert.Throws<RegistrationRecordProjectionException>(() => service.ProjectDetail(invalid));
        Assert.Equal(valid.Reference, service.ProjectDetail(valid).Receipt!.Reference);
    }
}
