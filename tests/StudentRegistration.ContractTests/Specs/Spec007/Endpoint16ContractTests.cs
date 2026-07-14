using Microsoft.AspNetCore.Authorization;

namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint16ContractTests
{
    [Fact]
    public void Role_replacement_has_allow_list_reason_and_one_aggregate_version()
    {
        var request = Spec007ContractAssertions.ContractType("UserRolesRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "Roles",
            "ExpectedRowVersion",
            "Reason");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "Claims",
            "Permissions",
            "SecurityStamp",
            "RoleVersion",
            "AdminGuardVersion");
    }

    [Fact]
    public void Role_replacement_requires_antiforgery_identity_admin_and_guarded_atomic_write()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "PUT",
            "/api/admin/users/{userId}/roles");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "IdentityManagement");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/Ports/IAdminUserLifecycleStore.cs"),
            "ReplaceUserRolesAsync",
            "AdminSecurityGuard",
            "SecurityEvent and AuditEvent",
            "FINAL_ADMIN_REQUIRED");
    }
}
