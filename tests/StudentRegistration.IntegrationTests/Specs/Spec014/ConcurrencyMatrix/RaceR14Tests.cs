namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR14Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Lost_response_after_commit_replays_the_same_reference_receipt_and_success()
    {
        await new RaceSqlServerHarness(fixture).ProveLostResponseReplayAsync(new(
            "Stored submission/result/reference/receipt snapshot in same transaction",
            "Committed result",
            "Term-scoped result lookup/replay",
            "No duplicate allocation, reference, receipt, or audit-success event"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ReadFinalByRequestAsync");
    }
}
