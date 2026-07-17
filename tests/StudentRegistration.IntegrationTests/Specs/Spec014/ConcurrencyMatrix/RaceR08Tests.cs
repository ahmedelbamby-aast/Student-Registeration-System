namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR08Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Emergency_close_committed_first_prevents_the_uncommitted_registration()
    {
        await new RaceSqlServerHarness(fixture).ProveVersionBoundaryAsync(
            new("Registration-context version boundary", "Emergency closure if committed first", "WINDOW_CHANGED", "No post-emergency uncommitted commit"),
            "Registration-context version boundary",
            "Emergency closure if committed first",
            "WINDOW_CHANGED",
            "WINDOW_CHANGED",
            "No post-emergency uncommitted commit");
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "LockRegistrationContextAsync");
    }
}
