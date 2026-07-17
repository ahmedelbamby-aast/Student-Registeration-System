namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR10Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Group_child_mutation_advances_the_owning_group_boundary_before_submit()
    {
        await new RaceSqlServerHarness(fixture).ProveVersionBoundaryAsync(
            new("Owning SectionGroup row/version advanced by every child mutation", "One valid serial order", "GROUP_CHANGED or valid enrollment", "No enrollment in invalid group state"),
            "Owning SectionGroup row/version advanced by every child mutation",
            "One valid serial order",
            "GROUP_CHANGED or valid enrollment",
            "GROUP_CHANGED",
            "No enrollment in invalid group state");
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "LockGroupVersionsAsync");
    }
}
