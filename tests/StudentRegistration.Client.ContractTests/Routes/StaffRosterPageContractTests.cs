using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StaffRosterPageContractTests
{
    [Fact]
    public void Stf_03_keeps_the_roster_bounded_and_minimal()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffRosterPage.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Staff/StaffApiClient.cs");

        RepositoryFiles.ContainsAll(page,
            "@page \"/staff/groups/{GroupId:guid}/roster\"", "data-route-id=\"STF-03\"",
            "GetRosterAsync(GroupId, page, PageSize)", "University ID", "Display name",
            "Enrollment state", "Pagination", "STAFF_GROUP_NOT_FOUND");
        RepositoryFiles.ContainsAll(client,
            "GetRosterAsync", "/api/staff/groups/{RequiredId(groupId",
            "int pageSize = 20", "pageSize={pageSize}");
        foreach (var forbidden in new[] { "GPA", "standing", "holds", "transcript", "contact" })
        {
            Assert.DoesNotContain(forbidden, page, StringComparison.OrdinalIgnoreCase);
        }
    }
}
