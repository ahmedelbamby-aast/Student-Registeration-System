using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec004;

public sealed class AC_1Tests
{
    [Fact]
    public void Registration_reference_to_an_academics_internal_type_is_a_build_failure()
    {
        // Given the approved Academics and Registration module boundaries.
        var boundaries = RepositoryFiles.Read("docs/architecture/module-boundaries.md");

        // When the executable dependency rule evaluates a direct internal reference.
        // Then the canonical contract requires the architecture test to fail the build.
        RepositoryFiles.ContainsAll(
            boundaries,
            "StudentRegistration.Academics",
            "StudentRegistration.Registration",
            "module internals",
            "ModuleDependencyTests",
            "fail the build");

        var executableRules = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs");
        RepositoryFiles.ContainsAll(
            executableRules,
            "Registration",
            "Academics",
            "Internal",
            "Assert.DoesNotContain");
    }
}
