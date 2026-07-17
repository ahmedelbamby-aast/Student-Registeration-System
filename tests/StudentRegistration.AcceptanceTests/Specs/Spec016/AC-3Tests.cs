using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_3Tests
{
    [Fact]
    public void Deadline_is_revalidated_from_server_time_before_replacement()
    {
        var rules = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Application/StaffAvailabilityTransactionRules.cs");
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Scheduling/SqlStaffAvailabilityPort.cs");
        RepositoryFiles.ContainsAll(
            rules,
            "serverTimeUtc > current.DeadlineUtc",
            "AVAILABILITY_DEADLINE_PASSED");
        RepositoryFiles.ContainsAll(port, "TimeProvider", "RollbackAsync", "DeadlinePassed");
    }
}
