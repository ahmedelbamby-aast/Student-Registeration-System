using StudentRegistration.Registration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec015;

public sealed class Endpoint02And05BehaviorTests
{
    [Fact]
    public async Task Detail_misses_are_privacy_safe_for_self_and_admin_scope()
    {
        var reader = new Endpoint01And04BehaviorTests.CapturingReader();
        var queries = new RegistrationRecordQueries(reader, new RegistrationReceiptService());

        var self = await queries.ReadOwnAsync(
            Endpoint01And04BehaviorTests.Principal("Student"), Guid.NewGuid());
        Assert.Equal(RegistrationRecordQueryOutcome.NotFound, self.Outcome);
        Assert.Equal("REGISTRATION_NOT_FOUND", self.ErrorCode);

        var admin = await queries.ReadAdminAsync(
            Endpoint01And04BehaviorTests.Principal("Admin"),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "corr-2");
        Assert.Equal(RegistrationRecordQueryOutcome.NotFound, admin.Outcome);
        Assert.Equal(Endpoint01And04BehaviorTests.ReaderCall.AdminDetail, reader.LastCall);
    }
}
