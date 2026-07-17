using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec013;

public sealed class NFR_4EvidenceTests
{
    private const int RequiredBranchCoveragePercent = 90;
    private const string EvidencePath =
        "docs/release-evidence/SPEC-013-NFR-4.md";

    [Fact]
    public void Validation_and_candidate_normalization_branches_are_exercised()
    {
        var optimizer = Optimizer();
        var preferences = new SchedulePreferences();
        var course = Id(1);
        var original = Group(course, 11, 3m, "published", viable: true);

        Assert.Throws<ArgumentNullException>(() =>
            optimizer.Optimize(null!, preferences, 3m));
        Assert.Throws<ArgumentNullException>(() =>
            optimizer.Optimize([original], null!, 3m));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            optimizer.Optimize([original], preferences, 0m));
        Assert.Throws<ArgumentException>(() =>
            optimizer.Optimize([original], preferences, 3m, [Guid.Empty]));
        Assert.Throws<ArgumentException>(() =>
            optimizer.Optimize([original], preferences, 3m, [course, course]));

        var duplicateResult = optimizer.Optimize(
            [original, original],
            preferences,
            3m);
        Assert.Single(duplicateResult.Options);

        var inconsistentDuplicates = new[]
        {
            Group(course, 11, 4m, "published", viable: true),
            Group(course, 11, 3m, "draft", viable: true),
            Group(course, 11, 3m, "published", viable: false),
            Group(
                course,
                11,
                3m,
                "published",
                viable: true,
                [Meeting(DayOfWeek.Tuesday, 10, 11)])
        };
        foreach (var inconsistent in inconsistentDuplicates)
        {
            Assert.Throws<ArgumentException>(() =>
                optimizer.Optimize(
                    [original, inconsistent],
                    preferences,
                    3m));
        }
    }

    [Fact]
    public void Empty_missing_filtered_budget_and_cancellation_branches_are_exercised()
    {
        var optimizer = Optimizer();
        var preferences = new SchedulePreferences();
        var requiredCourse = Id(1);
        var unrelatedCourse = Id(2);

        var empty = optimizer.Optimize([], preferences, 3m);
        Assert.Empty(empty.Options);
        Assert.Equal(0, empty.VisitedNodes);

        var missing = optimizer.Optimize(
            [
                Group(requiredCourse, 11, 3m, "draft", viable: true),
                Group(requiredCourse, 12, 3m, "published", viable: false),
                Group(unrelatedCourse, 21, 3m, "published", viable: true)
            ],
            preferences,
            3m,
            [requiredCourse]);
        Assert.Empty(missing.Options);
        Assert.Equal(0, missing.VisitedNodes);

        var expired = optimizer.Optimize(
            [Group(requiredCourse, 11, 3m, "published", viable: true)],
            preferences,
            3m,
            [requiredCourse],
            CancellationToken.None,
            budgetExpired: () => true);
        Assert.True(expired.SearchLimitReached);
        Assert.Equal(0, expired.VisitedNodes);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() =>
            optimizer.Optimize(
                [Group(requiredCourse, 11, 3m, "published", viable: true)],
                preferences,
                3m,
                [requiredCourse],
                cancellation.Token,
                budgetExpired: () => false));
    }

    [Fact]
    public void Search_prunes_over_credit_overlap_and_under_credit_leaf_branches()
    {
        var optimizer = Optimizer();
        var preferences = new SchedulePreferences();
        var courseOne = Id(1);
        var courseTwo = Id(2);

        var creditResult = optimizer.Optimize(
            [
                Group(courseOne, 11, 1m, "published", viable: true),
                Group(courseOne, 12, 4m, "published", viable: true)
            ],
            preferences,
            3m,
            [courseOne],
            CancellationToken.None,
            budgetExpired: () => false);
        Assert.Empty(creditResult.Options);
        Assert.True(creditResult.PrunedPartialSchedules >= 2);

        var overlapResult = optimizer.Optimize(
            [
                Group(
                    courseOne,
                    11,
                    3m,
                    "published",
                    viable: true,
                    [Meeting(DayOfWeek.Monday, 9, 11)]),
                Group(
                    courseTwo,
                    21,
                    3m,
                    "published",
                    viable: true,
                    [Meeting(DayOfWeek.Monday, 10, 12)]),
                Group(
                    courseTwo,
                    22,
                    3m,
                    "published",
                    viable: true,
                    [Meeting(DayOfWeek.Monday, 7, 8)]),
                Group(
                    courseTwo,
                    23,
                    3m,
                    "published",
                    viable: true,
                    [Meeting(DayOfWeek.Monday, 12, 13)]),
                Group(
                    courseTwo,
                    24,
                    3m,
                    "published",
                    viable: true,
                    [Meeting(DayOfWeek.Tuesday, 10, 12)])
            ],
            preferences,
            6m);
        Assert.Equal(3, overlapResult.Options.Count);
        Assert.True(overlapResult.PrunedPartialSchedules >= 1);
    }

    [Fact]
    public void Search_orders_constrained_courses_and_retains_only_three_best_options()
    {
        var optimizer = Optimizer();
        var constrained = Id(2);
        var flexible = Id(1);
        var candidates = new List<ScheduleCandidateGroup>
        {
            Group(
                constrained,
                21,
                3m,
                "published",
                viable: true,
                [Meeting(DayOfWeek.Tuesday, 9, 10)])
        };
        candidates.AddRange(Enumerable.Range(11, 4).Select(groupId =>
            Group(flexible, groupId, 3m, "published", viable: true)));

        var result = optimizer.Optimize(
            candidates,
            new SchedulePreferences(),
            6m);

        Assert.Equal(constrained, result.OrderedCourseIds[0]);
        Assert.Equal(3, result.Options.Count);
        Assert.False(result.SearchLimitReached);
        Assert.All(result.Options, option => Assert.Equal(2, option.Selections.Count));
    }

    [Fact]
    public void Search_limit_branch_stops_the_large_under_credit_tree()
    {
        var candidates = Enumerable.Range(1, 5)
            .SelectMany(course => Enumerable.Range(1, 10)
                .Select(group => Group(
                    Id(course),
                    course * 100 + group,
                    1m,
                    "published",
                    viable: true,
                    meetings: [])))
            .ToArray();

        var result = Optimizer().Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 100m);

        Assert.True(result.SearchLimitReached);
        Assert.Equal(100_000, result.VisitedNodes);
        Assert.Empty(result.Options);
        Assert.True(result.PrunedPartialSchedules > 0);
    }

    [Fact]
    public void Evidence_records_measured_coverage_and_the_scenario_boundary()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-013 NFR-4 Optimizer Branch-Coverage Evidence",
            "NFR-4",
            "ScheduleOptimizer",
            "Coverlet",
            "branch-rate",
            "scenario matrix",
            "validation and normalization",
            "immediate no-solution",
            "cooperative budget",
            "caller cancellation",
            "under-credit leaves",
            "over-credit pruning",
            "meeting-overlap pruning",
            "three-option retention",
            "100,000-node search limit",
            "0.9722",
            "97.22%",
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
        decimal credits,
        string state,
        bool viable,
        IReadOnlyList<ScheduleCandidateMeeting>? meetings = null) =>
        new(
            courseId,
            Id(1_000 + groupId),
            Id(groupId),
            credits,
            state,
            viable,
            meetings ?? [Meeting(DayOfWeek.Monday, 9, 10)]);

    private static ScheduleCandidateMeeting Meeting(
        DayOfWeek day,
        int startHour,
        int endHour) =>
        new(day, new TimeOnly(startHour, 0), new TimeOnly(endHour, 0), "R1");

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
