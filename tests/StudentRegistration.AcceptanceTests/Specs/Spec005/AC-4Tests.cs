namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_4Tests
{
    private const string DeferredReason =
        "Deferred composed-schema/bootstrap proof: activate the SPEC-004 DbContext, SPEC-007 identity, SPEC-008 academic, SPEC-009 catalogue, and SPEC-010 scheduling mappings plus S1/S2/S4/S6, DemoDatabaseInitializer, and SqlServerTestDatabaseFixture at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Migration_first_isolated_databases_enforce_the_erd_and_persist_only_deterministic_synthetic_hash_safe_seed_data()
    {
        // Given empty isolated Development and per-run Testing SQL Server 2022 Developer databases.
        // When compatibility 160 migrations run before the versioned synthetic seed and schema inspection.
        // Then rowversion/composite-FK invariants, deterministic provenance, unique University IDs, and hash-only credentials hold.
        // And Testing is disposed while Development persists until an explicit guarded reset.
        throw new NotImplementedException(
            "Run through Docker Development and Testcontainers Testing provisioners against the composed migrations; do not substitute an in-memory provider or static seed inspection.");
    }
}
