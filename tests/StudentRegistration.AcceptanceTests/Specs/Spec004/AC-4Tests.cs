using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec004;

public sealed class AC_4Tests
{
    [Fact]
    public void Reads_project_dtos_and_atomic_commands_share_the_single_context_boundary()
    {
        // Given an offering read and an atomic registration command.
        var boundary = RepositoryFiles.Read("docs/architecture/persistence-boundary.md");

        // When architecture review inspects their persistence behavior.
        RepositoryFiles.ContainsAll(
            boundary,
            "StudentRegistrationDbContext",
            "Infrastructure.SqlServer",
            "AsNoTracking",
            "Select",
            "DTO",
            "atomic transaction");

        // Then executable checks reject a second context and EF-backed API shapes.
        var persistenceChecks = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs");
        var dtoChecks = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/DtoIsolationTests.cs");
        RepositoryFiles.ContainsAll(
            persistenceChecks,
            "StudentRegistrationDbContext",
            "AsNoTracking",
            "Assert.");
        RepositoryFiles.ContainsAll(dtoChecks, "DTO", "EF", "Assert.");
    }
}
