using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Course_with_zero_viable_groups_returns_immediate_no_solution()
    {
        var firstCourseId = Id(1);
        var blockedCourseId = Id(2);
        var optimizer = new ScheduleOptimizer(
            new ScheduleScorer(Configuration()));
        ScheduleCandidateGroup[] candidates =
        [
            Candidate(firstCourseId, Id(11), Id(21), viable: true),
            Candidate(blockedCourseId, Id(12), Id(22), viable: false)
        ];

        var result = optimizer.Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 6m);

        Assert.Empty(result.Options);
        Assert.Equal(blockedCourseId, result.OrderedCourseIds[0]);
        Assert.Equal(0, result.VisitedNodes);
        Assert.Equal(0, result.PrunedPartialSchedules);
        Assert.False(result.SearchLimitReached);
    }

    private static ScheduleCandidateGroup Candidate(
        Guid courseId,
        Guid offeringId,
        Guid groupId,
        bool viable) =>
        new(
            courseId,
            offeringId,
            groupId,
            credits: 3m,
            state: "published",
            viable,
            meetings: []);

    private static OptimizerConfiguration Configuration() =>
        new(
            "1.0.0",
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            "SPEC-013-GATE-A-2026-07-13");

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
