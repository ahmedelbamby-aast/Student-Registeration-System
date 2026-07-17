using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Equal_scores_use_stable_group_identifiers_regardless_of_input_order()
    {
        ScheduleCandidateGroup[] candidates =
        [
            Candidate(Id(2), Id(12), Id(22)),
            Candidate(Id(1), Id(11), Id(21)),
            Candidate(Id(2), Id(12), Id(23)),
            Candidate(Id(1), Id(11), Id(24))
        ];
        var optimizer = new ScheduleOptimizer(
            new ScheduleScorer(Configuration()));

        var first = optimizer.Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 6m);
        var second = optimizer.Optimize(
            candidates.Reverse().ToArray(),
            new SchedulePreferences(),
            requiredCredits: 6m);

        Assert.Equal(3, first.Options.Count);
        Assert.Equal(
            first.Options.Select(GroupTuple),
            second.Options.Select(GroupTuple));
        Assert.Equal(
            first.Options
                .Select(GroupTuple)
                .Order(StringComparer.Ordinal),
            first.Options.Select(GroupTuple));
    }

    private static string GroupTuple(ScheduleOption option) =>
        string.Join(
            "|",
            option.Selections
                .OrderBy(selection => selection.CourseId)
                .Select(selection => selection.GroupId.ToString("N")));

    private static ScheduleCandidateGroup Candidate(
        Guid courseId,
        Guid offeringId,
        Guid groupId) =>
        new(
            courseId,
            offeringId,
            groupId,
            credits: 3m,
            state: "published",
            viable: true,
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
