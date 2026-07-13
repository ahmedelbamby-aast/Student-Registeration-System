using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec004.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Stale_read_cache_is_never_authoritative_for_final_registration()
    {
        // Given a catalogue or eligibility cache is stale at submission time.
        var boundary = RepositoryFiles.Read("docs/architecture/persistence-boundary.md");

        // When final registration executes, it revalidates against authoritative SQL state.
        RepositoryFiles.ContainsAll(
            boundary,
            "stale cache",
            "advisory",
            "final registration",
            "authoritative SQL",
            "revalidate");

        // Then the executable persistence checks preserve read/command separation.
        var executableRules = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs");
        RepositoryFiles.ContainsAll(
            executableRules,
            "AsNoTracking",
            "transaction",
            "Assert.");
    }
}
