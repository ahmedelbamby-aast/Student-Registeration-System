using System.Text.Json.Serialization;

namespace StudentRegistration.Client.Features.Frontend.Models;

[JsonConverter(typeof(JsonStringEnumConverter<UiStatusSeverity>))]
public enum UiStatusSeverity
{
    Information,
    Success,
    Warning,
    Error
}

public sealed class UiStatusAction
{
    public UiStatusAction(string actionId, string label, string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

        ActionId = actionId;
        Label = label;
        Target = target;
    }

    public string ActionId { get; }

    public string Label { get; }

    public string Target { get; }
}

/// <summary>
/// Privacy-safe status content already authorized for presentation to the user.
/// </summary>
public sealed class UiStatus
{
    public UiStatus(
        string code,
        string heading,
        string message,
        UiStatusSeverity severity,
        IReadOnlyList<UiStatusAction> nextActions,
        string? referenceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(nextActions);

        if (nextActions.Any(action => action is null))
        {
            throw new ArgumentException(
                "Status actions cannot contain null entries.",
                nameof(nextActions));
        }

        Code = code;
        Heading = heading;
        Message = message;
        Severity = severity;
        NextActions = nextActions.ToArray();
        ReferenceId = string.IsNullOrWhiteSpace(referenceId) ? null : referenceId;
    }

    public string Code { get; }

    public string Heading { get; }

    public string Message { get; }

    public UiStatusSeverity Severity { get; }

    public IReadOnlyList<UiStatusAction> NextActions { get; }

    public string? ReferenceId { get; }
}
