namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR01Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Two_students_contesting_the_final_seat_have_one_winner_and_group_full_loser()
    {
        await new RaceSqlServerHarness(fixture).ProveFinalSeatAsync(new(
            "SectionGroup conditional update",
            "First committed valid allocation",
            "409 GROUP_FULL",
            "EnrolledCount <= Capacity"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Infrastructure.SqlServer.Registration.SqlSeatAllocator");
    }
}
