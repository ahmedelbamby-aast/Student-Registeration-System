using StudentRegistration.Registration.Domain;

namespace StudentRegistration.ApplicationTests.Registration;

public sealed class ScheduleScorerTests
{
    [Fact]
    public void Produces_the_approved_explained_lexicographic_components()
    {
        var schedule = new[]
        {
            Candidate(1, 11, DayOfWeek.Monday, 8, 9),
            Candidate(2, 21, DayOfWeek.Monday, 11, 12),
            Candidate(3, 31, DayOfWeek.Tuesday, 18, 19)
        };
        var preferences = new SchedulePreferences(
            [DayOfWeek.Tuesday],
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        var score = Scorer().Score(schedule, preferences);

        Assert.Equal(
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            score.Select(component => component.Factor));
        Assert.Equal(["3", "120", "2"], score.Take(3).Select(x => x.Value));
        Assert.All(score, component => Assert.False(string.IsNullOrWhiteSpace(component.Message)));
        Assert.DoesNotContain(
            typeof(ScheduleScorer).GetProperties(),
            property => property.Name.Contains("Weight", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Fewer_preference_violations_then_gaps_then_days_rank_first()
    {
        var scorer = Scorer();
        var preferences = new SchedulePreferences(
            [DayOfWeek.Friday],
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));
        var preferred = new[]
        {
            Candidate(1, 11, DayOfWeek.Monday, 9, 10),
            Candidate(2, 21, DayOfWeek.Monday, 10, 11)
        };
        var gapped = new[]
        {
            Candidate(1, 12, DayOfWeek.Monday, 9, 10),
            Candidate(2, 22, DayOfWeek.Monday, 12, 13)
        };
        var avoided = new[]
        {
            Candidate(1, 13, DayOfWeek.Friday, 9, 10),
            Candidate(2, 23, DayOfWeek.Friday, 10, 11)
        };

        Assert.True(scorer.Compare(preferred, gapped, preferences) < 0);
        Assert.True(scorer.Compare(gapped, avoided, preferences) < 0);
    }

    [Fact]
    public void Stable_group_tuple_breaks_an_exact_tie_deterministically()
    {
        var scorer = Scorer();
        var first = new[]
        {
            Candidate(1, 11, DayOfWeek.Monday, 9, 10),
            Candidate(2, 21, DayOfWeek.Tuesday, 9, 10)
        };
        var second = new[]
        {
            Candidate(1, 12, DayOfWeek.Monday, 9, 10),
            Candidate(2, 22, DayOfWeek.Tuesday, 9, 10)
        };

        Assert.True(scorer.Compare(first, second, new SchedulePreferences()) < 0);
        Assert.Equal(
            scorer.Score(first, new SchedulePreferences()),
            scorer.Score(first.Reverse().ToArray(), new SchedulePreferences()));
    }

    private static ScheduleScorer Scorer() =>
        new(
            new OptimizerConfiguration(
                "1.0.0",
                [
                    ScoreFactor.PreferenceViolations,
                    ScoreFactor.IdleMinutes,
                    ScoreFactor.TeachingDays,
                    ScoreFactor.StableGroupTuple
                ],
                "SPEC-013-GATE-A-2026-07-13"));

    private static ScheduleCandidateGroup Candidate(
        int course,
        int group,
        DayOfWeek day,
        int startHour,
        int endHour) =>
        new(
            Id(course),
            Id(course + 100),
            Id(group),
            3m,
            "published",
            true,
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
