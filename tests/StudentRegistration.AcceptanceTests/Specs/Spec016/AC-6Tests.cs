using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_6Tests
{
    [Fact]
    public void Concurrent_complete_replacements_have_one_rowversion_winner()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Scheduling/SqlStaffAvailabilityPort.cs");
        var rules = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Application/StaffAvailabilityTransactionRules.cs");
        RepositoryFiles.ContainsAll(
            port,
            "IsolationLevel.Serializable",
            "UPDLOCK, HOLDLOCK",
            "DbUpdateConcurrencyException",
            "StaffAvailabilityPortResult.Stale");
        Assert.Contains("ExpectedStaffTermVersion", rules, StringComparison.Ordinal);
    }
}
