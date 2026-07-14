using StudentRegistration.IntegrationTests.Identity;

namespace StudentRegistration.IntegrationTests.Specs.Spec001.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public async Task Dual_lecturer_and_ta_identity_offers_only_effective_contexts()
    {
        var fixture = new IdentityServiceTestFixture();
        fixture.AddStaff(
            "dual.context",
            "correct horse battery staple",
            ["Student", "TeachingAssistant", "Lecturer", "Unsupported"]);

        var result = await fixture.CreateStaffAuthentication().AuthenticateAsync(
            "dual.context",
            "correct horse battery staple");

        Assert.True(result.Succeeded);
        Assert.True(result.RoleSelectionRequired);
        Assert.Null(result.ActiveRole);
        Assert.Equal(["Lecturer", "TeachingAssistant"], result.AuthorizedRoles);
    }
}
