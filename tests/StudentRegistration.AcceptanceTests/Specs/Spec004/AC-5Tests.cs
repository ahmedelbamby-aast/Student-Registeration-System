using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec004;

public sealed class AC_5Tests
{
    [Fact]
    public void Boundary_or_deployment_change_requires_an_approved_adr_and_test_update()
    {
        // Given a pull request changes a module dependency or deployment decision.
        var registry = RepositoryFiles.Read("docs/adr/README.md");

        // When architecture governance runs, both approval and executable evidence are mandatory.
        RepositoryFiles.ContainsAll(
            registry,
            "module dependency",
            "deployment decision",
            "approved",
            "Ahmed ELbamby",
            "architecture-test update");

        // Then CI has an executable rule rather than relying only on review prose.
        var executableRules = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs");
        RepositoryFiles.ContainsAll(
            executableRules,
            "ADR",
            "ModuleDependencyTests",
            "Assert.");
    }
}
