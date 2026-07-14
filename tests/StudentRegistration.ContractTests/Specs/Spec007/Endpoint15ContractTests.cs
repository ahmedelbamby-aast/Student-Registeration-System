using Microsoft.AspNetCore.Authorization;

namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint15ContractTests
{
    [Fact]
    public void Status_command_has_one_aggregate_version_and_reason()
    {
        var request = Spec007ContractAssertions.ContractType("UserStatusRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "Enabled",
            "ExpectedRowVersion",
            "Reason");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "SecurityStamp",
            "RoleVersion",
            "AdminGuardVersion");
    }

    [Fact]
    public void Status_command_requires_antiforgery_identity_admin_and_guarded_atomic_write()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "PATCH",
            "/api/admin/users/{userId}/status");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "IdentityManagement");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/Ports/IAdminUserLifecycleStore.cs"),
            "SetUserStatusAsync",
            "AdminSecurityGuard",
            "SecurityEvent and AuditEvent",
            "FINAL_ADMIN_REQUIRED");
    }
}
