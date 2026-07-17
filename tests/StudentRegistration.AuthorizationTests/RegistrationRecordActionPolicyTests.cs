using StudentRegistration.Registration.Application;

namespace StudentRegistration.AuthorizationTests;

public sealed class RegistrationRecordActionPolicyTests
{
    [Theory]
    [InlineData("drop")]
    [InlineData("withdrawal")]
    [InlineData("correction")]
    public void Unapproved_record_mutations_are_never_allowed(string action)
    {
        Assert.False(RegistrationRecordActionPolicy.IsAllowed(action));
        Assert.Empty(RegistrationRecordActionPolicy.AllowedActions);
    }
}
