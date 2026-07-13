using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

[JsonConverter(typeof(IdempotencyKeyJsonConverter))]
public sealed record IdempotencyKey
{
    public IdempotencyKey(string value)
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

internal sealed class IdempotencyKeyJsonConverter : JsonConverter<IdempotencyKey>
{
    public override IdempotencyKey Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new JsonException("Idempotency key must be a JSON string.");
        }

        try
        {
            return new IdempotencyKey(reader.GetString()!);
        }
        catch (ArgumentException exception)
        {
            throw new JsonException("Idempotency key must be non-empty.", exception);
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        IdempotencyKey value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStringValue(value.Value);
    }
}
