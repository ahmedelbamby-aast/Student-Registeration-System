namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class SubjectDiscoveryPageComponentTests
{
    [Fact]
    public void Stu_02_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        StudentPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("SubjectDiscoveryPage", "STU-02");

    [Fact]
    public void Stu_02_covers_state_filter_focus_retry_and_duplicate_action_rules() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-02", "T154", "SubjectDiscoveryPage",
            ["STU-02-COMP-STATE-LOADING", "STU-02-COMP-STATE-EMPTY", "STU-02-COMP-STATE-SUCCESS",
             "STU-02-COMP-STATE-VALIDATION-ERROR", "STU-02-COMP-STATE-SERVICE-ERROR",
             "STU-02-COMP-STATE-UNAUTHORIZED", "STU-02-COMP-STATE-SESSION-EXPIRED",
             "STU-02-COMP-STATE-STALE", "STU-02-COMP-STATE-OFFLINE"],
            "ApplyFiltersAsync", "ResetFiltersAsync", "Disabled=\"@_requestLoading\"");
}
