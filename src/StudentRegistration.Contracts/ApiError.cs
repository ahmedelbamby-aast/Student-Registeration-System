using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

public sealed record ApiError
{
    private const int MaximumFieldCount = 20;
    private const int MaximumMessagesPerField = 5;
    private const int MaximumFieldMessageLength = 256;

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

        if (fieldErrors.Count > MaximumFieldCount)
        {
            throw new ArgumentException(
                $"No more than {MaximumFieldCount} field errors are allowed.",
                nameof(fieldErrors));
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
            if (messages.Count > MaximumMessagesPerField)
            {
                throw new ArgumentException(
                    $"No more than {MaximumMessagesPerField} messages are allowed per field.",
                    nameof(fieldErrors));
            }

            var messageCopy = messages
                .Select((message, index) => RequiredBounded(
                    message,
                    $"{nameof(fieldErrors)}[{fieldName}][{index}]",
                    MaximumFieldMessageLength))
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

    private static string RequiredBounded(
        string value,
        string parameterName,
        int maximumLength)
    {
        var required = Required(value, parameterName);
        if (required.Length > maximumLength)
        {
            throw new ArgumentException(
                $"The value must not exceed {maximumLength} characters.",
                parameterName);
        }

        return required;
    }
}
