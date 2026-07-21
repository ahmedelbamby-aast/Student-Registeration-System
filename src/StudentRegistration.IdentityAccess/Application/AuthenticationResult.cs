namespace StudentRegistration.IdentityAccess.Application;

public enum AuthenticationOutcome
{
    AuthenticationSucceeded,
    AuthenticationFailed,
    ActivationFailed,
    PasswordRejected,
    RecoveryAccepted,
    ChallengeInvalid,
    InvalidRoleConfiguration
}

public sealed record AuthenticationResult(
    AuthenticationOutcome Outcome,
    Guid? UserId,
    string? DisplayName,
    string? SecurityStamp,
    IReadOnlyList<string> AuthorizedRoles,
    string? ActiveRole,
    DateTime? ExpiresAtUtc)
{
    public bool Succeeded => Outcome == AuthenticationOutcome.AuthenticationSucceeded;

    public static AuthenticationResult AuthenticationFailed() =>
        Failure(AuthenticationOutcome.AuthenticationFailed);

    public static AuthenticationResult ActivationFailed() =>
        Failure(AuthenticationOutcome.ActivationFailed);

    public static AuthenticationResult Failure(AuthenticationOutcome outcome) =>
        new(outcome, null, null, null, [], null, null);

    public static AuthenticationResult Success(
        Guid userId,
        string displayName,
        string securityStamp,
        IReadOnlyList<string> authorizedRoles,
        string? activeRole,
        DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);
        ArgumentNullException.ThrowIfNull(authorizedRoles);
        if (authorizedRoles.Count != 1 ||
            string.IsNullOrWhiteSpace(activeRole) ||
            !string.Equals(authorizedRoles[0], activeRole, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A successful session requires exactly one authorized role and that role must be active.",
                nameof(authorizedRoles));
        }

        return new(
            AuthenticationOutcome.AuthenticationSucceeded,
            userId,
            displayName,
            securityStamp,
            authorizedRoles.ToArray(),
            activeRole,
            expiresAtUtc);
    }
}
