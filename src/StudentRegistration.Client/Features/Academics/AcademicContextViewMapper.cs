using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Contracts;

namespace StudentRegistration.Client.Features.Academics;

/// <summary>
/// Projects the authenticated server contract into the shared presentation shell.
/// It performs no policy or authorization decision.
/// </summary>
public static class AcademicContextViewMapper
{
    public static FrontendAppContextView ToView(AppContextDto context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new FrontendAppContextView(
            new DateTimeOffset(context.ServerTimeUtc),
            context.TimeZoneId,
            context.TeachingTerm?.Label,
            context.RegistrationTerm?.Label,
            context.RegistrationWindowState,
            context.RegistrationWindow,
            context.DisplayName,
            context.AuthorizedRoles,
            context.ActiveRole,
            context.SessionState is SessionState.RoleSelectionRequired,
            SessionStateValue(context.SessionState),
            new DateTimeOffset(context.ExpiresAtUtc),
            ServiceStateValue(context.ServiceState),
            context.SupportReferencePath);
    }

    private static string SessionStateValue(SessionState state) => state switch
    {
        SessionState.Active => "Active",
        SessionState.Expiring => "Expiring",
        SessionState.RoleSelectionRequired => "Role selection required",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    private static string ServiceStateValue(ServiceState state) => state switch
    {
        ServiceState.Available => "Available",
        ServiceState.Maintenance => "Maintenance",
        ServiceState.Unavailable => "Unavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
