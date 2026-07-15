using StudentRegistration.Contracts;

namespace StudentRegistration.Academics.Domain;

public sealed class AcademicTerm
{
    private AcademicTerm()
    {
    }

    public AcademicTerm(
        Guid id,
        string code,
        Guid creationClientRequestId,
        string creationPayloadHash,
        string displayName,
        DateOnly teachingStartsOn,
        DateOnly teachingEndsOn,
        string timeZoneId,
        TermState state)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An academic term identifier is required.", nameof(id));
        }

        if (creationClientRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "A creation request identifier is required.",
                nameof(creationClientRequestId));
        }

        if (teachingEndsOn <= teachingStartsOn)
        {
            throw new ArgumentException(
                "The teaching end date must be after the start date.",
                nameof(teachingEndsOn));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A declared academic term state is required.");
        }

        Id = id;
        Code = Required(code, nameof(code));
        CreationClientRequestId = creationClientRequestId;
        CreationPayloadHash = Required(
            creationPayloadHash,
            nameof(creationPayloadHash));
        DisplayName = Required(displayName, nameof(displayName));
        TeachingStartsOn = teachingStartsOn;
        TeachingEndsOn = teachingEndsOn;
        TimeZoneId = RequireIanaTimeZone(timeZoneId, nameof(timeZoneId));
        State = state;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public Guid CreationClientRequestId { get; private set; }

    public string CreationPayloadHash { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public DateOnly TeachingStartsOn { get; private set; }

    public DateOnly TeachingEndsOn { get; private set; }

    public string TimeZoneId { get; private set; } = string.Empty;

    public TermState State { get; private set; }

    public byte[] Version { get; private set; } = [];

    private static string RequireIanaTimeZone(string value, string parameterName)
    {
        var timeZoneId = Required(value, parameterName);
        if (!TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out _))
        {
            throw new ArgumentException(
                "A valid IANA timezone identifier is required.",
                parameterName);
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ArgumentException(
                "A valid IANA timezone identifier is required.",
                parameterName,
                exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ArgumentException(
                "A valid IANA timezone identifier is required.",
                parameterName,
                exception);
        }

        return timeZoneId;
    }

    private static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return normalized;
    }
}
