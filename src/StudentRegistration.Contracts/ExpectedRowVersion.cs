using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

[JsonConverter(typeof(ExpectedRowVersionJsonConverter))]
public sealed record ExpectedRowVersion
{
    public ExpectedRowVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

internal sealed class ExpectedRowVersionJsonConverter : JsonConverter<ExpectedRowVersion>
{
    public override ExpectedRowVersion Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new JsonException("Expected row version must be a JSON string.");
        }

        try
        {
            return new ExpectedRowVersion(reader.GetString()!);
        }
        catch (ArgumentException exception)
        {
            throw new JsonException("Expected row version must be non-empty.", exception);
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        ExpectedRowVersion value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStringValue(value.Value);
    }
}
