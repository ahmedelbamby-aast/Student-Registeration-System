namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class EnvironmentDatabaseProvisioningTests
{
    private const string DeferredReason =
        "Deferred Docker/Testcontainers proof: activate the SPEC-004 DbContext, SPEC-007/008 seed contributors, DemoDatabaseInitializer, SqlServerTestDatabaseFixture, and composed S1/S2/S4/S6 chain at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Development_and_testing_provisioners_migrate_before_seed_enforce_environment_guards_and_apply_their_distinct_lifecycles()
    {
        // Given SQL Server 2022 Developer compatibility 160 in Docker Development and per-run Testcontainers Testing.
        // When each empty database is migrated, seeded twice, reset through its explicit guard, and torn down.
        // Then the deterministic synthetic seed is idempotent and hash-only, Testing is disposed, Development persists,
        // and every seed/reset request outside Development or Testing is rejected without mutation.
        throw new NotImplementedException(
            "Exercise the real provisioners and inspect SQL before/after lifecycle operations; provider substitutes and configuration-string checks are insufficient.");
    }
}
