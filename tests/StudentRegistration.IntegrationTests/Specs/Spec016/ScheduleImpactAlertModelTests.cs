using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec016;

public sealed class ScheduleImpactAlertModelTests
{
    [Fact]
    public void Workspace_consumes_the_canonical_durable_alert_lifecycle()
    {
        Assert.Equal("StudentRegistration.Scheduling", typeof(ScheduleImpactAlert).Assembly.GetName().Name);
        Assert.Equal(
            [ScheduleImpactAlertState.Open, ScheduleImpactAlertState.Revalidated, ScheduleImpactAlertState.Resolved],
            Enum.GetValues<ScheduleImpactAlertState>());
        Assert.Null(typeof(ScheduleImpactAlert).Assembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.ScheduleImpactAlert"));
    }
}
