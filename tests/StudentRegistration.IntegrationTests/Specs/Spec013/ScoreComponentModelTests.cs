using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013;

public sealed class ScoreComponentModelTests
{
    [Fact]
    public void Model_is_owned_by_the_registration_domain()
    {
        Assert.Equal(
            typeof(RegistrationPlan).Assembly,
            typeof(ScoreComponent).Assembly);
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(ScoreComponent).Namespace);
    }

    [Theory]
    [InlineData(ScoreFactor.PreferenceViolations, "preference-violations")]
    [InlineData(ScoreFactor.IdleMinutes, "idle-minutes")]
    [InlineData(ScoreFactor.TeachingDays, "teaching-days")]
    [InlineData(ScoreFactor.StableGroupTuple, "stable-group-tuple")]
    public void Component_preserves_and_serializes_the_closed_factor_contract(
        ScoreFactor factor,
        string expectedToken)
    {
        var component = new ScoreComponent(
            factor,
            "12",
            "The schedule has twelve idle minutes.");

        Assert.Equal(factor, component.Factor);
        Assert.Equal("12", component.Value);
        Assert.Equal(
            "The schedule has twelve idle minutes.",
            component.Message);

        var json = JsonSerializer.Serialize(component, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["factor", "message", "value"],
            root.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(expectedToken, root.GetProperty("factor").GetString());

        var roundTrip = JsonSerializer.Deserialize<ScoreComponent>(
            json,
            JsonSerializerOptions.Web);
        Assert.Equal(component, roundTrip);
    }

    [Fact]
    public void Component_accepts_a_stable_tuple_without_numeric_weights()
    {
        var component = new ScoreComponent(
            ScoreFactor.StableGroupTuple,
            "G-A|G-B|G-C",
            "Stable group identifiers break the final tie.");

        Assert.Equal("G-A|G-B|G-C", component.Value);
        Assert.Null(typeof(ScoreComponent).GetProperty("Weight"));
        Assert.Null(typeof(ScoreComponent).GetProperty("WeightedScore"));
    }

    [Fact]
    public void Component_rejects_undefined_factor_or_missing_display_content()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreComponent(
            (ScoreFactor)999,
            "1",
            "Invalid."));
        Assert.Throws<ArgumentException>(() => new ScoreComponent(
            ScoreFactor.IdleMinutes,
            " ",
            "Explanation."));
        Assert.Throws<ArgumentException>(() => new ScoreComponent(
            ScoreFactor.IdleMinutes,
            "1",
            " "));
    }
}
