using Microsoft.AspNetCore.Authorization;

namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint11ContractTests
{
    [Fact]
    public void User_list_is_minimized_bounded_and_versioned()
    {
        var dto = Spec007ContractAssertions.ContractType("IdentityUserSummaryDto");

        Spec007ContractAssertions.HasExactProperties(
            dto,
            "Id",
            "DisplayName",
            "LoginIdentifier",
            "Enabled",
            "Roles",
            "RowVersion");
        Spec007ContractAssertions.ExcludesProperties(
            dto,
            "PasswordHash",
            "SecurityStamp",
            "RecoveryToken",
            "RoleAssignmentVersion");
    }

    [Fact]
    public void User_list_requires_the_identity_management_policy()
    {
        var endpoint = Spec007ContractAssertions.Endpoint("GET", "/api/admin/users");

        Spec007ContractAssertions.IsProtected(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "IdentityManagement");
        Spec007ContractAssertions.DeclaresResponseStatus(endpoint, 200);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/AdminUserLifecycleService.cs"),
            "DefaultPageSize = 20",
            "MaximumPageSize = 100",
            "displayName,id");
    }

    [Fact]
    public void Identity_management_permission_is_issued_only_from_server_roles()
    {
        var staffLogin = Spec007ContractAssertions.ContractType("StaffLoginRequest");
        Spec007ContractAssertions.ExcludesProperties(
            staffLogin,
            "Role",
            "Roles",
            "Claims",
            "Permissions");

        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.EndpointSource(),
            "string.Equals(result.ActiveRole, RolePolicies.Admin",
            "RolePolicies.AvailableRoleClaimType",
            "RolePolicies.PermissionClaimType",
            "RolePolicies.IdentityManagement");
    }
}
