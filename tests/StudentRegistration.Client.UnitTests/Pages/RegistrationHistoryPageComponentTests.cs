namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationHistoryPageComponentTests
{
    [Fact]
    public void Stu_07_covers_empty_history_current_timetable_paging_and_retry_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-07", "T179", "RegistrationHistoryPage",
            ["STU-07-COMP-STATE-LOADING", "STU-07-COMP-STATE-EMPTY",
             "STU-07-COMP-STATE-SUCCESS", "STU-07-COMP-STATE-SERVICE-ERROR",
             "STU-07-COMP-STATE-UNAUTHORIZED"],
            "ScheduleCalendar", "ScheduleList", "Pagination", "disabled=\"@_requesting\"");
}
