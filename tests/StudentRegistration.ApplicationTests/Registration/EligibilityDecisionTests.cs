using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.ApplicationTests.Registration;

public sealed class EligibilityDecisionTests
{
    [Fact]
    public async Task Fifteen_plus_three_is_eligible_at_normal_maximum()
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId);

        var offering = Assert.Single(result.Items);
        Assert.Equal(EligibilityEvaluationOutcome.Found, result.Outcome);
        Assert.True(offering.Eligible);
        Assert.Equal(15m, offering.CurrentPlanCredits);
        Assert.Equal(18m, offering.ProjectedPlanCredits);
        Assert.Equal(21m, offering.MaximumAllowedCredits);
        Assert.Contains(offering.Reasons, reason => reason.Code == "LOAD_ALLOWED" && reason.Passed);
        Assert.Equal(2, Assert.Single(offering.Groups).Meetings.Count);
    }

    [Theory]
    [InlineData(15, 3, 3.0, true, "LOAD_ALLOWED")]
    [InlineData(18, 3, 2.99, false, "LOAD_ABOVE_NORMAL_MAXIMUM")]
    [InlineData(18, 3, 3.0, true, "LOAD_ALLOWED")]
    [InlineData(21, 3, 3.0, false, "LOAD_ABOVE_NORMAL_MAXIMUM")]
    [InlineData(9, 3, 1.99, true, "PROBATION_LOAD_ALLOWED")]
    [InlineData(12, 3, 1.99, false, "PROBATION_LOAD_EXCEEDED")]
    public async Task Applicable_maximum_is_server_authoritative(
        decimal current,
        decimal credits,
        double gpa,
        bool eligible,
        string reasonCode)
    {
        var fixture = new Spec011ScenarioBuilder
        {
            CurrentPlanCredits = current,
            CourseCredits = credits,
            Gpa = (decimal)gpa,
            MinimumGpa = null,
            MinimumEarnedCredits = null
        };

        var offering = Assert.Single((await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);

        Assert.Equal(eligible, offering.Eligible);
        Assert.Contains(offering.Reasons, reason => reason.Code == reasonCode);
    }

    [Fact]
    public async Task Repeat_policy_is_fail_closed_for_a_passed_current_leaf()
    {
        var fixture = new Spec011ScenarioBuilder { CurrentCoursePassed = true };
        var offering = Assert.Single((await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);

        Assert.False(offering.Eligible);
        Assert.Contains(
            offering.Reasons,
            reason => reason.Code == "REPEAT_POLICY_UNAVAILABLE" && reason.Blocking);
    }

    [Fact]
    public async Task Missing_policy_fails_closed_with_aggregate_reason()
    {
        var fixture = new Spec011ScenarioBuilder { IncludePolicy = false };
        var offering = Assert.Single((await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);

        Assert.False(offering.Eligible);
        Assert.Contains(
            offering.Reasons,
            reason => reason.Code == "DECISION_DATA_UNAVAILABLE" && reason.Blocking);
    }

    [Fact]
    public async Task Governed_student_and_course_inputs_each_fail_with_their_exact_reason()
    {
        var scenarios = new (Spec011ScenarioBuilder Fixture, string ReasonCode)[]
        {
            (new() { WindowOpen = false }, "REGISTRATION_WINDOW_CLOSED"),
            (new() { Standing = "Suspended" }, "ACADEMIC_STANDING_UNAVAILABLE"),
            (new() { BlockingHold = true }, "REGISTRATION_HOLD"),
            (new() { PrerequisitePassed = false }, "PREREQUISITE_NOT_COMPLETED"),
            (new() { Gpa = 1.99m }, "MINIMUM_GPA_NOT_MET"),
            (new() { EarnedCredits = 95m }, "MINIMUM_EARNED_CREDITS_NOT_MET"),
            (new() { Capacity = 30, EnrolledCount = 30 }, "GROUP_FULL"),
            (new() { Capacity = 30, EnrolledCount = 29, HeldSeatCount = 1 }, "GROUP_FULL"),
        };

        foreach (var scenario in scenarios)
        {
            var offering = Assert.Single((await Service(scenario.Fixture)
                .EvaluateTermAsync(
                    scenario.Fixture.ApplicationUserId,
                    scenario.Fixture.TermId)).Items);

            Assert.False(offering.Eligible);
            Assert.Contains(
                offering.Reasons,
                reason => reason.Code == scenario.ReasonCode && reason.Blocking);
        }
    }

    [Fact]
    public async Task Conflict_is_group_specific_and_adjacency_does_not_overlap()
    {
        var fixture = new Spec011ScenarioBuilder { Conflict = true };
        var offering = Assert.Single((await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);
        var group = Assert.Single(offering.Groups);

        Assert.False(offering.Eligible);
        Assert.Contains(group.NonSelectableReasons, item => item.Code == "MEETING_CONFLICT");
        Assert.Contains(offering.Reasons, item => item.Code == "MEETING_CONFLICT");
    }

    [Fact]
    public async Task Replacing_a_selected_offering_does_not_double_count_or_self_conflict()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            CandidateOfferingAlreadySelected = true,
            CurrentPlanCredits = 15m
        };

        var offering = Assert.Single((await Service(fixture).EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);
        var group = Assert.Single(offering.Groups);

        Assert.Equal(15m, offering.CurrentPlanCredits);
        Assert.Equal(15m, offering.ProjectedPlanCredits);
        Assert.True(group.Selectable);
        Assert.DoesNotContain(
            group.NonSelectableReasons,
            reason => reason.Code == "MEETING_CONFLICT");
        Assert.Contains(
            offering.Reasons,
            reason => reason.Code == "NO_MEETING_CONFLICT" && reason.Passed);
    }

    [Fact]
    public async Task Versioned_empty_plan_is_the_live_default()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            CurrentPlanCredits = 0m,
            CurrentPlanVersion = "initial-empty/1",
            MinimumGpa = null,
            MinimumEarnedCredits = null
        };
        var service = new EligibilityService(
            fixture.AcademicReader(),
            fixture.OfferingReader(),
            new EmptyCurrentPlanReader(),
            new GroupSummaryProjection(),
            new FixedTimeProvider(fixture.EvaluatedAtUtc));

        var offering = Assert.Single((await service.EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);

        Assert.Equal(0m, offering.CurrentPlanCredits);
        Assert.Equal(3m, offering.ProjectedPlanCredits);
        Assert.Equal("initial-empty/1", offering.CurrentPlanVersion);
        Assert.True(offering.Eligible);
    }

    private static EligibilityService Service(Spec011ScenarioBuilder fixture) =>
        new(
            fixture.AcademicReader(),
            fixture.OfferingReader(),
            fixture.CurrentPlanReader(),
            new GroupSummaryProjection(),
            new FixedTimeProvider(fixture.EvaluatedAtUtc));

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
