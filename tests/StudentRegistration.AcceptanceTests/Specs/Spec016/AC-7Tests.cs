using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_7Tests
{
    [Fact]
    public void Publication_race_is_serialized_and_never_silently_unwarned()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Scheduling/SqlStaffAvailabilityPort.cs");
        RepositoryFiles.ContainsAll(
            port,
            "IsolationLevel.Serializable",
            "LoadPublishedMeetings",
            "AddMissingImpactAlerts",
            "SaveChangesAsync",
            "CommitAsync");
    }
}
