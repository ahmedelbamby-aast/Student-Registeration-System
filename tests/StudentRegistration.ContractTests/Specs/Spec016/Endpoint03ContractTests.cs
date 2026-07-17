using StudentRegistration.Contracts.Staff;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec016;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Roster_contract_is_preauthorized_bounded_minimal_and_privacy_safe()
    {
        var contract = RepositoryFiles.Read("specs/016-lecturer-ta-workspace/contracts/api.md");

        Assert.Contains("### GET /api/staff/groups/{groupId}/roster", contract);
        Assert.Contains("Current-assignment authorization occurs before", contract);
        Assert.Contains("roster rows are queried", contract);
        Assert.Contains("default `20`", contract);
        Assert.Contains("maximum `100`", contract);
        Assert.Contains("displayName:asc,universityId:asc", contract);
        Assert.Contains("STAFF_GROUP_NOT_FOUND", contract);
        Assert.Contains("audit metadata contains no roster", contract);
        Assert.Contains("row value", contract);
        Assert.Equal(
            ["DisplayName", "EnrollmentState", "UniversityId"],
            typeof(RosterRowDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }
}
