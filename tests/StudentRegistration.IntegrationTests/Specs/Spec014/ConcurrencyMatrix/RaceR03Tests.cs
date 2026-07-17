namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR03Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Same_key_and_payload_execute_once_and_replay_or_return_bounded_processing_guidance()
    {
        await new RaceSqlServerHarness(fixture).ProveSamePayloadClaimAsync(new(
            "Atomic uncommitted claim plus 500-ms bounded wait",
            "First claimant",
            "Visible final replay or non-durable 202 with no submissionId",
            "One execution/final result"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ClaimOrObserveAsync");
    }
}
