namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR15Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Duplicate_repair_replays_and_counter_matches_active_enrollment_evidence_before_resume()
    {
        await new RaceSqlServerHarness(fixture).ProveReconciliationAsync(new(
            "SectionGroup lock/pause plus GroupId, observed rowversion and evidence-hash repair scope",
            "One authorized operations-service repair from active Enrollment evidence",
            "Duplicate replica replays; unauthorized/Admin invocation denied",
            "Counter equals active enrollment before audited resume"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.EnrollmentCounterReconciler",
            "RepairAsync");
    }
}
