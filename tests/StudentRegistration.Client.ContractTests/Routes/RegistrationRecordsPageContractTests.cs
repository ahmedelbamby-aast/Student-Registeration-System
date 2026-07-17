using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RegistrationRecordsPageContractTests
{
    [Fact]
    public void Stu_06_uses_the_frozen_route_shared_read_contracts_and_owned_components()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/RegistrationResultPage.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Registration/RegistrationApiClient.cs");
        var design = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/design/pages/STU-06.md");

        RepositoryFiles.ContainsAll(design, "\"routeTemplate\": \"/student/registration/result/{id}\"", "STU-06-accepted-v1", "STU-06-rejected-no-partial-result-v1");
        RepositoryFiles.ContainsAll(page, "@page \"/student/registration/result/{Id:guid}\"", "GetRegistrationDetailAsync(Id)", "ReceiptSummary", "ScheduleCalendar", "ScheduleList", "No subjects were partially registered.", "LookupRegistrationAsync(termId, requestId)", "STU-06-COMP-STATE-SUCCESS", "STU-06-COMP-STATE-VALIDATION-ERROR");
        RepositoryFiles.ContainsAll(client, "GetRegistrationDetailAsync", "/api/student/registrations/{RequiredId(submissionId", "LookupRegistrationAsync");
        Assert.DoesNotContain("Drop", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Correct", page, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Stu_07_uses_the_frozen_route_bounded_history_and_one_equivalent_schedule_source()
    {
        var page = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor");
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Registration/RegistrationApiClient.cs");
        var design = RepositoryFiles.Read("specs/003-ux-storyboard-accessibility/design/pages/STU-07.md");

        RepositoryFiles.ContainsAll(design, "\"routeTemplate\": \"/student/registrations\"", "STU-07-current-v1", "STU-07-history-v1", "STU-07-archived-v1");
        RepositoryFiles.ContainsAll(page, "@page \"/student/registrations\"", "GetRegistrationHistoryAsync(page, PageSize)", "GetCurrentRegistrationTimetableAsync", "Task.WhenAll", "ScheduleCalendar", "ScheduleList", "Current and archived registration outcomes", "Registration view", "record.TermState", "Pagination", "Find subjects");
        RepositoryFiles.ContainsAll(client, "GetRegistrationHistoryAsync", "GetCurrentRegistrationTimetableAsync", "/api/student/registrations/current/timetable");
        Assert.DoesNotContain("Drop", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Correction", page, StringComparison.OrdinalIgnoreCase);
    }
}
