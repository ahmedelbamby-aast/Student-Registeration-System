namespace StudentRegistration.Client.Features.Identity;

public static class IdentityPageFeedback
{
    public static IdentityRouteStateResult MapFailure(
        string pageDesignRecordId,
        string? code) =>
        IdentityRouteStateMapper.Map(
            pageDesignRecordId,
            StateFor(code),
            serverAccepted: false,
            code);

    public static string StateFor(string? code) => code switch
    {
        "RATE_LIMITED" => "rate-limited",
        "SESSION_EXPIRED" => "session-expired",
        "UNAUTHORIZED" or "FORBIDDEN" => "unauthorized",
        "STALE_VERSION" => "stale",
        "AUTHENTICATION_FAILED" or
        "ACTIVATION_FAILED" or
        "CHALLENGE_INVALID" or
        "CURRENT_PASSWORD_INVALID" or
        "PASSWORD_REJECTED" or
        "ROLE_NOT_AVAILABLE" or
        "FINAL_ADMIN_REQUIRED" or
        "IMPORT_INVALID" or
        "IMPORT_NOT_VALIDATED" or
        "VALIDATION_FAILED" => "validation-error",
        _ => "service-error"
    };

    public static string SafeMessageFor(string? code) => code switch
    {
        "RATE_LIMITED" => "Too many attempts were received. Wait, then try again.",
        "SESSION_EXPIRED" or "UNAUTHORIZED" =>
            "Your session is no longer available. Sign in again.",
        "STALE_VERSION" => "The account changed. Refresh and review before trying again.",
        "PASSWORD_REJECTED" =>
            "Choose a different password that meets the stated requirements.",
        "ROLE_NOT_AVAILABLE" => "That role is no longer available for this account.",
        "CURRENT_PASSWORD_INVALID" => "The password change could not be accepted.",
        "CHALLENGE_INVALID" => "The recovery request could not be accepted. Request a new one.",
        "ACTIVATION_FAILED" =>
            "Activation could not be completed. Review the details or use account recovery.",
        "AUTHENTICATION_FAILED" => "The sign-in details could not be accepted.",
        "VALIDATION_FAILED" => "Review the highlighted information and try again.",
        _ => "The service is temporarily unavailable. Try again or use the support link."
    };
}
