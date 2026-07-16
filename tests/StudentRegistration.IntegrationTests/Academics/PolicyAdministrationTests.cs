using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class PolicyAdministrationTests
{
    [Fact]
    public void Demo_policy_contains_only_the_approved_typed_sourced_rules()
    {
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());

        Assert.Equal("DEMO-POC-2026.1", draft.PolicySet.VersionCode);
        Assert.Equal(PolicySetState.Draft, draft.PolicySet.State);
        Assert.Equal(
            [
                "ACADEMIC_STANDING_ALLOWED",
                "CAPACITY_REQUIRED",
                "CONFLICT_BLOCKED",
                "DS413_MIN_EARNED_CREDITS",
                "DS413_MIN_GPA",
                "NORMAL_MAX_CREDITS",
                "NORMAL_MIN_CREDITS",
                "NORMAL_RECOMMENDED_CREDITS",
                "PREREQUISITES_REQUIRED",
                "PROBATION_MAX_CREDITS",
                "REGISTRATION_WINDOW_OPEN",
            ],
            draft.Rules.Select(rule => rule.Code).Order().ToArray());
        Assert.DoesNotContain(
            draft.Rules,
            rule => rule.Code.Contains("WAITLIST", StringComparison.Ordinal)
                || rule.Code.Contains("OVERRIDE", StringComparison.Ordinal)
                || rule.Code.Contains("ADVISOR", StringComparison.Ordinal));
        Assert.All(draft.Rules, rule =>
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.SourceReference));
            Assert.True(Enum.IsDefined(rule.SourceKind));
            Assert.True(Enum.IsDefined(rule.ValueType));
        });
    }

    [Theory]
    [InlineData(3.00, 18, true, "NORMAL_MAX_CREDITS")]
    [InlineData(3.00, 19, false, "NORMAL_MAX_CREDITS")]
    [InlineData(1.99, 12, true, "PROBATION_MAX_CREDITS")]
    [InlineData(1.99, 13, false, "PROBATION_MAX_CREDITS")]
    public void Simulation_enforces_normal_and_probation_credit_boundaries(
        decimal gpa,
        int requestedCredits,
        bool eligible,
        string boundaryRule)
    {
        var service = new PolicyAdministrationService();
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());

        var result = service.Simulate(
            draft,
            Input(gpa, earnedCredits: 120m, requestedCredits));

        Assert.Equal(eligible, result.Eligible);
        var rule = Assert.Single(result.RuleResults, item => item.RuleCode == boundaryRule);
        Assert.Equal(eligible, rule.Passed);
        Assert.False(string.IsNullOrWhiteSpace(rule.SourceReference));
        Assert.True(Enum.IsDefined(rule.SourceKind));
    }

    [Theory]
    [InlineData(2.00, 95, false, "DS413_MIN_EARNED_CREDITS")]
    [InlineData(1.99, 96, false, "DS413_MIN_GPA")]
    [InlineData(2.00, 96, true, null)]
    public void Project_i_simulation_enforces_the_source_backed_gpa_and_credit_boundary(
        decimal gpa,
        decimal earnedCredits,
        bool eligible,
        string? failedRule)
    {
        var service = new PolicyAdministrationService();
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());

        var result = service.Simulate(
            draft,
            Input(gpa, earnedCredits, requestedCredits: 9, requestedCourses: ["DS413"]));

        Assert.Equal(eligible, result.Eligible);
        if (failedRule is not null)
        {
            Assert.Contains(
                result.RuleResults,
                item => item.RuleCode == failedRule && !item.Passed);
        }
        Assert.All(
            result.RuleResults.Where(item => item.RuleCode.StartsWith("DS413", StringComparison.Ordinal)),
            item => Assert.Equal(CatalogueSourceKind.OfficialSource, item.SourceKind));
    }

    [Fact]
    public void Same_policy_version_and_input_produce_the_same_ordered_explanation()
    {
        var service = new PolicyAdministrationService();
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());
        var input = Input(1.99m, 95m, 13, ["DS413"]);

        var first = service.Simulate(draft, input);
        var second = service.Simulate(draft, input);

        Assert.Equal(first.Eligible, second.Eligible);
        Assert.Equal(first.PolicyVersion, second.PolicyVersion);
        Assert.Equal(first.RuleResults, second.RuleResults);
        Assert.Equal(
            first.RuleResults.OrderBy(result => result.RuleCode).ToArray(),
            first.RuleResults);
    }

    [Fact]
    public void Validation_and_publication_require_complete_rules_current_lifecycle_and_permission()
    {
        var service = new PolicyAdministrationService();
        var draft = PolicyAdministrationService.CreateDemoPolicySet(Guid.NewGuid());

        var validation = service.Validate(draft);
        Assert.True(validation.IsValid);
        Assert.Equal(PolicySetState.Validated, draft.PolicySet.State);

        Assert.Equal(
            PolicyPublishOutcome.Unauthorized,
            service.Publish(draft, canPublish: false));
        Assert.Equal(PolicySetState.Validated, draft.PolicySet.State);
        Assert.Equal(
            PolicyPublishOutcome.Published,
            service.Publish(draft, canPublish: true));
        Assert.Equal(PolicySetState.Published, draft.PolicySet.State);
        Assert.Equal(
            PolicyPublishOutcome.InvalidLifecycle,
            service.Publish(draft, canPublish: true));
    }

    private static PolicySimulationInput Input(
        decimal gpa,
        decimal earnedCredits,
        int requestedCredits,
        IReadOnlyList<string>? requestedCourses = null) =>
        new(
            Gpa: gpa,
            EarnedCredits: earnedCredits,
            Standing: "Active",
            RegistrationWindowOpen: true,
            HasBlockingHold: false,
            RequestedCredits: requestedCredits,
            RequestedCourseCodes: requestedCourses ?? ["BA101", "BA113", "GN111"],
            CompletedCourseCodes: ["BA101", "GN111", "GN112", "DS413"],
            AllGroupsHaveCapacity: true,
            HasTimetableConflict: false);
}
