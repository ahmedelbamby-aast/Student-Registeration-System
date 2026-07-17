namespace StudentRegistration.IntegrationTests.Specs.Spec014.ConcurrencyMatrix;

[Collection(RaceSqlServerCollection.CollectionName)]
public sealed class RaceR02Tests(RaceSqlServerFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Two_plans_for_one_student_term_serialize_before_group_locks()
    {
        await new RaceSqlServerHarness(fixture).ProveStudentTermSerializationAsync(new(
            "SPEC-008 StudentTermAcademicState via ExecuteRegistrationBoundaryAsync before groups",
            "First valid plan",
            "Re-read and 409 policy/conflict",
            "Combined enrollment remains valid"));
        RaceSqlServerHarness.RequireProductionCapability(
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "ExecuteRegistrationAsync");
    }
}
