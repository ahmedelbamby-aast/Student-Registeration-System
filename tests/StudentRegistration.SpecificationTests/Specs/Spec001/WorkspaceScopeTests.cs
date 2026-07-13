using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec001;

public sealed class WorkspaceScopeTests
{
    [Fact]
    public void Every_role_has_an_explicit_workspace_and_data_scope_boundary()
    {
        var matrix = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/rbac-matrix.md");
        var grants = RepositoryFiles.Section(matrix, "Human role grants");

        RepositoryFiles.ContainsAll(
            grants,
            "student workspace",
            "admin workspace",
            "lecturer workspace",
            "teaching-assistant workspace",
            "assigned-groups",
            "institutional-admin");
    }
}
