namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_6Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-018 T041-T044 must deliver .github/workflows/ci.yml, SqlServerTestDatabaseFixture, and NonProductionSeedLifecycle; canonical SPEC-007 identity and SPEC-008 academic seed contributors must be activated before migrations-before-seed and disposal can execute.";

    [Fact(Skip = ActivationGate)]
    public void Ci_runs_the_approved_gate_order_with_one_isolated_database_per_run()
    {
        // Given a behavior-changing pull request.
        // When CI runs its approved ordered gates.
        // Then each SQL run migrates, seeds, verifies, and disposes a unique Testing database.
        throw new NotImplementedException(
            "A future workflow declaration and real bootstrap fixture are required for AC-6.");
    }
}
