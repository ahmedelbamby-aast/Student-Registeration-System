using System.Diagnostics;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;
using Xunit.Abstractions;

namespace StudentRegistration.QualityTests.Specs.Spec013;

public sealed class NFR_1EvidenceTests(ITestOutputHelper output)
{
    private const int MeasurementCount = 50;
    private const double MaximumP95Milliseconds = 500d;

    [Fact]
    public void Approved_eight_course_ten_group_fixture_meets_the_p95_gate()
    {
        var candidates = Spec013OptimizerEvidenceFixture.Candidates();
        var optimizer = Spec013OptimizerEvidenceFixture.Optimizer();

        Assert.Equal(80, candidates.Count);
        Assert.All(
            candidates.GroupBy(candidate => candidate.CourseId),
            course => Assert.Equal(10, course.Count()));

        for (var warmup = 0; warmup < 10; warmup++)
        {
            AssertComplete(optimizer.Optimize(
                candidates,
                Spec013OptimizerEvidenceFixture.Preferences,
                Spec013OptimizerEvidenceFixture.RequiredCredits));
        }

        var latencies = new double[MeasurementCount];
        ScheduleOptimizationResult? lastResult = null;
        for (var index = 0; index < latencies.Length; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            lastResult = optimizer.Optimize(
                candidates,
                Spec013OptimizerEvidenceFixture.Preferences,
                Spec013OptimizerEvidenceFixture.RequiredCredits);
            stopwatch.Stop();

            AssertComplete(lastResult);
            latencies[index] = stopwatch.Elapsed.TotalMilliseconds;
        }

        Array.Sort(latencies);
        var p95Index = (int)Math.Ceiling(latencies.Length * 0.95d) - 1;
        var p95Milliseconds = latencies[p95Index];

        output.WriteLine(
            "SPEC013_NFR001 courses=8 groups_per_course=10 candidates=80 " +
            "samples={0} p95_ms={1:F3} min_ms={2:F3} max_ms={3:F3} " +
            "visited={4} pruned={5} options={6} search_limit={7}",
            MeasurementCount,
            p95Milliseconds,
            latencies[0],
            latencies[^1],
            lastResult!.VisitedNodes,
            lastResult.PrunedPartialSchedules,
            lastResult.Options.Count,
            lastResult.SearchLimitReached);
        Assert.True(
            p95Milliseconds <= MaximumP95Milliseconds,
            $"Measured p95 was {p95Milliseconds:F3} ms; the limit is " +
            $"{MaximumP95Milliseconds:F0} ms.");
    }

    [Fact]
    public void Evidence_records_the_measured_optimizer_boundary()
    {
        var evidence = NormalizeWhitespace(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-013-NFR-1.md"));

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-013 NFR-1 Optimizer Performance Evidence",
            "8 selected courses",
            "10 published viable groups per course",
            "80 candidates",
            "50 measured runs",
            "p95 <= 500 ms",
            "ScheduleOptimizer.Optimize",
            "complete three-option result",
            "SearchLimitReached == false",
            "component-level",
            "approved load shape",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static void AssertComplete(ScheduleOptimizationResult result)
    {
        Assert.False(result.SearchLimitReached);
        Assert.Equal(8, result.OrderedCourseIds.Count);
        Assert.Equal(3, result.Options.Count);
        Assert.True(result.PrunedPartialSchedules > 0);
        Assert.All(
            result.Options,
            option => Assert.Equal(8, option.Selections.Count));
    }

    private static string NormalizeWhitespace(string value) =>
        string.Join(
            " ",
            value.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}

internal static class Spec013OptimizerEvidenceFixture
{
    private const int CourseCount = 8;
    private const int GroupsPerCourse = 10;

    private static readonly (DayOfWeek Day, int Hour)[] FeasibleSlots =
    [
        (DayOfWeek.Sunday, 8),
        (DayOfWeek.Sunday, 10),
        (DayOfWeek.Sunday, 12),
        (DayOfWeek.Sunday, 14),
        (DayOfWeek.Monday, 8),
        (DayOfWeek.Monday, 10),
        (DayOfWeek.Monday, 12),
        (DayOfWeek.Monday, 14)
    ];

    public const decimal RequiredCredits = 18m;

    public static SchedulePreferences Preferences { get; } =
        new(
            avoidedWeekdays: [DayOfWeek.Thursday],
            earliestPreferredStartLocal: new TimeOnly(8, 0),
            latestPreferredEndLocal: new TimeOnly(16, 0));

    public static ScheduleOptimizer Optimizer() =>
        new(new ScheduleScorer(new OptimizerConfiguration(
            "1.0.0",
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            "SPEC-013-GATE-A-2026-07-13")));

    public static IReadOnlyList<ScheduleCandidateGroup> Candidates() =>
        Enumerable.Range(0, CourseCount)
            .SelectMany(courseIndex =>
                Enumerable.Range(0, GroupsPerCourse)
                    .Select(groupIndex => Candidate(courseIndex, groupIndex)))
            .ToArray();

    private static ScheduleCandidateGroup Candidate(
        int courseIndex,
        int groupIndex)
    {
        // Nine alternatives for each later course overlap its predecessor's
        // feasible slot; the tenth advances to the next realistic timetable
        // slot. This exercises pruning while retaining ten complete schedules.
        var slotIndex = courseIndex == 0 || groupIndex == GroupsPerCourse - 1
            ? courseIndex
            : courseIndex - 1;
        var slot = FeasibleSlots[slotIndex];
        var start = new TimeOnly(slot.Hour, 0);
        var identity = courseIndex * GroupsPerCourse + groupIndex;

        return new ScheduleCandidateGroup(
            Id(100 + courseIndex),
            Id(1_000 + identity),
            Id(10_000 + identity),
            courseIndex < 4 ? 3m : 1.5m,
            "published",
            viable: true,
            [
                new ScheduleCandidateMeeting(
                    slot.Day,
                    start,
                    start.AddMinutes(75),
                    $"MAIN-{courseIndex + 1:00}")
            ]);
    }

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
