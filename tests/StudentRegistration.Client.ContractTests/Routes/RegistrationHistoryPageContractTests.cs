namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RegistrationHistoryPageContractTests
{
    [Fact]
    public void Stu_07_freezes_owned_history_timetable_and_paging_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "STU-07", "T178", "RegistrationHistoryPage", "/student/registrations",
            "GetRegistrationHistoryAsync", "GetCurrentRegistrationTimetableAsync",
            "REGISTRATION_HISTORY_UNAVAILABLE", "TotalCount");
}
