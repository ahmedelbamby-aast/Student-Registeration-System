namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR13Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Process_death_before_commit_rolls_back_claim_for_the_second_replica()
    {
        await new RaceSqlServerHarness(fixture).ProveRolledBackClaimAsync(new(
            "Claim inside the registration SQL transaction",
            "Retried complete request",
            "Rolled-back claim can be reclaimed",
            "No orphan Processing record"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ClaimOrObserveAsync");
    }
}
