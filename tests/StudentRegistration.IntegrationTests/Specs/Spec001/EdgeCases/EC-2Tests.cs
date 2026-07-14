using StudentRegistration.IntegrationTests.Identity;

namespace StudentRegistration.IntegrationTests.Specs.Spec001.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public async Task Staff_identity_without_supported_role_is_denied_safely()
    {
        var fixture = new IdentityServiceTestFixture();
        fixture.AddStaff(
            "unsupported.context",
            "correct horse battery staple",
            ["Student", "Unsupported"]);

        var result = await fixture.CreateStaffAuthentication().AuthenticateAsync(
            "unsupported.context",
            "correct horse battery staple");

        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);
        Assert.Null(result.DisplayName);
        Assert.Empty(result.AuthorizedRoles);
        Assert.Null(result.ActiveRole);
        Assert.Null(result.ExpiresAtUtc);
    }
}
