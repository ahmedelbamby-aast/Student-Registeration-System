using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class UserAdministrationPageContributorTests
{
    [Fact]
    public void Adm_03_delegates_identity_import_status_and_role_commands()
    {
        var (contract, page) = ContributorContractAssertions.Load(
            "ADM-03",
            "SPEC-007",
            "UserAdministrationPage.razor",
            "/admin/users",
            "spec017-adm03/1.0");

        RepositoryFiles.ContainsAll(
            contract,
            "GET /api/admin/users",
            "PATCH /api/admin/users/{userId}/status",
            "PUT /api/admin/users/{userId}/roles",
            "IdentityAccess.Manage",
            "expected user rowversion",
            "FINAL_ADMIN_REQUIRED",
            "STALE_VERSION",
            "IDEMPOTENCY_KEY_REUSED",
            "AdminSecurityGuard",
            "RoleAssignment",
            "SecurityEvent",
            "shared `AuditEvent`");
        RepositoryFiles.ContainsAll(
            page,
            "IdentityApi.ListAdminUsersAsync",
            "IdentityApi.ChangeAdminUserStatusAsync",
            "IdentityApi.ReplaceAdminUserRolesAsync",
            "IdentityApi.CreateAdminImportAsync",
            "IdentityApi.PublishAdminImportAsync",
            "The server accepted and audited");
    }

    [Fact]
    public void Adm_03_preserves_final_admin_and_identity_ownership()
    {
        var contract = ContributorContractAssertions.Contract("ADM-03");
        RepositoryFiles.ContainsAll(
            contract,
            "returns 409 `FINAL_ADMIN_REQUIRED`",
            "leaves at least one enabled Admin",
            "no `RoleAssignment` writer",
            "guard writer",
            "credential display",
            "final-Admin override");
        Assert.DoesNotContain("Canonical page owner:** SPEC-017", contract);
    }
}
