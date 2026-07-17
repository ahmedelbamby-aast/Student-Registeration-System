using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec016;

public sealed class AC_8Tests
{
    [Fact]
    public void Workspace_quality_surfaces_scope_privacy_and_equivalent_views()
    {
        var roster = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs");
        var timetable = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffTimetablePage.razor");
        var availability = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor");
        RepositoryFiles.ContainsAll(roster, "ReadRosterIfAssignedAsync", "RosterRowDto", "WriteRosterAccessAsync");
        RepositoryFiles.ContainsAll(timetable, "calendar", "table");
        RepositoryFiles.ContainsAll(
            availability,
            "Availability range editor",
            "type=\"time\"",
            "Save availability");
    }
}
