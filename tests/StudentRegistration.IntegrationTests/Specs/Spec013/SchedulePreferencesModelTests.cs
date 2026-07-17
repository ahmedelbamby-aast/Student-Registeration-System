using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013;

public sealed class SchedulePreferencesModelTests
{
    [Fact]
    public void Model_is_owned_by_the_registration_domain()
    {
        Assert.Equal(
            typeof(RegistrationPlan).Assembly,
            typeof(SchedulePreferences).Assembly);
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(SchedulePreferences).Namespace);
    }

    [Fact]
    public void Empty_preferences_have_no_avoided_days_or_time_bounds()
    {
        var preferences = new SchedulePreferences();

        Assert.Empty(preferences.AvoidedWeekdays);
        Assert.Null(preferences.EarliestPreferredStartLocal);
        Assert.Null(preferences.LatestPreferredEndLocal);
    }

    [Fact]
    public void Preferences_copy_and_canonically_order_unique_weekdays()
    {
        var avoidedWeekdays = new List<DayOfWeek>
        {
            DayOfWeek.Friday,
            DayOfWeek.Monday,
            DayOfWeek.Wednesday
        };

        var preferences = new SchedulePreferences(
            avoidedWeekdays,
            new TimeOnly(9, 0),
            new TimeOnly(16, 30));
        avoidedWeekdays.Clear();

        Assert.Equal(
            [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
            preferences.AvoidedWeekdays);
        Assert.Equal(
            new TimeOnly(9, 0),
            preferences.EarliestPreferredStartLocal);
        Assert.Equal(
            new TimeOnly(16, 30),
            preferences.LatestPreferredEndLocal);

        var mutableView = Assert.IsAssignableFrom<IList<DayOfWeek>>(
            preferences.AvoidedWeekdays);
        Assert.Throws<NotSupportedException>(() =>
            mutableView.Add(DayOfWeek.Sunday));
    }

    [Fact]
    public void Preferences_reject_duplicate_undefined_or_reversed_bounds()
    {
        Assert.Throws<ArgumentException>(() => new SchedulePreferences(
            [DayOfWeek.Monday, DayOfWeek.Monday]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SchedulePreferences(
            [(DayOfWeek)7]));
        Assert.Throws<ArgumentException>(() => new SchedulePreferences(
            earliestPreferredStartLocal: new TimeOnly(17, 0),
            latestPreferredEndLocal: new TimeOnly(8, 0)));
    }

    [Fact]
    public void Json_contract_uses_numeric_weekdays_and_round_trips()
    {
        var preferences = new SchedulePreferences(
            [DayOfWeek.Sunday, DayOfWeek.Thursday],
            new TimeOnly(8, 30),
            new TimeOnly(15, 45));

        var json = JsonSerializer.Serialize(
            preferences,
            JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            [
                "avoidedWeekdays",
                "earliestPreferredStartLocal",
                "latestPreferredEndLocal"
            ],
            root.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            [0, 4],
            root.GetProperty("avoidedWeekdays")
                .EnumerateArray()
                .Select(value => value.GetInt32()));

        var roundTrip = JsonSerializer.Deserialize<SchedulePreferences>(
            json,
            JsonSerializerOptions.Web);

        Assert.Equal(preferences, roundTrip);
    }
}
