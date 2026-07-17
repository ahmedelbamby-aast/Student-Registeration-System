using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StaffAvailabilityPageContractTests
{
    [Fact]
    public void Stf_04_replaces_the_complete_owned_range_set_with_expected_version()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Staff/StaffApiClient.cs");

        RepositoryFiles.ContainsAll(page,
            "@page \"/staff/availability\"", "data-route-id=\"STF-04\"",
            "GetAvailabilityAsync", "ReplaceAvailabilityAsync",
            "ExpectedStaffTermRowVersion", "AVAILABILITY_DEADLINE_PASSED",
            "STALE_VERSION", "No class, room, or staff assignment moved automatically");
        RepositoryFiles.ContainsAll(client,
            "GetAvailabilityAsync", "ReplaceAvailabilityAsync", "HttpMethod.Put",
            "\"/api/staff/availability\"");
        Assert.DoesNotContain("/api/admin/", client, StringComparison.OrdinalIgnoreCase);
    }
}
