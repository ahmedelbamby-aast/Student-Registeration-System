namespace StudentRegistration.IdentityAccess.Application;

public enum AuthenticationOutcome
{
    AuthenticationSucceeded,
    AuthenticationFailed,
    ActivationFailed,
    PasswordRejected,
    RecoveryAccepted,
    ChallengeInvalid,
    RoleNotAvailable
}

public sealed record AuthenticationResult(
    AuthenticationOutcome Outcome,
    Guid? UserId,
    string? DisplayName,
    IReadOnlyList<string> AuthorizedRoles,
    string? ActiveRole,
    DateTime? ExpiresAtUtc)
{
    public bool Succeeded => Outcome == AuthenticationOutcome.AuthenticationSucceeded;

    public bool RoleSelectionRequired =>
        Succeeded && AuthorizedRoles.Count > 1 && ActiveRole is null;

    public static AuthenticationResult AuthenticationFailed() =>
        Failure(AuthenticationOutcome.AuthenticationFailed);

    public static AuthenticationResult ActivationFailed() =>
        Failure(AuthenticationOutcome.ActivationFailed);

    public static AuthenticationResult Failure(AuthenticationOutcome outcome) =>
        new(outcome, null, null, [], null, null);

    public static AuthenticationResult Success(
        Guid userId,
        string displayName,
        IReadOnlyList<string> authorizedRoles,
        string? activeRole,
        DateTime expiresAtUtc) =>
        new(
            AuthenticationOutcome.AuthenticationSucceeded,
            userId,
            displayName,
            authorizedRoles,
            activeRole,
            expiresAtUtc);
}
