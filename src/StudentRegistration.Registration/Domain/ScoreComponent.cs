using System.Text.Json.Serialization;

namespace StudentRegistration.Registration.Domain;

[JsonConverter(typeof(JsonStringEnumConverter<ScoreFactor>))]
public enum ScoreFactor
{
    [JsonStringEnumMemberName("preference-violations")]
    PreferenceViolations,

    [JsonStringEnumMemberName("idle-minutes")]
    IdleMinutes,

    [JsonStringEnumMemberName("teaching-days")]
    TeachingDays,

    [JsonStringEnumMemberName("stable-group-tuple")]
    StableGroupTuple
}

public sealed record ScoreComponent
{
    public ScoreComponent(
        ScoreFactor factor,
        string value,
        string message)
    {
        if (!Enum.IsDefined(factor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(factor),
                factor,
                "A canonical score factor is required.");
        }

        Factor = factor;
        Value = Required(value, nameof(value));
        Message = Required(message, nameof(message));
    }

    public ScoreFactor Factor { get; }

    public string Value { get; }

    public string Message { get; }

    private static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException(
                "A non-empty display value is required.",
                parameterName)
            : normalized;
    }
}
