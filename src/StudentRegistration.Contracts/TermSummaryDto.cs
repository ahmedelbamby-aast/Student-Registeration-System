using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<TermState>))]
public enum TermState
{
    [JsonStringEnumMemberName("draft")]
    Draft,

    [JsonStringEnumMemberName("registrationOpen")]
    RegistrationOpen,

    [JsonStringEnumMemberName("registrationClosed")]
    RegistrationClosed,

    [JsonStringEnumMemberName("teaching")]
    Teaching,

    [JsonStringEnumMemberName("completed")]
    Completed,

    [JsonStringEnumMemberName("archived")]
    Archived
}

public sealed record TermSummaryDto
{
    public TermSummaryDto(
        string id,
        string code,
        string label,
        TermState state,
        string rowVersion)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A declared term state is required.");
        }

        Id = Required(id, nameof(id));
        Code = Required(code, nameof(code));
        Label = Required(label, nameof(label));
        State = state;
        RowVersion = Required(rowVersion, nameof(rowVersion));
    }

    public string Id { get; }

    public string Code { get; }

    public string Label { get; }

    public TermState State { get; }

    public string RowVersion { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}
