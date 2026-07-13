namespace StudentRegistration.Client.Features.Frontend.Models;

/// <summary>
/// Presentation-only projection of the authoritative server application context.
/// </summary>
public sealed class FrontendAppContextView
{
    public FrontendAppContextView(
        DateTimeOffset serverDateTime,
        string timeZone,
        string teachingTerm,
        string registrationTerm,
        DateTimeOffset registrationWindowOpensAt,
        DateTimeOffset registrationWindowClosesAt,
        string displayName,
        IReadOnlyList<string> authorizedRoles,
        string? activeRole,
        bool roleSelectionRequired,
        string sessionState,
        DateTimeOffset sessionExpiresAt,
        string serviceState,
        string supportReferencePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZone);
        ArgumentException.ThrowIfNullOrWhiteSpace(teachingTerm);
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationTerm);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(authorizedRoles);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionState);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceState);
        ArgumentException.ThrowIfNullOrWhiteSpace(supportReferencePath);

        if (registrationWindowClosesAt <= registrationWindowOpensAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(registrationWindowClosesAt),
                "The registration window must close after it opens.");
        }

        if (authorizedRoles.Count == 0 || authorizedRoles.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "At least one nonblank authorized role is required.",
                nameof(authorizedRoles));
        }

        if (!roleSelectionRequired && string.IsNullOrWhiteSpace(activeRole))
        {
            throw new ArgumentException(
                "An active role is required unless the server requires role selection.",
                nameof(activeRole));
        }

        if (!string.IsNullOrWhiteSpace(activeRole)
            && !authorizedRoles.Contains(activeRole, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The active role must be one of the server-authorized roles.",
                nameof(activeRole));
        }

        if (!supportReferencePath.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The support reference must be an application-relative path.",
                nameof(supportReferencePath));
        }

        ServerDateTime = serverDateTime;
        TimeZone = timeZone;
        TeachingTerm = teachingTerm;
        RegistrationTerm = registrationTerm;
        RegistrationWindowOpensAt = registrationWindowOpensAt;
        RegistrationWindowClosesAt = registrationWindowClosesAt;
        DisplayName = displayName;
        AuthorizedRoles = authorizedRoles.ToArray();
        ActiveRole = activeRole;
        RoleSelectionRequired = roleSelectionRequired;
        SessionState = sessionState;
        SessionExpiresAt = sessionExpiresAt;
        ServiceState = serviceState;
        SupportReferencePath = supportReferencePath;
    }

    public DateTimeOffset ServerDateTime { get; }

    public string TimeZone { get; }

    public string TeachingTerm { get; }

    public string RegistrationTerm { get; }

    public DateTimeOffset RegistrationWindowOpensAt { get; }

    public DateTimeOffset RegistrationWindowClosesAt { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> AuthorizedRoles { get; }

    public string? ActiveRole { get; }

    public bool RoleSelectionRequired { get; }

    public string SessionState { get; }

    public DateTimeOffset SessionExpiresAt { get; }

    public string ServiceState { get; }

    public string SupportReferencePath { get; }
}
