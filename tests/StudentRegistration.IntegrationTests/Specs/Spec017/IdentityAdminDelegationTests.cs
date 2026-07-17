using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec017;

public sealed class IdentityAdminDelegationTests
{
    [Fact]
    public void Admin_page_delegates_role_replacement_to_the_spec007_identity_endpoint()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs");
        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityApiClient.cs");
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");
        var service = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/AdminUserLifecycleService.cs");

        RepositoryFiles.ContainsAll(
            page,
            "IdentityApi.ReplaceAdminUserRolesAsync",
            "new UserRolesRequest(",
            "_selectedUser.RowVersion",
            "_roleReason");
        RepositoryFiles.ContainsAll(
            client,
            "HttpMethod.Put",
            "$\"api/admin/users/{userId:D}/roles\"");
        RepositoryFiles.ContainsAll(
            endpoint,
            "MapPut(\"/api/admin/users/{userId}/roles\"",
            "ReplaceUserRolesAsync",
            "RequireAuthorization(RolePolicies.IdentityManagement)",
            "StatusCodes.Status409Conflict",
            "FINAL_ADMIN_REQUIRED");
        RepositoryFiles.ContainsAll(
            service,
            "IAdminUserLifecycleStore",
            "ReplaceUserRolesAsync",
            "AdminUserLifecycleOutcome.FinalAdminRequired");
    }

    [Fact]
    public void Staff_administration_adds_no_identity_role_or_guard_writer()
    {
        var source = ReadStaffAdministrationSources();

        Assert.DoesNotContain("AdminCommandService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdminConfirmationService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IAdminUserLifecycleStore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new RoleAssignment", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new AdminSecurityGuard", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<RoleAssignment>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<AdminSecurityGuard>", source, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Two_replica_revocation_uses_identity_guard_and_leaves_one_admin()
    {
        var upstream = new Spec007.AdminUserLifecycleStorePersistenceTests();

        await upstream.Real_sql_admin_lifecycle_is_idempotent_guarded_and_audit_atomic();
    }

    private static string ReadStaffAdministrationSources() => string.Join(
        "\n",
        Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.StaffAdministration"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .Select(File.ReadAllText));
}
