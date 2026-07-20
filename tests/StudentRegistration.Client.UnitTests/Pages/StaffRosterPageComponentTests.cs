using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffRosterPageComponentTests
{
    [Fact]
    public void Stf_03_removes_roster_content_from_denied_and_stale_states()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffRosterPage.razor");
        RepositoryFiles.ContainsAll(source,
            "AuthenticatedPage", "WorkspaceKind.Staff", "UiDensity.Compact", "STF-03-COMP-STATE-EMPTY",
            "STF-03-COMP-STATE-SUCCESS", "STF-03-COMP-STATE-UNAUTHORIZED",
            "STF-03-COMP-STATE-STALE", "STF-03-COMP-STATE-SERVICE-ERROR",
            "DataTable", "Access denied", "Assignment changed", "Return to Staff home");
    }
}
