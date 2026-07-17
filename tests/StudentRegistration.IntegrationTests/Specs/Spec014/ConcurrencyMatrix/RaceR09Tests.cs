namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR09Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Policy_publish_and_submit_choose_one_versioned_serial_order()
    {
        await new RaceSqlServerHarness(fixture).ProveVersionBoundaryAsync(
            new("Policy scope/version then submission re-read", "One valid serial order", "POLICY_CHANGED or commit under final version", "One policy version per decision"),
            "Policy scope/version then submission re-read",
            "One valid serial order",
            "POLICY_CHANGED or commit under final version",
            "POLICY_CHANGED",
            "One policy version per decision");
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "LockPolicyPublicationScopeAsync");
    }
}
