namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class ScheduleBuilderPageContractTests
{
    [Fact]
    public void Stu_04_freezes_authoritative_plan_conflict_and_recommendation_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "STU-04", "T163", "ScheduleBuilderPage", "/student/schedule",
            "GetRegistrationPlanAsync", "ReplaceRegistrationPlanAsync",
            "ValidateRegistrationPlanAsync", "RecommendScheduleAsync", "STALE_VERSION");
}
