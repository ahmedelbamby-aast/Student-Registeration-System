using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec013;

public sealed class NFR_3EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-013-NFR-3.md";

    [Fact]
    public async Task Deterministic_deadline_returns_safe_status_with_only_complete_options()
    {
        var timeProvider = new DeadlineStepTimeProvider(
            new DateTimeOffset(2026, 7, 17, 8, 0, 0, TimeSpan.Zero),
            expireAtRead: 5);
        var coordinator = new OptimizationCoordinator(
            Optimizer(),
            timeProvider);
        var courseOne = Id(1);
        var courseTwo = Id(2);
        var candidates = new[]
        {
            Group(courseOne, 11, DayOfWeek.Monday, 8),
            Group(courseTwo, 21, DayOfWeek.Tuesday, 8),
            Group(courseTwo, 22, DayOfWeek.Wednesday, 8)
        };

        var pending = coordinator.OptimizeAsync(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 6m,
            computationBudget: TimeSpan.FromSeconds(1));

        Assert.True(pending.IsCompletedSuccessfully);
        var result = await pending;
        Assert.Equal(OptimizationStatus.TimeBudget, result.Status);
        Assert.Empty(result.Diagnostics);
        var option = Assert.Single(result.Options);
        Assert.Equal(2, option.Selections.Count);
        Assert.Equal(
            new[] { courseOne, courseTwo },
            option.Selections.Select(selection => selection.CourseId).Order());
        Assert.Equal(2, option.Selections.Select(selection => selection.CourseId).Distinct().Count());
        Assert.Equal(5, timeProvider.ReadCount);
    }

    [Fact]
    public async Task Caller_cancellation_propagates_without_starting_background_work()
    {
        var timeProvider = new DeadlineStepTimeProvider(
            new DateTimeOffset(2026, 7, 17, 8, 0, 0, TimeSpan.Zero),
            expireAtRead: int.MaxValue);
        var coordinator = new OptimizationCoordinator(
            Optimizer(),
            timeProvider);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var pending = coordinator.OptimizeAsync(
            [Group(Id(1), 11, DayOfWeek.Monday, 8)],
            new SchedulePreferences(),
            requiredCredits: 3m,
            computationBudget: TimeSpan.FromSeconds(1),
            cancellationToken: cancellation.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.True(pending.IsCompleted);
        Assert.Equal(0, timeProvider.ReadCount);
    }

    [Fact]
    public void Coordinator_uses_the_injected_deadline_without_an_unbounded_task()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Application/OptimizationCoordinator.cs");

        RepositoryFiles.ContainsAll(
            source,
            "TimeProvider",
            "var deadline = _timeProvider.GetUtcNow().Add(computationBudget)",
            "() => _timeProvider.GetUtcNow() >= deadline",
            "OptimizationStatus.TimeBudget");
        Assert.DoesNotContain("Task.Run(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Evidence_records_the_bounded_runtime_proof_and_non_claims()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-013 NFR-3 Safe Time-Budget Evidence",
            "NFR-3",
            "injected deterministic",
            "`TimeProvider`",
            "time-budget",
            "complete option",
            "caller cancellation",
            "`Task.Run`",
            "not a wall-clock performance claim",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static ScheduleOptimizer Optimizer() =>
        new(new ScheduleScorer(new OptimizerConfiguration(
            "1.0.0",
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            "SPEC-013-GATE-A-2026-07-13")));

    private static ScheduleCandidateGroup Group(
        Guid courseId,
        int groupId,
        DayOfWeek day,
        int startHour) =>
        new(
            courseId,
            Id(1_000 + groupId),
            Id(groupId),
            3m,
            "published",
            viable: true,
            [new(day, new TimeOnly(startHour, 0), new TimeOnly(startHour + 1, 0), "R1")]);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private sealed class DeadlineStepTimeProvider(
        DateTimeOffset start,
        int expireAtRead) : TimeProvider
    {
        private int _readCount;

        public int ReadCount => Volatile.Read(ref _readCount);

        public override DateTimeOffset GetUtcNow() =>
            Interlocked.Increment(ref _readCount) >= expireAtRead
                ? start.AddSeconds(1)
                : start;
    }
}
