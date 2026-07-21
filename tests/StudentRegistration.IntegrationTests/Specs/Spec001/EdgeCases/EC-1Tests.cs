using StudentRegistration.IntegrationTests.Identity;

namespace StudentRegistration.IntegrationTests.Specs.Spec001.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public async Task Combined_lecturer_and_ta_identity_fails_closed()
    {
        var fixture = new IdentityServiceTestFixture();
        fixture.AddStaff(
            "dual.context",
            "correct horse battery staple",
            ["Student", "TeachingAssistant", "Lecturer", "Unsupported"]);

        var result = await fixture.CreateStaffAuthentication().AuthenticateAsync(
            "dual.context",
            "correct horse battery staple");

        Assert.False(result.Succeeded);
        Assert.Null(result.ActiveRole);
        Assert.Empty(result.AuthorizedRoles);
    }
}
