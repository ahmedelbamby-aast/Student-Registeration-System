using StudentRegistration.Contracts.Staff;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec016;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public void Roster_projection_is_exactly_three_fields_and_audit_is_row_free()
    {
        Assert.Equal(
            ["DisplayName", "EnrollmentState", "UniversityId"],
            typeof(RosterRowDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
        var query = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        var adapter = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Staff/SqlStaffWorkspaceAdapter.cs");
        RepositoryFiles.ContainsAll(query, "WriteRosterAccessAsync", "rowCount", "correlationId");
        RepositoryFiles.ContainsAll(adapter, "staff-roster-access", "rowCount", "purpose");
        Assert.DoesNotContain("CurrentGpa", adapter, StringComparison.Ordinal);
        Assert.DoesNotContain("Transcript", adapter, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", adapter, StringComparison.Ordinal);
    }
}
