using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_1Tests
{
    [Fact]
    public void Scoped_roster_is_assignment_authorized_bounded_and_minimal()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        RepositoryFiles.ContainsAll(
            source,
            "GetRosterAsync",
            "STAFF_GROUP_NOT_FOUND",
            "DefaultPageSize",
            "MaximumPageSize",
            "RosterSort",
            "RosterRowDto");
        Assert.DoesNotContain("Gpa", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Transcript", source, StringComparison.OrdinalIgnoreCase);
    }
}
