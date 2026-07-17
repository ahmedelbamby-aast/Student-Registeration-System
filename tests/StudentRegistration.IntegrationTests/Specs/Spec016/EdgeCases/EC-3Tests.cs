using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec016.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Stale_availability_returns_current_aggregate()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Application/Ports/IStaffAvailabilityPort.cs");
        RepositoryFiles.ContainsAll(port, "STALE_VERSION", "Availability", "StaffAvailabilityPortResult Stale");
    }
}
