using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec016.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Removed_assignment_is_reauthorized_before_roster_query()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        Assert.Contains("ReadRosterIfAssignedAsync", source, StringComparison.Ordinal);
        Assert.Contains("STAFF_GROUP_NOT_FOUND", source, StringComparison.Ordinal);
    }
}
