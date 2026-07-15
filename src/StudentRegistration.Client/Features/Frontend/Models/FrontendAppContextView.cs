using StudentRegistration.Contracts;

namespace StudentRegistration.Client.Features.Frontend.Models;

/// <summary>
/// Presentation-only projection of the authoritative server application context.
/// </summary>
public sealed class FrontendAppContextView
{
    public FrontendAppContextView(
        DateTimeOffset serverDateTime,
        string timeZone,
        string? teachingTerm,
        string? registrationTerm,
        RegistrationWindowState registrationWindowState,
        RegistrationWindowSummaryDto? registrationWindow,
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
        if (teachingTerm is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(teachingTerm);
        }

        if (registrationTerm is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(registrationTerm);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(authorizedRoles);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionState);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceState);
        ArgumentException.ThrowIfNullOrWhiteSpace(supportReferencePath);

        if (!Enum.IsDefined(registrationWindowState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(registrationWindowState),
                registrationWindowState,
                "A declared registration-window state is required.");
        }

        ValidateRegistrationWindow(
            registrationTerm,
            registrationWindowState,
            registrationWindow);

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
        RegistrationWindowState = registrationWindowState;
        RegistrationWindow = registrationWindow;
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

    public string? TeachingTerm { get; }

    public string? RegistrationTerm { get; }

    public RegistrationWindowState RegistrationWindowState { get; }

    public RegistrationWindowSummaryDto? RegistrationWindow { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> AuthorizedRoles { get; }

    public string? ActiveRole { get; }

    public bool RoleSelectionRequired { get; }

    public string SessionState { get; }

    public DateTimeOffset SessionExpiresAt { get; }

    public string ServiceState { get; }

    public string SupportReferencePath { get; }

    private static void ValidateRegistrationWindow(
        string? registrationTerm,
        RegistrationWindowState registrationWindowState,
        RegistrationWindowSummaryDto? registrationWindow)
    {
        if (registrationTerm is null
            && registrationWindowState is not RegistrationWindowState.None)
        {
            throw new ArgumentException(
                "A missing registration term requires the none window state.",
                nameof(registrationWindowState));
        }

        if (registrationTerm is null && registrationWindow is not null)
        {
            throw new ArgumentException(
                "A registration window requires a registration term.",
                nameof(registrationWindow));
        }

        if (registrationWindowState is RegistrationWindowState.None)
        {
            if (registrationWindow is not null)
            {
                throw new ArgumentException(
                    "The none window state requires no window summary.",
                    nameof(registrationWindow));
            }

            return;
        }

        if (registrationWindow is null
            || registrationWindow.State != registrationWindowState)
        {
            throw new ArgumentException(
                "A concrete window state requires a matching server summary.",
                nameof(registrationWindow));
        }
    }
}
