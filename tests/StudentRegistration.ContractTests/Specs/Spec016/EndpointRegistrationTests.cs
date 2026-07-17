using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec016;

public sealed class EndpointRegistrationTests
{
    [Fact]
    public void Canonical_handlers_map_exactly_the_five_staff_routes()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Endpoints/Spec016Endpoints.cs");
        RepositoryFiles.ContainsAll(
            source,
            "MapGet(\"/api/staff/assignments\"",
            "MapGet(\"/api/staff/timetable\"",
            "MapGet(\"/api/staff/groups/{groupId:guid}/roster\"",
            "MapGet(\"/api/staff/availability\"",
            "MapPut(\"/api/staff/availability\"",
            "Context.Read",
            "RequireAntiforgeryTokenAttribute",
            "AvailabilityConflictDto",
            "STAFF_GROUP_NOT_FOUND");
        Assert.DoesNotContain("/api/admin", source, StringComparison.OrdinalIgnoreCase);

        var composition = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/ModuleRegistration.cs");
        Assert.Equal(
            1,
            composition.Split("MapSpec016Endpoints", StringSplitOptions.None).Length - 1);
    }
}
