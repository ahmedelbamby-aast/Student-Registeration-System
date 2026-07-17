using StudentRegistration.Contracts.Staff;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec016;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Assignments_contract_is_staff_scoped_and_documents_all_safe_outcomes()
    {
        var contract = RepositoryFiles.Read("specs/016-lecturer-ta-workspace/contracts/api.md");

        Assert.Contains("### GET /api/staff/assignments", contract);
        Assert.Contains("Lecturer or TeachingAssistant", contract);
        Assert.Contains("Context.Read", contract);
        Assert.Contains("401 `UNAUTHORIZED`", contract);
        Assert.Contains("403 `FORBIDDEN`", contract);
        Assert.Contains("429 `RATE_LIMITED`", contract);
        Assert.Contains("500 `UNEXPECTED_ERROR`", contract);
        Assert.Equal(
            ["Group", "RosterCount", "StaffRole", "SubjectCode", "SubjectTitle"],
            typeof(StaffAssignmentDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }
}
