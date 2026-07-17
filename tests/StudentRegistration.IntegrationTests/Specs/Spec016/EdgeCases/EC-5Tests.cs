using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec016.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public void Deadline_that_passes_after_load_is_checked_inside_transaction()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Scheduling/SqlStaffAvailabilityPort.cs");
        var transaction = port.IndexOf("BeginTransactionAsync", StringComparison.Ordinal);
        var rules = port.IndexOf("StaffAvailabilityTransactionRules.Apply", StringComparison.Ordinal);
        Assert.True(transaction >= 0 && rules > transaction);
        Assert.Contains("TimeProvider", port, StringComparison.Ordinal);
    }
}
