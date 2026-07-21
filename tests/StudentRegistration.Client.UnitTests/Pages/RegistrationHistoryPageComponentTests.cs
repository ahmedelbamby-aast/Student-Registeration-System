namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationHistoryPageComponentTests
{
    [Fact]
    public void Stu_07_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        StudentPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("RegistrationHistoryPage", "STU-07");

    [Fact]
    public void Stu_07_covers_empty_history_current_timetable_paging_and_retry_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-07", "T179", "RegistrationHistoryPage",
            ["STU-07-COMP-STATE-LOADING", "STU-07-COMP-STATE-EMPTY",
             "STU-07-COMP-STATE-SUCCESS", "STU-07-COMP-STATE-SERVICE-ERROR",
             "STU-07-COMP-STATE-UNAUTHORIZED"],
            "ScheduleCalendar", "ScheduleList", "Pagination", "Disabled=\"@_requesting\"");

    [Fact]
    public void Stu_07_table_compact_list_and_calendar_list_pairs_preserve_information()
    {
        var source = StudentRegistration.TestSupport.RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor");
        StudentRegistration.TestSupport.RepositoryFiles.ContainsAll(source,
            "Registration records table", "Registration records compact view",
            "ScheduleCalendar", "ScheduleList", "record.Reference", "@record.Term.DisplayName",
            "record.SubmittedAtUtc", "@record.Status", "@record.GroupCount", "@record.TotalCredits");
        Assert.True(Occurrences(source, "@record.Status") >= 2);
        Assert.True(Occurrences(source, "@record.TotalCredits") >= 2);
    }

    private static int Occurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
