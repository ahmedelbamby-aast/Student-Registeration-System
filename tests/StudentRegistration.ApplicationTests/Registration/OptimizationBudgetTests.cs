using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.ApplicationTests.Registration;

public sealed class OptimizationBudgetTests
{
    [Fact]
    public async Task Zero_budget_returns_safe_time_budget_without_partial_options()
    {
        var result = await Coordinator().OptimizeAsync(
            [
                Candidate(1, 11, DayOfWeek.Monday, 9, 10),
                Candidate(2, 21, DayOfWeek.Tuesday, 9, 10)
            ],
            new SchedulePreferences(),
            requiredCredits: 6m,
            computationBudget: TimeSpan.Zero);

        Assert.Equal(OptimizationStatus.TimeBudget, result.Status);
        Assert.Empty(result.Options);
        Assert.All(
            result.Options,
            option => Assert.Equal(2, option.Selections.Count));
    }

    [Fact]
    public async Task Caller_cancellation_is_observed()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Coordinator().OptimizeAsync(
                [Candidate(1, 11, DayOfWeek.Monday, 9, 10)],
                new SchedulePreferences(),
                requiredCredits: 3m,
                computationBudget: TimeSpan.FromSeconds(1),
                cancellation.Token));
    }

    [Fact]
    public async Task No_viable_group_returns_one_single_member_minimal_diagnostic()
    {
        var result = await Coordinator().OptimizeAsync(
            [Candidate(1, 11, DayOfWeek.Monday, 9, 10, viable: false)],
            new SchedulePreferences(),
            requiredCredits: 3m,
            computationBudget: TimeSpan.FromSeconds(1));

        Assert.Equal(OptimizationStatus.NoSolution, result.Status);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("NO_VIABLE_GROUP", diagnostic.ReasonCode);
        Assert.Equal(OptimizationDiagnostic.InclusionMinimal, diagnostic.Minimality);
        var member = Assert.Single(diagnostic.Members);
        Assert.Equal(Id(1), member.CourseId);
        Assert.Equal("remove-course", member.Action);
    }

    [Fact]
    public async Task Unavoidable_overlap_returns_ordered_pair_with_intervals_and_actions()
    {
        var result = await Coordinator().OptimizeAsync(
            [
                Candidate(2, 21, DayOfWeek.Monday, 10, 12),
                Candidate(1, 11, DayOfWeek.Monday, 9, 11)
            ],
            new SchedulePreferences(),
            requiredCredits: 6m,
            computationBudget: TimeSpan.FromSeconds(1));

        Assert.Equal(OptimizationStatus.NoSolution, result.Status);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("MEETING_OVERLAP", diagnostic.ReasonCode);
        Assert.Equal([Id(1), Id(2)], diagnostic.Members.Select(x => x.CourseId));
        Assert.All(diagnostic.Members, member =>
        {
            Assert.NotNull(member.GroupId);
            Assert.NotNull(member.Interval);
            Assert.Equal("change-group", member.Action);
        });
        Assert.Equal(new TimeOnly(10, 0), diagnostic.ConflictingInterval!.StartLocal);
        Assert.Equal(new TimeOnly(11, 0), diagnostic.ConflictingInterval.EndLocal);
    }

    private static OptimizationCoordinator Coordinator()
    {
        var configuration = new OptimizerConfiguration(
            "1.0.0",
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            "SPEC-013-GATE-A-2026-07-13");
        return new OptimizationCoordinator(
            new ScheduleOptimizer(new ScheduleScorer(configuration)));
    }

    private static ScheduleCandidateGroup Candidate(
        int course,
        int group,
        DayOfWeek day,
        int startHour,
        int endHour,
        bool viable = true) =>
        new(
            Id(course),
            Id(course + 100),
            Id(group),
            3m,
            "published",
            viable,
            [
                new ScheduleCandidateMeeting(
                    day,
                    new TimeOnly(startHour, 0),
                    new TimeOnly(endHour, 0),
                    "MAIN")
            ]);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:D12}");
}
