using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_7Tests
{
    private const string PanelPath =
        "src/StudentRegistration.Client/Features/Registration/ScheduleRecommendationsPanel.razor";
    private const string ApplicationPath =
        "src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs";

    [Fact]
    public void Client_suppresses_out_of_order_results_by_correlation_and_plan_version()
    {
        var panel = FutureSource(
            PanelPath,
            "The bounded STU-04 recommendation panel must exist before AC-7 can pass.");

        RepositoryFiles.ContainsAll(
            panel,
            "RequestCorrelationId",
            "PlanRowVersion");
        Assert.Contains(
            "current",
            panel,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Server_rejects_an_out_of_order_apply_against_the_current_plan_version()
    {
        var application = FutureSource(
            ApplicationPath,
            "FR-9 and FR-10 must be implemented before stale apply rejection can pass.");

        RepositoryFiles.ContainsAll(
            application,
            "RequestCorrelationId",
            "ExpectedPlanRowVersion",
            "PLAN_CHANGED",
            "ReplaceAsync");
    }

    private static string FutureSource(string path, string message)
    {
        Assert.True(RepositoryFiles.Exists(path), message);
        return RepositoryFiles.Read(path);
    }
}
