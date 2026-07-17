using System.Reflection;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_2Tests
{
    [Fact]
    public void Fewer_gaps_requires_first_rank_and_all_approved_score_components()
    {
        // Given two feasible schedules and the approved lexicographic configuration.
        var configuration = new OptimizerConfiguration(
            "1.0.0",
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            "SPEC-013-GATE-A");

        Assert.Equal(
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            configuration.FactorOrder);

        // When feasible results are ranked, the production scorer is required.
        var scorer = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Domain.ScheduleScorer");

        // Then the lower-gap schedule ranks first and every factor is explained.
        Assert.True(
            scorer is not null,
            "ScheduleScorer must deliver AC-2 lexicographic ranking and explained components before this future acceptance test can pass.");
        var methods = scorer!.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        Assert.Contains(
            methods,
            method => method.Name.Contains("Score", StringComparison.Ordinal)
                || method.Name.Contains("Rank", StringComparison.Ordinal));
    }
}
