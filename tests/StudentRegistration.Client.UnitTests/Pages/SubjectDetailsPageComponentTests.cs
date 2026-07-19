namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class SubjectDetailsPageComponentTests
{
    [Fact]
    public void Stu_03_covers_details_focus_refresh_and_safe_reason_states() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-03", "T159", "SubjectDetailsPage",
            ["STU-03-COMP-STATE-LOADING", "STU-03-COMP-STATE-SUCCESS",
             "STU-03-COMP-STATE-VALIDATION-ERROR", "STU-03-COMP-STATE-SERVICE-ERROR",
             "STU-03-COMP-STATE-UNAUTHORIZED", "STU-03-COMP-STATE-SESSION-EXPIRED",
             "STU-03-COMP-STATE-STALE", "STU-03-COMP-STATE-OFFLINE"],
            "LoadDetailsAsync", "disabled=\"@_requestLoading\"", "Return to discovery");
}
