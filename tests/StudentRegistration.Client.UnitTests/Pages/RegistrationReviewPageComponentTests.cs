namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationReviewPageComponentTests
{
    [Fact]
    public void Stu_05_covers_validation_dialog_focus_pending_and_single_submit_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-05", "T169", "RegistrationReviewPage",
            ["STU-05-COMP-STATE-LOADING", "STU-05-COMP-STATE-EMPTY", "STU-05-COMP-STATE-SUCCESS",
             "STU-05-COMP-STATE-VALIDATION-ERROR", "STU-05-COMP-STATE-SERVICE-ERROR",
             "STU-05-COMP-STATE-UNAUTHORIZED", "STU-05-COMP-STATE-SESSION-EXPIRED",
             "STU-05-COMP-STATE-STALE", "STU-05-COMP-STATE-OFFLINE"],
            "Confirm registration", "RestoreReviewFocusAsync", "_processing", "SubmitRegistrationAsync");
}
