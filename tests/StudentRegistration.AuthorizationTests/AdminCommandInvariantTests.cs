using StudentRegistration.TestSupport;

namespace StudentRegistration.AuthorizationTests;

public sealed class AdminCommandInvariantTests
{
    [Fact]
    public void Admin_pages_delegate_mutations_to_the_canonical_feature_owners()
    {
        AssertDelegation(
            "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor",
            "AcademicApi",
            "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs",
            "/api/admin/terms");
        AssertDelegation(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs",
            "IdentityApi",
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs",
            "/api/admin/users");
        AssertDelegation(
            "src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor",
            "AcademicApi",
            "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs",
            "/api/admin/students");
        AssertDelegation(
            "src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor",
            "CatalogueApi",
            "src/StudentRegistration.Academics/Endpoints/Spec009Endpoints.cs",
            "/api/admin/catalogue");
        AssertDelegation(
            "src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor",
            "SchedulingApi",
            "src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs",
            "/api/admin/offerings");
        AssertDelegation(
            "src/StudentRegistration.Client/Pages/ResourceAdministrationPage.razor",
            "SchedulingApi",
            "src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs",
            "/api/admin/rooms");

        Assert.False(RepositoryFiles.Exists(
            "src/StudentRegistration.StaffAdministration/Application/AdminCommandService.cs"));
        Assert.False(RepositoryFiles.Exists(
            "src/StudentRegistration.StaffAdministration/Application/AdminConfirmationService.cs"));
    }

    [Fact]
    public void Availability_is_imported_read_only_and_no_bypass_or_registration_correction_surface_exists()
    {
        var schedulingEndpoints = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs");
        RepositoryFiles.ContainsAll(
            schedulingEndpoints,
            "endpoints.MapGet(",
            "\"/api/admin/staff-availability\"");
        Assert.DoesNotContain(
            "MapPost(\"/api/admin/staff-availability",
            schedulingEndpoints,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MapPut(\"/api/admin/staff-availability",
            schedulingEndpoints,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MapPatch(\"/api/admin/staff-availability",
            schedulingEndpoints,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MapDelete(\"/api/admin/staff-availability",
            schedulingEndpoints,
            StringComparison.Ordinal);

        var spec017Source = ReadTree("src/StudentRegistration.StaffAdministration");
        foreach (var forbidden in new[]
        {
            "EnrollmentCorrection",
            "DropEnrollment",
            "WithdrawEnrollment",
            "DecrementSeat",
            "CapacityOverride",
            "ConflictOverride",
            "StaffTermAvailabilityWriter",
            "AvailabilityCorrection",
            "AvailabilityOverride"
        })
        {
            Assert.DoesNotContain(forbidden, spec017Source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Preview_confirmation_remains_owner_specific_and_preserves_server_authority()
    {
        var catalogue = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/PublicationConfirmationService.cs");
        var offering = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Application/OfferingPublicationService.cs");
        var identityStore = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/Ports/IAdminUserLifecycleStore.cs");

        RepositoryFiles.ContainsAll(
            catalogue,
            "ActorReference",
            "ScopeCode",
            "ExpectedRowVersion",
            "DependencyVersions",
            "ExpiresAtUtc",
            "StalePreview",
            "IdempotencyKeyReused");
        RepositoryFiles.ContainsAll(
            offering,
            "PreviewToken",
            "ClientRequestId",
            "ExpectedOfferingVersion",
            "ExpectedGroupVersions",
            "ExpectedRoomVersions",
            "ExpectedStaffTermAvailabilityVersions");
        RepositoryFiles.ContainsAll(
            identityStore,
            "AdminSecurityGuard",
            "FINAL_ADMIN_REQUIRED");
    }

    [Fact]
    public void No_break_glass_or_generic_admin_command_facade_exists()
    {
        var source = ReadTree("src");

        Assert.DoesNotContain("BreakGlass", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AdminCommandService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdminConfirmationService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("FinalAdminOverride", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Admin_role_changes_remain_owned_and_serialized_by_spec007()
    {
        var identityEndpoint = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");
        var identityStore = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/AdminUserLifecycleStore.cs");
        var staffAdministration = ReadTree("src/StudentRegistration.StaffAdministration");

        RepositoryFiles.ContainsAll(
            identityEndpoint,
            "MapPut(\"/api/admin/users/{userId}/roles\"",
            "StatusCodes.Status409Conflict",
            "FINAL_ADMIN_REQUIRED");
        RepositoryFiles.ContainsAll(
            identityStore,
            "AdminSecurityGuard",
            "FinalAdminRequired");
        Assert.DoesNotContain("RoleAssignment", staffAdministration, StringComparison.Ordinal);
        Assert.DoesNotContain("AdminSecurityGuard", staffAdministration, StringComparison.Ordinal);
    }

    private static void AssertDelegation(
        string pagePath,
        string clientMarker,
        string endpointPath,
        string routeMarker)
    {
        Assert.Contains(clientMarker, RepositoryFiles.Read(pagePath), StringComparison.Ordinal);
        Assert.Contains(routeMarker, RepositoryFiles.Read(endpointPath), StringComparison.Ordinal);
    }

    private static string ReadTree(string relativeDirectory)
    {
        var directory = RepositoryFiles.PathTo(relativeDirectory);
        return string.Join(
            '\n',
            Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(directory, "*.razor", SearchOption.AllDirectories))
                .Where(path => !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));
    }
}
