using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class AC_2Tests
{
    [Fact]
    public void Staff_context_is_derived_from_effective_server_roles()
    {
        // Given a staff account has an effective Lecturer role; when it signs in.
        var scope = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/server-derived-scope.md");
        var matrix = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/rbac-matrix.md");

        // Then Lecturer is an authorized context and a direct Admin route adds nothing.
        RepositoryFiles.ContainsAll(
            scope,
            "effective server role set",
            "client-supplied role",
            "deny");
        RepositoryFiles.ContainsAll(
            matrix,
            "`Lecturer`",
            "Direct route or client role change");
    }
}
