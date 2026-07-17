using StudentRegistration.TestSupport;

namespace StudentRegistration.AccessibilityTests.Routes;

public sealed class StaffTimetablePageAccessibilityTests
{
    [Fact]
    public void Stf_02_has_information_equivalent_calendar_and_list_regions()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffTimetablePage.razor");
        RepositoryFiles.ContainsAll(page, "ScheduleCalendar", "ScheduleList",
            "EquivalentListId", "EquivalentCalendarId", "Meetings=\"@_meetings\"");
    }
}
