using StudentRegistration.Client.Features.Frontend.Models;

namespace StudentRegistration.Client.UX;

public enum RouteUiState
{
    Loading,
    Empty,
    Success,
    ValidationError,
    ServiceError,
    Unauthorized,
    SessionExpired,
    Stale,
    Offline
}

public interface IUiTextProvider
{
    string Get(string key);
}

public sealed class UiStateInput
{
    public UiStateInput(
        RouteUiState state,
        bool serverAccepted,
        string? reasonCode,
        string? referenceId,
        IReadOnlyList<UiStatusAction> nextActions)
    {
        ArgumentNullException.ThrowIfNull(nextActions);

        if (nextActions.Any(action => action is null))
        {
            throw new ArgumentException(
                "Actions cannot contain null entries.",
                nameof(nextActions));
        }

        State = state;
        ServerAccepted = serverAccepted;
        ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode;
        ReferenceId = string.IsNullOrWhiteSpace(referenceId) ? null : referenceId;
        NextActions = nextActions.ToArray();
    }

    public RouteUiState State { get; }

    public bool ServerAccepted { get; }

    public string? ReasonCode { get; }

    public string? ReferenceId { get; }

    public IReadOnlyList<UiStatusAction> NextActions { get; }
}

public sealed record UiStateResult(
    RouteUiState State,
    bool ServerAccepted,
    UiStatus Status);

/// <summary>
/// Maps server-authoritative outcomes to presentation states without making a
/// business or authorization decision.
/// </summary>
public sealed class UiStateMapper
{
    private static readonly IReadOnlyDictionary<string, RouteUiState> ReasonStates =
        new Dictionary<string, RouteUiState>(StringComparer.Ordinal)
        {
            ["UNAUTHORIZED"] = RouteUiState.Unauthorized,
            ["FORBIDDEN"] = RouteUiState.Unauthorized,
            ["SESSION_EXPIRED"] = RouteUiState.SessionExpired,
            ["STALE_VERSION"] = RouteUiState.Stale,
            ["STALE_PREVIEW"] = RouteUiState.Stale,
            ["PLAN_CHANGED"] = RouteUiState.Stale,
            ["POLICY_CHANGED"] = RouteUiState.Stale,
            ["WINDOW_CHANGED"] = RouteUiState.Stale,
            ["WINDOW_CLOSED"] = RouteUiState.Stale,
            ["REGISTRATION_WINDOW_CLOSED"] = RouteUiState.Stale,
            ["GROUP_FULL"] = RouteUiState.Stale,
            ["RESOURCE_CONFLICT"] = RouteUiState.Stale,
            ["AVAILABILITY_DEADLINE_PASSED"] = RouteUiState.Stale,
            ["ACADEMIC_STANDING_UNAVAILABLE"] = RouteUiState.ValidationError,
            ["REGISTRATION_HOLD"] = RouteUiState.ValidationError,
            ["PROFILE_NOT_READY"] = RouteUiState.ValidationError,
            ["PREREQUISITE_NOT_COMPLETED"] = RouteUiState.ValidationError,
            ["LOAD_ABOVE_NORMAL_MAXIMUM"] = RouteUiState.ValidationError,
            ["PROBATION_LOAD_EXCEEDED"] = RouteUiState.ValidationError,
            ["REPEAT_POLICY_UNAVAILABLE"] = RouteUiState.ValidationError,
            ["MEETING_CONFLICT"] = RouteUiState.ValidationError,
            ["SCHEDULE_CONFLICT"] = RouteUiState.ValidationError,
            ["SERVICE_UNAVAILABLE"] = RouteUiState.ServiceError,
            ["MAINTENANCE"] = RouteUiState.ServiceError
        };

    private readonly IUiTextProvider _text;

    public UiStateMapper(IUiTextProvider text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
    }

    public UiStateResult Map(UiStateInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var state = input.State;
        var isUnknownReason = false;
        if (!input.ServerAccepted && input.ReasonCode is not null)
        {
            if (!ReasonStates.TryGetValue(input.ReasonCode, out state))
            {
                state = RouteUiState.ServiceError;
                isUnknownReason = true;
            }
        }
        else if (!input.ServerAccepted && input.State == RouteUiState.Success)
        {
            state = RouteUiState.ServiceError;
            isUnknownReason = true;
        }

        if (input.ServerAccepted
            && input.ReasonCode is not null
            && ReasonStates.TryGetValue(input.ReasonCode, out var blockingState)
            && blockingState is not RouteUiState.Success)
        {
            throw new ArgumentException(
                "An accepted result cannot contain a blocking reason code.",
                nameof(input));
        }

        var key = isUnknownReason
            ? "Ui.State.UnknownReason"
            : $"Ui.State.{state}";
        var status = new UiStatus(
            input.ReasonCode ?? StateCode(state),
            _text.Get($"{key}.Heading"),
            _text.Get($"{key}.Message"),
            SeverityFor(state),
            input.NextActions,
            input.ReferenceId);

        return new UiStateResult(state, input.ServerAccepted, status);
    }

    private static string StateCode(RouteUiState state) => state switch
    {
        RouteUiState.Loading => "UI_LOADING",
        RouteUiState.Empty => "UI_EMPTY",
        RouteUiState.Success => "UI_SUCCESS",
        RouteUiState.ValidationError => "UI_VALIDATION_ERROR",
        RouteUiState.ServiceError => "UI_SERVICE_ERROR",
        RouteUiState.Unauthorized => "UI_UNAUTHORIZED",
        RouteUiState.SessionExpired => "UI_SESSION_EXPIRED",
        RouteUiState.Stale => "UI_STALE",
        RouteUiState.Offline => "UI_OFFLINE",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    private static UiStatusSeverity SeverityFor(RouteUiState state) => state switch
    {
        RouteUiState.Success => UiStatusSeverity.Success,
        RouteUiState.ValidationError or RouteUiState.Stale or RouteUiState.Offline =>
            UiStatusSeverity.Warning,
        RouteUiState.ServiceError or RouteUiState.Unauthorized or RouteUiState.SessionExpired =>
            UiStatusSeverity.Error,
        _ => UiStatusSeverity.Information
    };
}
