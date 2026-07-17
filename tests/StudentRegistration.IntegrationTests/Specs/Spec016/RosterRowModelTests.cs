using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec016;

public sealed class RosterRowModelTests
{
    [Fact]
    public void Roster_contract_contains_exactly_the_three_approved_fields()
    {
        Assert.Equal(
            ["DisplayName", "EnrollmentState", "UniversityId"],
            typeof(RosterRowDto).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());

        var dto = new RosterRow("20260001", "Ada Lovelace", "active").ToDto();
        var json = JsonSerializer.Serialize(dto, JsonSerializerOptions.Web);

        Assert.Contains("universityId", json, StringComparison.Ordinal);
        Assert.Contains("displayName", json, StringComparison.Ordinal);
        Assert.Contains("enrollmentState", json, StringComparison.Ordinal);
        Assert.DoesNotContain("gpa", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hold", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("contact", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("grade", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("transcript", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Canonical_page_enforces_the_roster_bounds_and_sort_contract()
    {
        var item = new RosterRowDto("20260001", "Ada Lovelace", "active");
        var page = new Page<RosterRowDto>(
            [item],
            1,
            StaffWorkspaceContract.DefaultPageSize,
            1,
            StaffWorkspaceContract.RosterSort);

        Assert.Equal(20, page.PageSize);
        Assert.Equal("displayName:asc,universityId:asc", page.Sort);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Page<RosterRowDto>(
            [], 1, StaffWorkspaceContract.MaximumPageSize + 1, 0,
            StaffWorkspaceContract.RosterSort));
    }
}
