using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec004.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Cross_module_transaction_stays_in_the_single_context_or_stops_for_review()
    {
        // Given a new use case needs one transaction across module-owned data.
        var boundary = RepositoryFiles.Read("docs/architecture/persistence-boundary.md");

        // When the transaction boundary is evaluated, no second context or distributed protocol is invented.
        RepositoryFiles.ContainsAll(
            boundary,
            "cross-module transaction",
            "single StudentRegistrationDbContext",
            "architecture review",
            "distributed transaction");

        // Then executable checks enforce the one-context transaction boundary.
        var executableRules = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs");
        RepositoryFiles.ContainsAll(
            executableRules,
            "StudentRegistrationDbContext",
            "transaction",
            "Assert.");
    }
}
