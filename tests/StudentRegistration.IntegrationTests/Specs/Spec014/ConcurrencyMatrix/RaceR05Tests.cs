namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR05Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Losing_group_rolls_all_allocations_back_to_savepoint_and_commits_replayable_rejection()
    {
        await new RaceSqlServerHarness(fixture).ProveAllocationSavepointAsync(new(
            "Allocation savepoint after claim/validation",
            "Valid whole plan",
            "Roll back seat mutations to savepoint; commit stable rejected result",
            "No partial enrollment/receipt; rejection replayable"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.SqlSeatAllocator",
            "AllocateAllOrRejectAsync");
    }
}
