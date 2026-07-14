namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_4Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-018 T041-T044 must deliver SqlServerTestDatabaseFixture and NonProductionSeedLifecycle with compatibility/environment/connection-target guards, while SPEC-007 and SPEC-008 must deliver canonical identity/academic seed contributors; SPEC-005 is design-contract only and cannot execute this fault.";

    [Fact(Skip = ActivationGate)]
    public void Compatibility_or_environment_guard_failure_stops_before_seed_or_destructive_mutation()
    {
        throw new NotImplementedException(
            "No approved bootstrap/reset runtime exists to fault-inject without faking EC-4.");
    }
}
