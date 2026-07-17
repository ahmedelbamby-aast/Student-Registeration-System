namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR07Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Received_at_is_server_authoritative_at_the_scheduled_close_boundary()
    {
        await new RaceSqlServerHarness(fixture).ProveReceivedAtCutoffAsync(new(
            "Server ReceivedAtUtc",
            "Request received inside window",
            "WINDOW_CLOSED after cutoff",
            "Browser time ignored"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationCommandFactory",
            "Create");
    }
}
