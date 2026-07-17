using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_5Tests
{
    [Fact]
    public void Conflicting_change_persists_durable_alert_without_moving_schedule()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Scheduling/SqlStaffAvailabilityPort.cs");
        RepositoryFiles.ContainsAll(
            port,
            "AddMissingImpactAlerts",
            "ScheduleImpactAlert",
            "STAFF_UNAVAILABLE",
            "CommitAsync");
        Assert.DoesNotContain("MoveGroup", port, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateRoom", port, StringComparison.Ordinal);
    }
}
