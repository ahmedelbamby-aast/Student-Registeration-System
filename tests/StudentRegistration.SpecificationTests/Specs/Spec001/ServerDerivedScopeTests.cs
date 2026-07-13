using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec001;

public sealed class ServerDerivedScopeTests
{
    [Fact]
    public void Server_is_authoritative_for_roles_permissions_and_resource_scope()
    {
        var scope = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/contracts/server-derived-scope.md");

        RepositoryFiles.ContainsAll(
            scope,
            "authenticated server claims",
            "effective server role set",
            "resource ownership",
            "client-supplied role",
            "fail closed");
    }
}
