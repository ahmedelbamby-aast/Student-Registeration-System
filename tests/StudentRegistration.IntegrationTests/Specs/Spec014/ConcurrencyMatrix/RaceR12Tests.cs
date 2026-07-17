namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR12Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Transient_failure_retries_the_complete_idempotent_transaction_only()
    {
        await new RaceSqlServerHarness(fixture).ProveCompleteRetryAsync(new(
            "Stable lock order plus complete execution-strategy retry",
            "One complete transaction",
            "Safe idempotent retry",
            "Never retry a fragment"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "ExecuteWithCompleteRetryAsync");
    }
}
