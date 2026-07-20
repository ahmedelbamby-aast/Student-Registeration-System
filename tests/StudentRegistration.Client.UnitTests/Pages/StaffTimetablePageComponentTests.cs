using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffTimetablePageComponentTests
{
    [Fact]
    public void Stf_02_declares_equivalent_views_and_stale_recovery()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffTimetablePage.razor");
        RepositoryFiles.ContainsAll(source,
            "AuthenticatedPage", "WorkspaceKind.Staff", "UiDensity.Compact", "STF-02-COMP-STATE-EMPTY",
            "STF-02-COMP-STATE-SUCCESS", "STF-02-COMP-STATE-UNAUTHORIZED",
            "STF-02-COMP-STATE-STALE", "STF-02-COMP-STATE-SERVICE-ERROR",
            "Current authorized timetable calendar", "Chronological timetable list",
            "Refresh timetable");
    }
}
