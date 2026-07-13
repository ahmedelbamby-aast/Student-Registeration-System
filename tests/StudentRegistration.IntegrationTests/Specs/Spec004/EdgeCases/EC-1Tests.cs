using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec004.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Additional_cross_module_read_uses_a_narrow_query_interface()
    {
        // Given a module requests an additional read from another owner.
        var boundaries = RepositoryFiles.Read("docs/architecture/module-boundaries.md");

        // When the boundary is reviewed, direct internal/table access is rejected.
        RepositoryFiles.ContainsAll(
            boundaries,
            "additional read",
            "narrow query interface",
            "direct table",
            "module internals");

        // Then the executable dependency rule guards the narrow contract boundary.
        var executableRules = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs");
        RepositoryFiles.ContainsAll(
            executableRules,
            "Internal",
            "AllowedProjectReferences",
            "Assert.DoesNotContain");
    }
}
