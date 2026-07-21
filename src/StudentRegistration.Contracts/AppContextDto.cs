using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<RegistrationWindowState>))]
public enum RegistrationWindowState
{
    [JsonStringEnumMemberName("open")]
    Open,

    [JsonStringEnumMemberName("upcoming")]
    Upcoming,

    [JsonStringEnumMemberName("closed")]
    Closed,

    [JsonStringEnumMemberName("none")]
    None
}

[JsonConverter(typeof(JsonStringEnumConverter<ServiceState>))]
public enum ServiceState
{
    [JsonStringEnumMemberName("available")]
    Available,

    [JsonStringEnumMemberName("maintenance")]
    Maintenance,

    [JsonStringEnumMemberName("unavailable")]
    Unavailable
}

[JsonConverter(typeof(JsonStringEnumConverter<SessionState>))]
public enum SessionState
{
    [JsonStringEnumMemberName("active")]
    Active,

    [JsonStringEnumMemberName("expiring")]
    Expiring
}

public sealed record AppContextDto
{
    public AppContextDto(
        DateTime serverTimeUtc,
        string timeZoneId,
        TermSummaryDto? teachingTerm,
        TermSummaryDto? registrationTerm,
        RegistrationWindowState registrationWindowState,
        RegistrationWindowSummaryDto? registrationWindow,
        ServiceState serviceState,
        string displayName,
        IReadOnlyList<string> authorizedRoles,
        string? activeRole,
        SessionState sessionState,
        DateTime expiresAtUtc,
        string supportReferencePath)
    {
        EnsureUtc(serverTimeUtc, nameof(serverTimeUtc));
        EnsureUtc(expiresAtUtc, nameof(expiresAtUtc));
        EnsureDefined(registrationWindowState, nameof(registrationWindowState));
        EnsureDefined(serviceState, nameof(serviceState));
        EnsureDefined(sessionState, nameof(sessionState));

        var roles = CopyAuthorizedRoles(authorizedRoles);
        ActiveRole = ValidateActiveRole(activeRole, sessionState, roles);

        ValidateRegistrationWindow(
            registrationTerm,
            registrationWindowState,
            registrationWindow);

        ServerTimeUtc = serverTimeUtc;
        TimeZoneId = Required(timeZoneId, nameof(timeZoneId));
        TeachingTerm = teachingTerm;
        RegistrationTerm = registrationTerm;
        RegistrationWindowState = registrationWindowState;
        RegistrationWindow = registrationWindow;
        ServiceState = serviceState;
        DisplayName = Required(displayName, nameof(displayName));
        AuthorizedRoles = Array.AsReadOnly(roles);
        SessionState = sessionState;
        ExpiresAtUtc = expiresAtUtc;
        SupportReferencePath = CanonicalPath(
            supportReferencePath,
            nameof(supportReferencePath));
    }

    public DateTime ServerTimeUtc { get; }

    public string TimeZoneId { get; }

    public TermSummaryDto? TeachingTerm { get; }

    public TermSummaryDto? RegistrationTerm { get; }

    public RegistrationWindowState RegistrationWindowState { get; }

    public RegistrationWindowSummaryDto? RegistrationWindow { get; }

    public ServiceState ServiceState { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> AuthorizedRoles { get; }

    public string? ActiveRole { get; }

    public SessionState SessionState { get; }

    public DateTime ExpiresAtUtc { get; }

    public string SupportReferencePath { get; }

    private static void ValidateRegistrationWindow(
        TermSummaryDto? registrationTerm,
        RegistrationWindowState state,
        RegistrationWindowSummaryDto? window)
    {
        if (registrationTerm is null && state is not RegistrationWindowState.None)
        {
            throw new ArgumentException(
                "A missing registration term requires the none window state.",
                nameof(state));
        }

        if (registrationTerm is null && window is not null)
        {
            throw new ArgumentException(
                "A registration window requires a registration term.",
                nameof(window));
        }

        if (state is RegistrationWindowState.None)
        {
            if (window is not null)
            {
                throw new ArgumentException(
                    "The none window state requires a null window summary.",
                    nameof(window));
            }

            return;
        }

        if (window is null || window.State != state)
        {
            throw new ArgumentException(
                "A concrete window state requires a matching window summary.",
                nameof(window));
        }
    }

    private static string[] CopyAuthorizedRoles(IReadOnlyList<string> authorizedRoles)
    {
        ArgumentNullException.ThrowIfNull(authorizedRoles);

        var roles = authorizedRoles
            .Select((role, index) => Required(role, $"{nameof(authorizedRoles)}[{index}]"))
            .ToArray();
        if (roles.Length == 0)
        {
            throw new ArgumentException(
                "At least one authorized role is required.",
                nameof(authorizedRoles));
        }

        if (roles.Distinct(StringComparer.Ordinal).Count() != roles.Length)
        {
            throw new ArgumentException(
                "Authorized roles must be unique.",
                nameof(authorizedRoles));
        }

        return roles;
    }

    private static string? ValidateActiveRole(
        string? activeRole,
        SessionState sessionState,
        IReadOnlyCollection<string> authorizedRoles)
    {
        var selectedRole = Required(activeRole, nameof(activeRole));
        if (authorizedRoles.Count != 1 ||
            !string.Equals(authorizedRoles.Single(), selectedRole, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A session must have exactly one authorized role and that role must be active.",
                nameof(activeRole));
        }

        return selectedRole;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC timestamp is required.", parameterName);
        }
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A declared contract state is required.");
        }
    }

    private static string CanonicalPath(string value, string parameterName)
    {
        var path = Required(value, parameterName);
        if (!path.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains('?') ||
            path.Contains('#'))
        {
            throw new ArgumentException(
                "A canonical application-relative path is required.",
                parameterName);
        }

        return path;
    }

    private static string Required(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}
