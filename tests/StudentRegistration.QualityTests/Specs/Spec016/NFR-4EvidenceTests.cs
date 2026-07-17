using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec016;

public sealed class NFR_4EvidenceTests
{
    [Fact]
    public void Timetable_and_availability_have_keyboard_and_list_table_equivalence()
    {
        var timetable = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffTimetablePage.razor");
        var availability = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor");
        RepositoryFiles.ContainsAll(
            timetable,
            "ScheduleCalendar",
            "ScheduleList",
            "EquivalentListId",
            "EquivalentCalendarId",
            "Refresh timetable");
        RepositoryFiles.ContainsAll(
            availability,
            "ScheduleCalendar",
            "ScheduleList",
            "type=\"time\"",
            "Save availability");
    }
}
