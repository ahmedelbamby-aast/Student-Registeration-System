using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec016.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Dual_role_projection_deduplicates_group_entries()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        RepositoryFiles.ContainsAll(source, "GroupBy", "Group.Id", "StaffRole");
    }
}
