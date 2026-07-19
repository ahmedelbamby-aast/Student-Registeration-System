namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationResultPageComponentTests
{
    [Fact]
    public void Stu_06_covers_atomic_outcome_recovery_focus_and_retry_pending_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-06", "T174", "RegistrationResultPage",
            ["STU-06-COMP-STATE-LOADING", "STU-06-COMP-STATE-SUCCESS",
             "STU-06-COMP-STATE-VALIDATION-ERROR", "STU-06-COMP-STATE-SERVICE-ERROR",
             "STU-06-COMP-STATE-UNAUTHORIZED", "STU-06-COMP-STATE-STALE"],
            "NoPartialRegistration", "LookupRegistrationAsync", "disabled=\"@_retrying\"");
}
