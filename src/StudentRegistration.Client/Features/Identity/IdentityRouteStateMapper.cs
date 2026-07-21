using StudentRegistration.Client.UX;

namespace StudentRegistration.Client.Features.Identity;

/// <summary>
/// Maps server-returned identity states to the approved presentation states.
/// It does not grant a role or infer access from navigation state.
/// </summary>
public static class IdentityRouteStateMapper
{
    public const string StudentLoginRecord = "AUTH-02";
    public const string StudentActivationRecord = "AUTH-03";
    public const string StaffLoginRecord = "AUTH-04";
    public const string AccountRecoveryRecord = "AUTH-05";
    public const string StudentAccountRecord = "STU-08";

    private static readonly HashSet<string> ApprovedRecords =
        new(StringComparer.Ordinal)
        {
            StudentLoginRecord,
            StudentActivationRecord,
            StaffLoginRecord,
            AccountRecoveryRecord,
            StudentAccountRecord
        };

    public static IdentityRouteStateResult Map(
        string pageDesignRecordId,
        string serviceState,
        bool serverAccepted,
        string? reasonCode = null)
    {
        if (!ApprovedRecords.Contains(pageDesignRecordId))
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageDesignRecordId),
                pageDesignRecordId,
                "The identity page-design record is not approved.");
        }

        if (string.IsNullOrWhiteSpace(serviceState))
        {
            throw new ArgumentException("A service state is required.", nameof(serviceState));
        }

        var normalized = serviceState.Trim().ToLowerInvariant();
        if (!serverAccepted && normalized is "success")
        {
            return Result(
                pageDesignRecordId,
                RouteUiState.ServiceError,
                "service-error",
                reasonCode,
                "retry");
        }

        return normalized switch
        {
            "loading" => Result(
                pageDesignRecordId,
                RouteUiState.Loading,
                normalized,
                reasonCode,
                null),
            "success" => Result(
                pageDesignRecordId,
                RouteUiState.Success,
                normalized,
                reasonCode,
                null),
            "expired" or "session-expired" => Result(
                pageDesignRecordId,
                RouteUiState.SessionExpired,
                "expired",
                reasonCode,
                "reauthenticate"),
            "locked" => Result(
                pageDesignRecordId,
                RouteUiState.ValidationError,
                normalized,
                reasonCode,
                "recovery"),
            "validation" or "validation-error" or "password-failure" => Result(
                pageDesignRecordId,
                RouteUiState.ValidationError,
                "validation",
                reasonCode,
                "correct-and-retry"),
            "rate-limited" => Result(
                pageDesignRecordId,
                RouteUiState.ValidationError,
                normalized,
                reasonCode,
                "wait-and-retry"),
            "unauthorized" => Result(
                pageDesignRecordId,
                RouteUiState.Unauthorized,
                normalized,
                reasonCode,
                "sign-in"),
            "stale" => Result(
                pageDesignRecordId,
                RouteUiState.Stale,
                normalized,
                reasonCode,
                "refresh-and-review"),
            "offline" => Result(
                pageDesignRecordId,
                RouteUiState.Offline,
                normalized,
                reasonCode,
                "retry"),
            "service-error" => Result(
                pageDesignRecordId,
                RouteUiState.ServiceError,
                normalized,
                reasonCode,
                "retry"),
            _ => Result(
                pageDesignRecordId,
                RouteUiState.ServiceError,
                "service-error",
                reasonCode,
                "retry")
        };
    }

    private static IdentityRouteStateResult Result(
        string pageDesignRecordId,
        RouteUiState uiState,
        string serviceState,
        string? reasonCode,
        string? nextAction) =>
        new(
            pageDesignRecordId,
            uiState,
            serviceState,
            string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode,
            nextAction);
}

public sealed record IdentityRouteStateResult(
    string PageDesignRecordId,
    RouteUiState UiState,
    string ServiceState,
    string? ReasonCode,
    string? NextAction);
