using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class UserAdministrationPageFeatureTests
{
    [Fact]
    public void Admin_user_lifecycle_consumes_identity_owned_versioned_actions()
    {
        var contribution = RepositoryFiles.Read(
            "specs/007-identity-account-lifecycle/contracts/routes/ADM-03.md");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor");
        var implementation = page + RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs");

        RepositoryFiles.ContainsAll(
            contribution,
            "GET /api/admin/users",
            "POST /api/admin/users/imports",
            "PATCH /api/admin/users/{userId}/status",
            "PUT /api/admin/users/{userId}/roles",
            "expectedRowVersion",
            "FINAL_ADMIN_REQUIRED");
        RepositoryFiles.ContainsAll(
            implementation,
            "@page \"/admin/users\"",
            "IdentityUserSummaryDto",
            "IdentityImportRequest",
            "IdentityImportPublishRequest",
            "UserStatusRequest",
            "UserRolesRequest",
            "ListAdminUsersAsync",
            "CreateAdminImportAsync",
            "PublishAdminImportAsync",
            "ChangeAdminUserStatusAsync",
            "ReplaceAdminUserRolesAsync",
            "ConfirmationDialog",
            "expectedRowVersion",
            "FINAL_ADMIN_REQUIRED",
            "STALE_VERSION");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", page, StringComparison.Ordinal);
    }
}
