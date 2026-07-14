using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec001;

public sealed class PermissionDefinitionTests
{
    private const string PermissionsPath =
        "specs/001-product-charter-rbac/contracts/permissions.md";

    [Fact]
    public void Catalogue_defines_stable_capability_scope_operation_and_denial_fields()
    {
        var permissions = RepositoryFiles.Read(PermissionsPath);
        var catalogue = RepositoryFiles.Section(permissions, "Permission definitions");

        RepositoryFiles.ContainsAll(
            catalogue,
            "Permission token",
            "Allowed role or principal",
            "Data scope",
            "Allowed operations",
            "Explicit denial");

        Assert.Equal(
            18,
            catalogue.Split('\n').Count(line => line.StartsWith("| `", StringComparison.Ordinal)));

        RepositoryFiles.ContainsAll(
            catalogue,
            "`AcademicTerms.Manage`",
            "`AcademicProfiles.Manage`",
            "No unrestricted student dump",
            "implicit access from Admin role alone");
    }

    [Fact]
    public void Catalogue_preserves_fixed_registration_permissions_and_runtime_owner()
    {
        var permissions = RepositoryFiles.Read(PermissionsPath);

        RepositoryFiles.ContainsAll(
            permissions,
            "`RegistrationRecords.Read`",
            "`Registration.Reconcile`",
            "service identity only",
            "Executable policy owner: SPEC-007");

        Assert.DoesNotMatch(
            new Regex(@"\| `Registration\.Reconcile` \| (Student|Admin|Lecturer|TeachingAssistant)", RegexOptions.CultureInvariant),
            permissions);
    }

    [Fact]
    public void Every_human_role_has_context_access_but_not_unbounded_scope()
    {
        var permissions = RepositoryFiles.Read(PermissionsPath);

        RepositoryFiles.ContainsAll(
            permissions,
            "Student, Admin, Lecturer, TeachingAssistant",
            "server-composed current context",
            "Route or request input cannot widen the scope");
    }
}
