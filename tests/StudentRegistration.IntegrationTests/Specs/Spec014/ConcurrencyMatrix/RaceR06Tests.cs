namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR06Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Profile_or_hold_edit_and_submit_serialize_on_the_academic_state_boundary()
    {
        await new RaceSqlServerHarness(fixture).ProveProfileEditSerializationAsync(new(
            "SPEC-008 StudentTermAcademicState row/version through ExecuteRegistrationBoundaryAsync",
            "First serialized transaction",
            "Re-read and reject or valid later commit",
            "Final profile governs"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "ExecuteRegistrationAsync");
    }
}
