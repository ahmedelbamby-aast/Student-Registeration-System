using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013;

public sealed class ScheduleOptionModelTests
{
    [Fact]
    public void Option_preserves_one_group_per_course_and_score_explanation_immutably()
    {
        var firstCourseId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var secondCourseId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var selections = new List<ScheduleOptionSelection>
        {
            Selection(
                secondCourseId,
                Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Guid.Parse("30000000-0000-0000-0000-000000000002")),
            Selection(
                firstCourseId,
                Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Guid.Parse("30000000-0000-0000-0000-000000000001"))
        };
        var explanation = ScoreExplanation().ToList();

        var option = new ScheduleOption(1, selections, explanation);
        selections.Clear();
        explanation.Clear();

        Assert.Equal(1, option.Rank);
        Assert.Equal(2, option.Selections.Count);
        Assert.Equal(firstCourseId, option.Selections[0].CourseId);
        Assert.Equal(secondCourseId, option.Selections[1].CourseId);
        Assert.Equal(4, option.ScoreExplanation.Count);
        Assert.Equal(
            ScoreFactor.PreferenceViolations,
            option.ScoreExplanation[0].Factor);
    }

    [Fact]
    public void Option_rejects_invalid_rank_incomplete_selection_or_duplicate_identity()
    {
        var selection = Selection(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScheduleOption(0, [selection], ScoreExplanation()));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScheduleOption(4, [selection], ScoreExplanation()));
        Assert.Throws<ArgumentException>(() =>
            new ScheduleOption(1, [], ScoreExplanation()));
        Assert.Throws<ArgumentException>(() =>
            new ScheduleOption(1, [selection], []));
        Assert.Throws<ArgumentException>(() =>
            new ScheduleOption(
                1,
                [
                    selection,
                    Selection(
                        selection.CourseId,
                        Guid.NewGuid(),
                        Guid.NewGuid())
                ],
                ScoreExplanation()));
        Assert.Throws<ArgumentException>(() =>
            new ScheduleOption(
                1,
                [
                    selection,
                    Selection(
                        Guid.NewGuid(),
                        selection.OfferingId,
                        Guid.NewGuid())
                ],
                ScoreExplanation()));
        Assert.Throws<ArgumentException>(() =>
            new ScheduleOption(
                1,
                [
                    selection,
                    Selection(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        selection.GroupId)
                ],
                ScoreExplanation()));
        Assert.Throws<ArgumentException>(() =>
            Selection(Guid.Empty, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Json_contract_is_bounded_and_round_trips_without_persistence_state()
    {
        var option = new ScheduleOption(
            2,
            [Selection(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())],
            ScoreExplanation());

        var json = JsonSerializer.Serialize(option, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            ["rank", "scoreExplanation", "selections"],
            document.RootElement
                .EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(2, document.RootElement.GetProperty("rank").GetInt32());
        Assert.Single(
            document.RootElement.GetProperty("selections").EnumerateArray());
        Assert.Equal(
            4,
            document.RootElement
                .GetProperty("scoreExplanation")
                .GetArrayLength());

        var roundTrip = JsonSerializer.Deserialize<ScheduleOption>(
            json,
            JsonSerializerOptions.Web);

        Assert.NotNull(roundTrip);
        Assert.Equal(option.Rank, roundTrip.Rank);
        Assert.Equal(option.Selections, roundTrip.Selections);
        Assert.Equal(option.ScoreExplanation, roundTrip.ScoreExplanation);
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(ScheduleOption).Namespace);
        Assert.DoesNotContain(
            typeof(ScheduleOption).GetCustomAttributes(inherit: false),
            attribute => attribute.GetType().Namespace?.Contains(
                "EntityFrameworkCore",
                StringComparison.Ordinal) is true);
    }

    private static ScheduleOptionSelection Selection(
        Guid courseId,
        Guid offeringId,
        Guid groupId) =>
        new(courseId, offeringId, groupId);

    private static IReadOnlyList<ScoreComponent> ScoreExplanation() =>
    [
        new(
            ScoreFactor.PreferenceViolations,
            "0",
            "No explicit preference violations."),
        new(ScoreFactor.IdleMinutes, "30", "Thirty idle minutes."),
        new(ScoreFactor.TeachingDays, "3", "Three teaching days."),
        new(
            ScoreFactor.StableGroupTuple,
            "G-01|G-02",
            "Stable group identifier tie-break.")
    ];
}
