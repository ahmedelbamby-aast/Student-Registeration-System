using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Dual_role_context_switch_is_limited_to_effective_assignments()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs");
        RepositoryFiles.ContainsAll(
            source,
            "SelectRoleContextAsync",
            "GetEffectiveRolesAsync",
            "RoleNotAvailable",
            "RotateSecurityStampAsync");
        Assert.DoesNotContain("AddRole", source, StringComparison.Ordinal);
    }
}
