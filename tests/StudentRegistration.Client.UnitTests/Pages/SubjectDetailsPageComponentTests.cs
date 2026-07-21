namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class SubjectDetailsPageComponentTests
{
    [Fact]
    public void Stu_03_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        StudentPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("SubjectDetailsPage", "STU-03");

    [Fact]
    public void Stu_03_covers_details_focus_refresh_and_safe_reason_states() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-03", "T159", "SubjectDetailsPage",
            ["STU-03-COMP-STATE-LOADING", "STU-03-COMP-STATE-SUCCESS",
             "STU-03-COMP-STATE-VALIDATION-ERROR", "STU-03-COMP-STATE-SERVICE-ERROR",
             "STU-03-COMP-STATE-UNAUTHORIZED", "STU-03-COMP-STATE-SESSION-EXPIRED",
             "STU-03-COMP-STATE-STALE", "STU-03-COMP-STATE-OFFLINE"],
            "LoadDetailsAsync", "Disabled=\"@_requestLoading\"", "Return to discovery");
}
