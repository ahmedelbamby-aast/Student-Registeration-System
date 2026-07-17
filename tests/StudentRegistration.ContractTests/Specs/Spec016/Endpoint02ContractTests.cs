using StudentRegistration.Contracts.Staff;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec016;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Timetable_contract_preserves_role_context_and_calendar_list_equivalence()
    {
        var contract = RepositoryFiles.Read("specs/016-lecturer-ta-workspace/contracts/api.md");

        Assert.Contains("### GET /api/staff/timetable", contract);
        Assert.Contains("StaffTimetableDto", contract);
        Assert.Contains("Calendar and chronological list/table", contract);
        Assert.Contains("401 `UNAUTHORIZED`", contract);
        Assert.Contains("403 `FORBIDDEN`", contract);
        Assert.Contains("429 `RATE_LIMITED`", contract);
        Assert.Contains("500 `UNEXPECTED_ERROR`", contract);
        Assert.Equal(["Assignments", "RoleContext"],
            typeof(StaffTimetableDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }
}
