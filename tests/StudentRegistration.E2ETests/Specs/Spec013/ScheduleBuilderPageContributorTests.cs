using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec013;

public sealed class ScheduleBuilderPageContributorTests
{
    private const string ContractPath =
        "specs/013-schedule-recommendations/contracts/routes/STU-04.md";
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor";
    private const string ClientPath =
        "src/StudentRegistration.Client/Features/Registration/RegistrationApiClient.cs";
    private const string PanelPath =
        "src/StudentRegistration.Client/Features/Registration/ScheduleRecommendationsPanel.razor";

    [Fact]
    public void Stu_04_contract_freezes_bounded_options_diagnostics_and_budget_states()
    {
        var contract = NormalizeWhitespace(RepositoryFiles.Read(ContractPath));

        RepositoryFiles.ContainsAll(
            contract,
            "spec013-stu04/1.0",
            "POST /api/student/terms/{termId}/registration-plan/recommendations",
            "PUT /api/student/terms/{termId}/registration-plan/recommended-option",
            "expectedPlanRowVersion",
            "requestCorrelationId",
            "Find schedule alternatives",
            "Finding schedule alternatives",
            "preference-violations",
            "idle-minutes",
            "teaching-days",
            "stable-group-tuple",
            "optimizerConfigurationVersion",
            "Apply option {rank}",
            "No complete schedule alternative was found",
            "inclusion-minimal",
            "change-group",
            "remove-course",
            "Recommendation time budget reached",
            "complete verified options",
            "never rendered as an option");
    }

    [Fact]
    public void Stu_04_contract_freezes_suppression_errors_authority_and_accessibility()
    {
        var contract = NormalizeWhitespace(RepositoryFiles.Read(ContractPath));

        RepositoryFiles.ContainsAll(
            NormalizeWhitespace(contract),
            "requestCorrelationId` equals the active request",
            "planRowVersion` equals the currently rendered plan",
            "silently discarded",
            "PLAN_CHANGED",
            "STALE_INPUT",
            "OPTION_EXPIRED",
            "RATE_LIMITED",
            "RECOMMENDATIONS_UNAVAILABLE",
            "Schedule alternatives",
            "polite live region",
            "assertive alert",
            "ordered list",
            "44-by-44 CSS-pixel",
            "320 CSS pixels",
            "400% zoom",
            "opaque",
            "never reserves capacity",
            "SPEC-014");
    }

    [Fact]
    public void Stu_04_contribution_preserves_spec012_page_ownership_and_scope()
    {
        var contract = RepositoryFiles.Read(ContractPath);

        Assert.Contains(
            "Canonical page owner:** SPEC-012",
            contract,
            StringComparison.Ordinal);
        Assert.Contains(
            "does not own or redesign",
            contract,
            StringComparison.Ordinal);
        Assert.Contains(
            "SPEC-012 remains the canonical STU-04 implementation owner",
            contract,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Canonical page owner:** SPEC-013",
            contract,
            StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(
            NormalizeWhitespace(contract),
            "no machine-learning ranking",
            "institution-wide solver",
            "travel-buffer guess",
            "seat reservation",
            "conflict override",
            "durable option store",
            "sticky-session dependency");
    }

    [Fact]
    public void Stu_04_owner_page_consumes_one_bounded_recommendation_panel()
    {
        Assert.True(
            RepositoryFiles.Exists(PanelPath),
            "T056 must deliver the bounded ScheduleRecommendationsPanel contributor.");
        var page = RepositoryFiles.Read(PagePath);
        var panel = RepositoryFiles.Read(PanelPath);

        RepositoryFiles.ContainsAll(
            page,
            "ScheduleRecommendationsPanel",
            "Find schedule alternatives",
            "RequestCorrelationId",
            "PlanRowVersion",
            "CancellationTokenSource");
        Assert.Equal(
            1,
            Count(page, "<ScheduleRecommendationsPanel"));
        RepositoryFiles.ContainsAll(
            panel,
            "Schedule alternatives",
            "Finding schedule alternatives",
            "No complete schedule alternative was found",
            "Recommendation time budget reached",
            "Apply option",
            "ScoreExplanation",
            "Diagnostics",
            "aria-live",
            "role=\"alert\"");
    }

    [Fact]
    public void Registration_client_and_page_bind_both_endpoints_and_suppress_late_results()
    {
        var client = RepositoryFiles.Read(ClientPath);
        var page = RepositoryFiles.Read(PagePath);

        RepositoryFiles.ContainsAll(
            client,
            "RecommendScheduleRequest",
            "ApplyScheduleOptionRequest",
            "OptimizationResultDto",
            "/recommendations",
            "/recommended-option");
        RepositoryFiles.ContainsAll(
            page,
            "RequestCorrelationId",
            "PlanRowVersion",
            "CancellationTokenSource",
            "Cancel()",
            "OptimizationResultDto",
            "PLAN_CHANGED",
            "STALE_INPUT",
            "OPTION_EXPIRED");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("optionToken=", page, StringComparison.OrdinalIgnoreCase);
    }

    private static int Count(string value, string fragment)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }

    private static string NormalizeWhitespace(string value) =>
        string.Join(
            " ",
            value.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
