using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Contracts;

namespace StudentRegistration.Client.Features.Registration;

public static class RegistrationPageSupport
{
    public static bool IsStudentContext(AppContextDto context) =>
        string.Equals(context.ActiveRole, "Student", StringComparison.Ordinal) &&
        context.AuthorizedRoles.Contains("Student", StringComparer.Ordinal);

    public static FrontendAppContextView ToShellContext(AppContextDto context) => new(
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
        SessionStateToken(context.SessionState),
        new DateTimeOffset(context.ExpiresAtUtc),
        ServiceStateToken(context.ServiceState),
        context.SupportReferencePath);

    public static string DayName(int day) => day is >= 0 and <= 6
        ? ((DayOfWeek)day).ToString()
        : $"Day {day}";

    private static string SessionStateToken(SessionState state) => state switch
    {
        SessionState.Active => "active",
        SessionState.Expiring => "expiring",
        SessionState.RoleSelectionRequired => "role-selection-required",
        _ => "unknown"
    };

    private static string ServiceStateToken(ServiceState state) => state switch
    {
        ServiceState.Available => "available",
        ServiceState.Maintenance => "maintenance",
        ServiceState.Unavailable => "unavailable",
        _ => "unavailable"
    };
}
