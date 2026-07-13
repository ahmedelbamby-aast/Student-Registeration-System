using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

public sealed record ApiError
{
    public ApiError(
        string code,
        string message,
        string correlationId,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors = null,
        string? currentVersion = null)
    {
        Code = Required(code, nameof(code));
        Message = Required(message, nameof(message));
        CorrelationId = Required(correlationId, nameof(correlationId));
        FieldErrors = CopyFieldErrors(fieldErrors);
        CurrentVersion = currentVersion is null
            ? null
            : Required(currentVersion, nameof(currentVersion));
    }

    public string Code { get; }

    public string Message { get; }

    public string CorrelationId { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? FieldErrors { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentVersion { get; }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>>? CopyFieldErrors(
        IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors)
    {
        if (fieldErrors is null)
        {
            return null;
        }

        var copy = new Dictionary<string, IReadOnlyList<string>>(
            fieldErrors.Count,
            StringComparer.Ordinal);
        foreach (var (field, messages) in fieldErrors)
        {
            var fieldName = Required(field, nameof(fieldErrors));
            if (messages is null || messages.Count == 0)
            {
                throw new ArgumentException(
                    "Each field error requires at least one message.",
                    nameof(fieldErrors));
            }

            var messageCopy = messages
                .Select((message, index) =>
                    Required(message, $"{nameof(fieldErrors)}[{fieldName}][{index}]"))
                .ToArray();
            copy.Add(fieldName, Array.AsReadOnly(messageCopy));
        }

        return new ReadOnlyDictionary<string, IReadOnlyList<string>>(copy);
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}
