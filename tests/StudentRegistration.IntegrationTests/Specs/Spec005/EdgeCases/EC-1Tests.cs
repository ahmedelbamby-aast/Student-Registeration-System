namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

public sealed class EC_1Tests
{
    private const string DeferredReason =
        "Deferred bootstrap fault proof: activate SPEC-004 DbContext composition, SPEC-007/008 seed contributors, DemoDatabaseInitializer, SqlServerTestDatabaseFixture, and S1/S2/S4/S6 at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Partial_bootstrap_never_marks_ready_and_non_development_or_testing_seed_reset_is_rejected_without_mutation()
    {
        // Given a fault injected between migration and seed, plus seed/reset requests targeting a forbidden environment.
        // When the real bootstrap and environment guards execute.
        // Then readiness remains false, partial work is not accepted, and forbidden requests make no database mutation.
        throw new NotImplementedException(
            "Inject faults into the real SQL bootstrap and compare database state; a mocked readiness flag cannot prove this boundary.");
    }
}
