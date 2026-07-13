using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec001;

public sealed class RbacMatrixTests
{
    private const string MatrixPath =
        "specs/001-product-charter-rbac/contracts/rbac-matrix.md";

    [Fact]
    public void Matrix_maps_every_human_role_to_governed_permissions_and_scope()
    {
        var matrix = RepositoryFiles.Read(MatrixPath);
        var grants = RepositoryFiles.Section(matrix, "Human role grants");

        RepositoryFiles.ContainsAll(
            grants,
            "`Student`",
            "`Admin`",
            "`Lecturer`",
            "`TeachingAssistant`",
            "Data-scope rule",
            "Explicit denials");
    }

    [Fact]
    public void Matrix_covers_dual_role_no_role_and_client_route_denials()
    {
        var matrix = RepositoryFiles.Read(MatrixPath);

        RepositoryFiles.ContainsAll(
            matrix,
            "Dual Lecturer and TeachingAssistant",
            "No supported staff role",
            "Direct route or client role change",
            "deny");
    }

    [Fact]
    public void Matrix_references_governed_catalogue_without_owning_runtime_policies()
    {
        var matrix = RepositoryFiles.Read(MatrixPath);

        RepositoryFiles.ContainsAll(
            matrix,
            "`rbac-matrix/1.0`",
            "`permissions.md`",
            "Runtime authorization owner: SPEC-007",
            "`Registration.Reconcile` is never granted to a human role");
    }
}
