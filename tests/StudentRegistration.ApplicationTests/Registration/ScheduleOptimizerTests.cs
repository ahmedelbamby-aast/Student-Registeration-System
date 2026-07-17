using StudentRegistration.Registration.Domain;

namespace StudentRegistration.ApplicationTests.Registration;

public sealed class ScheduleOptimizerTests
{
    [Fact]
    public void Chooses_one_published_viable_group_per_course_and_prunes_overlaps()
    {
        var firstCourse = Id(1);
        var secondCourse = Id(2);
        var candidates = new[]
        {
            Candidate(firstCourse, 11, DayOfWeek.Monday, 9, 11),
            Candidate(firstCourse, 12, DayOfWeek.Tuesday, 9, 11),
            Candidate(secondCourse, 21, DayOfWeek.Monday, 10, 12),
            Candidate(secondCourse, 22, DayOfWeek.Wednesday, 10, 12),
            Candidate(secondCourse, 23, DayOfWeek.Thursday, 10, 12,
                state: "draft"),
            Candidate(secondCourse, 24, DayOfWeek.Friday, 10, 12,
                viable: false)
        };

        var result = Optimizer().Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 6m);

        Assert.NotEmpty(result.Options);
        Assert.All(
            result.Options,
            option =>
            {
                Assert.Equal(2, option.Selections.Count);
                Assert.Equal(
                    2,
                    option.Selections.Select(selection => selection.CourseId)
                        .Distinct()
                        .Count());
                Assert.DoesNotContain(
                    option.Selections,
                    selection => selection.GroupId == Id(23)
                        || selection.GroupId == Id(24));
            });
        Assert.DoesNotContain(
            result.Options,
            option => option.Selections.Select(selection => selection.GroupId)
                .ToHashSet()
                .IsSupersetOf([Id(11), Id(21)]));
        Assert.True(result.PrunedPartialSchedules > 0);
    }

    [Fact]
    public void Processes_most_constrained_course_first_and_returns_at_most_three()
    {
        var constrained = Id(2);
        var candidates = new[]
        {
            Candidate(Id(1), 11, DayOfWeek.Monday, 8, 9),
            Candidate(Id(1), 12, DayOfWeek.Tuesday, 8, 9),
            Candidate(Id(1), 13, DayOfWeek.Wednesday, 8, 9),
            Candidate(constrained, 21, DayOfWeek.Thursday, 8, 9)
        };

        var result = Optimizer().Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 6m);

        Assert.Equal(constrained, result.OrderedCourseIds[0]);
        Assert.InRange(result.Options.Count, 1, 3);
        Assert.Equal(
            result.Options.Count,
            result.Options.Select(OptionTuple).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Rejects_incomplete_or_wrong_credit_schedules_and_has_no_travel_rule()
    {
        var candidates = new[]
        {
            Candidate(Id(1), 11, DayOfWeek.Monday, 8, 9, location: "A"),
            Candidate(Id(2), 21, DayOfWeek.Monday, 9, 10, location: "B")
        };

        var result = Optimizer().Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 6m);
        var wrongCredit = Optimizer().Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 9m);

        Assert.Single(result.Options);
        Assert.Empty(wrongCredit.Options);
        Assert.DoesNotContain(
            typeof(ScheduleOptimizer).GetMembers(),
            member => member.Name.Contains("Travel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Same_inputs_return_the_same_stable_top_three()
    {
        var candidates = Enumerable.Range(1, 4)
            .SelectMany(course => Enumerable.Range(1, 3)
                .Select(group => Candidate(
                    Id(course),
                    course * 10 + group,
                    (DayOfWeek)((course + group) % 5 + 1),
                    8 + group,
                    9 + group,
                    credits: 4.5m)))
            .Reverse()
            .ToArray();

        var first = Optimizer().Optimize(
            candidates,
            new SchedulePreferences(),
            requiredCredits: 18m);
        var second = Optimizer().Optimize(
            candidates.Reverse().ToArray(),
            new SchedulePreferences(),
            requiredCredits: 18m);

        Assert.Equal(3, first.Options.Count);
        Assert.Equal(
            first.Options.Select(OptionTuple),
            second.Options.Select(OptionTuple));
    }

    private static ScheduleOptimizer Optimizer() =>
        new(new ScheduleScorer(Configuration()));

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

    private static ScheduleCandidateGroup Candidate(
        Guid courseId,
        int group,
        DayOfWeek day,
        int startHour,
        int endHour,
        string state = "published",
        bool viable = true,
        decimal credits = 3m,
        string location = "MAIN") =>
        new(
            courseId,
            Id(group + 100),
            Id(group),
            credits,
            state,
            viable,
            [
                new ScheduleCandidateMeeting(
                    day,
                    new TimeOnly(startHour, 0),
                    new TimeOnly(endHour, 0),
                    location)
            ]);

    private static string OptionTuple(ScheduleOption option) =>
        string.Join(
            "|",
            option.Selections.Select(selection => selection.GroupId.ToString("N")));

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:D12}");
}
