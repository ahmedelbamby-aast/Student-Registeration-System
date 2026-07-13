namespace StudentRegistration.Contracts;

public sealed record PublicContextDto
{
    public PublicContextDto(
        DateTime serverTimeUtc,
        string timeZoneId,
        string? teachingTermLabel,
        string? registrationTermLabel,
        RegistrationWindowState registrationWindowState,
        ServiceState serviceState)
    {
        EnsureUtc(serverTimeUtc, nameof(serverTimeUtc));
        EnsureDefined(registrationWindowState, nameof(registrationWindowState));
        EnsureDefined(serviceState, nameof(serviceState));

        var authoritativeTeachingLabel = OptionalLabel(
            teachingTermLabel,
            nameof(teachingTermLabel));
        var authoritativeRegistrationLabel = OptionalLabel(
            registrationTermLabel,
            nameof(registrationTermLabel));
        if (authoritativeRegistrationLabel is null &&
            registrationWindowState is not RegistrationWindowState.None)
        {
            throw new ArgumentException(
                "A missing registration term label requires the none window state.",
                nameof(registrationWindowState));
        }

        ServerTimeUtc = serverTimeUtc;
        TimeZoneId = Required(timeZoneId, nameof(timeZoneId));
        TeachingTermLabel = authoritativeTeachingLabel;
        RegistrationTermLabel = authoritativeRegistrationLabel;
        RegistrationWindowState = registrationWindowState;
        ServiceState = serviceState;
    }

    public DateTime ServerTimeUtc { get; }

    public string TimeZoneId { get; }

    public string? TeachingTermLabel { get; }

    public string? RegistrationTermLabel { get; }

    public RegistrationWindowState RegistrationWindowState { get; }

    public ServiceState ServiceState { get; }

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

    private static string? OptionalLabel(string? value, string parameterName) =>
        value is null ? null : Required(value, parameterName);

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}
