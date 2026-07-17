using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StaffTimetablePageContractTests
{
    [Fact]
    public void Stf_02_uses_one_authorized_timetable_source_for_both_views()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/StaffTimetablePage.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Staff/StaffApiClient.cs");

        RepositoryFiles.ContainsAll(page,
            "@page \"/staff/timetable\"", "data-route-id=\"STF-02\"",
            "GetTimetableAsync", "ScheduleCalendar", "ScheduleList",
            "Meetings=\"@_meetings\"", "No authorized timetable assignments");
        RepositoryFiles.ContainsAll(client,
            "GetTimetableAsync", "\"/api/staff/timetable\"");
    }
}
