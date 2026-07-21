using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Runtime_has_no_role_context_switch_capability()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/SessionLifecycleService.cs");
        Assert.DoesNotContain("SelectRoleContextAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RoleNotAvailable", source, StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(source, "GetEffectiveRolesAsync", "roles.Count != 1");
        Assert.DoesNotContain("AddRole", source, StringComparison.Ordinal);
    }
}
