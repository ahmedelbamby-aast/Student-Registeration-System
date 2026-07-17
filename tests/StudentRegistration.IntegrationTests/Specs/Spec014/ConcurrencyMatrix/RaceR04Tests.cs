namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR04Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Same_scoped_key_with_new_payload_is_rejected_while_another_term_is_independent()
    {
        await new RaceSqlServerHarness(fixture).ProvePayloadScopeAsync(new(
            "Unique (StudentId, TermId, ClientRequestId) claim plus payload comparison",
            "Original scoped payload",
            "409 IDEMPOTENCY_KEY_REUSED",
            "New payload never executes; same UUID in another term is independent"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ClaimOrObserveAsync");
    }
}
