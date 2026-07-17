using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationRecordsPageComponentTests
{
    [Fact]
    public void Stu_06_component_has_explicit_loading_success_rejection_denied_and_recovery_states()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/RegistrationResultPage.razor");
        RepositoryFiles.ContainsAll(source,
            "data-route-id=\"STU-06\"", "data-contract-state=\"@ContractState\"",
            "STU-06-COMP-STATE-LOADING", "STU-06-COMP-STATE-SUCCESS",
            "STU-06-COMP-STATE-VALIDATION-ERROR", "STU-06-COMP-STATE-UNAUTHORIZED",
            "STU-06-COMP-STATE-STALE", "STU-06-COMP-STATE-SERVICE-ERROR",
            "RegistrationPageSupport.ToMeetingItems(receipt.Groups, receipt.Term.TimeZoneId)",
            "Meetings=\"@_meetings\"", "window.print");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Stu_07_component_has_bounded_paging_empty_discovery_and_safe_retry_states()
    {
        var source = RepositoryFiles.Read("src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor");
        RepositoryFiles.ContainsAll(source,
            "data-route-id=\"STU-07\"", "STU-07-COMP-STATE-LOADING",
            "STU-07-COMP-STATE-SUCCESS", "STU-07-COMP-STATE-EMPTY",
            "STU-07-COMP-STATE-UNAUTHORIZED", "STU-07-COMP-STATE-SERVICE-ERROR",
            "RegistrationWindowState, \"open\"", "Math.Max(1", "Math.Abs(page - _history.PageNumber) <= 2",
            "Retry registration records", "Meetings=\"@_meetings\"");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
    }
}
