namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR11Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Capacity_reduction_and_submit_preserve_count_within_capacity_in_either_order()
    {
        await new RaceSqlServerHarness(fixture).ProveCapacityReductionAsync(new(
            "Same SectionGroup row/version",
            "Either serial order",
            "Invalid reduction or GROUP_FULL",
            "0 <= count <= capacity"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.SqlSeatAllocator");
    }
}
