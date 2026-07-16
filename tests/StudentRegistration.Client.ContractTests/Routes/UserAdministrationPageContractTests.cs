using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class UserAdministrationPageContractTests
{
    [Fact]
    public void Adm_03_fixture_and_page_bind_the_complete_identity_command_surface()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/ADM-03/route-contract.json"));
        var root = document.RootElement;
        Assert.Equal("frontend-fixture/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("ADM-03", root.GetProperty("routeId").GetString());
        Assert.Equal("/admin/users", root.GetProperty("routeTemplate").GetString());
        Assert.Equal(6, root.GetProperty("apis").GetArrayLength());
        Assert.Equal(9, root.GetProperty("requiredStates").GetArrayLength());

        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor");
        var logic = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs");
        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityApiClient.cs");
        RepositoryFiles.ContainsAll(
            page,
            "@page \"/admin/users\"",
            "role=\"search\"",
            "User results",
            "expectedRowVersion",
            "ConfirmationDialog",
            "Preview identity import");
        RepositoryFiles.ContainsAll(
            logic,
            "ListAdminUsersAsync",
            "CreateAdminImportAsync",
            "GetAdminImportAsync",
            "PublishAdminImportAsync",
            "ChangeAdminUserStatusAsync",
            "ReplaceAdminUserRolesAsync",
            "FINAL_ADMIN_REQUIRED",
            "STALE_VERSION",
            "UNAUTHORIZED",
            "FORBIDDEN");
        RepositoryFiles.ContainsAll(
            client,
            "api/admin/users?",
            "api/admin/users/imports",
            "/publish",
            "/status",
            "/roles");
    }

    [Fact]
    public void Adm_03_preserves_server_authority_and_bounded_queries()
    {
        var logic = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs");
        var contribution = RepositoryFiles.Read(
            "specs/007-identity-account-lifecycle/contracts/routes/ADM-03.md");

        RepositoryFiles.ContainsAll(
            logic,
            "private const int PageSize = 20;",
            "private const string DefaultSort = \"displayName,id\";",
            "new UserStatusRequest(",
            "_selectedUser.RowVersion",
            "new UserRolesRequest(",
            "IdentityImportContentHash.Compute");
        RepositoryFiles.ContainsAll(
            contribution,
            "final-enabled-Admin guard",
            "`STALE_VERSION`",
            "same-origin antiforgery request token",
            "stores no credential or long-lived browser token");
        Assert.DoesNotContain("localStorage", logic, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DateTime.Now", logic, StringComparison.Ordinal);
    }
}
