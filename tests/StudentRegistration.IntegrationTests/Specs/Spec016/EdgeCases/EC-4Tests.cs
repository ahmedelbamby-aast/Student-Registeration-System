using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec016.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void No_assignments_has_an_empty_state_without_broad_search()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffDashboardPage.razor");
        RepositoryFiles.ContainsAll(page, "STF-01-COMP-STATE-EMPTY", "No authorized assignments");
        Assert.DoesNotContain("Search all", page, StringComparison.OrdinalIgnoreCase);
    }
}
