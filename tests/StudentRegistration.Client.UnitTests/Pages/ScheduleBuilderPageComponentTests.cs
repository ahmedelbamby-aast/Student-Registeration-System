namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class ScheduleBuilderPageComponentTests
{
    [Fact]
    public void Stu_04_covers_plan_states_conflict_actions_focus_and_pending_commands() =>
        Spec003RouteComponentAssertions.AssertRoute(
            "STU-04", "T164", "ScheduleBuilderPage",
            ["STU-04-COMP-STATE-LOADING", "STU-04-COMP-STATE-EMPTY", "STU-04-COMP-STATE-SUCCESS",
             "STU-04-COMP-STATE-VALIDATION-ERROR", "STU-04-COMP-STATE-SERVICE-ERROR",
             "STU-04-COMP-STATE-UNAUTHORIZED", "STU-04-COMP-STATE-SESSION-EXPIRED",
             "STU-04-COMP-STATE-STALE", "STU-04-COMP-STATE-OFFLINE"],
            "RemoveGroupAsync", "ApplyRecommendationAsync", "disabled=\"@(_plan.ReviewBlocked || _requestLoading)\"");
}
