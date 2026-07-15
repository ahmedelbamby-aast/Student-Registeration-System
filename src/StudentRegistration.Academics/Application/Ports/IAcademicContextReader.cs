using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;

namespace StudentRegistration.Academics.Application.Ports;

public interface IAcademicContextReader
{
    Task<AcademicContextSnapshot> ResolveContextAsync(
        AcademicStudentScope? studentScope,
        CancellationToken cancellationToken = default);
}

public interface IAcademicStudentScopeReader
{
    Task<AcademicStudentScope?> FindByApplicationUserIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default);
}

public sealed record AcademicContextSnapshot
{
    public AcademicContextSnapshot(
        IReadOnlyList<AcademicTermContextRecord> terms,
        IReadOnlyList<RegistrationWindowContextRecord> publishedWindows)
    {
        ArgumentNullException.ThrowIfNull(terms);
        ArgumentNullException.ThrowIfNull(publishedWindows);
        if (terms.Any(term => term is null))
        {
            throw new ArgumentException("Term snapshots cannot contain null entries.", nameof(terms));
        }

        if (publishedWindows.Any(window => window is null))
        {
            throw new ArgumentException(
                "Registration-window snapshots cannot contain null entries.",
                nameof(publishedWindows));
        }

        Terms = Array.AsReadOnly(terms.ToArray());
        PublishedWindows = Array.AsReadOnly(publishedWindows.ToArray());
    }

    public IReadOnlyList<AcademicTermContextRecord> Terms { get; }

    public IReadOnlyList<RegistrationWindowContextRecord> PublishedWindows { get; }
}

public sealed record AcademicStudentScope
{
    public AcademicStudentScope(string programCode, string cohort)
    {
        ProgramCode = AcademicContextPortGuard.Required(
            programCode,
            nameof(programCode),
            50);
        Cohort = AcademicContextPortGuard.Required(cohort, nameof(cohort), 50);
    }

    public string ProgramCode { get; }

    public string Cohort { get; }
}

public sealed record AcademicTermContextRecord
{
    private readonly byte[] _rowVersion;

    public AcademicTermContextRecord(
        Guid id,
        string code,
        string label,
        string timeZoneId,
        DateOnly teachingStartsOn,
        DateOnly teachingEndsOn,
        TermState state,
        byte[] rowVersion)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A term identifier is required.", nameof(id));
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
                "A declared term state is required.");
        }

        Id = id;
        Code = AcademicContextPortGuard.Required(code, nameof(code), 50);
        Label = AcademicContextPortGuard.Required(label, nameof(label), 200);
        TimeZoneId = AcademicContextPortGuard.IanaTimeZone(timeZoneId, nameof(timeZoneId));
        TeachingStartsOn = teachingStartsOn;
        TeachingEndsOn = teachingEndsOn;
        State = state;
        _rowVersion = AcademicContextPortGuard.Version(rowVersion, nameof(rowVersion));
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Label { get; }

    public string TimeZoneId { get; }

    public DateOnly TeachingStartsOn { get; }

    public DateOnly TeachingEndsOn { get; }

    public TermState State { get; }

    public byte[] RowVersion => [.. _rowVersion];
}

public sealed record RegistrationWindowContextRecord
{
    private readonly byte[] _rowVersion;

    public RegistrationWindowContextRecord(
        Guid id,
        Guid termId,
        RegistrationWindowScopeType scopeType,
        string? scopeValue,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        byte[] rowVersion)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A window identifier is required.", nameof(id));
        }

        if (termId == Guid.Empty)
        {
            throw new ArgumentException("A term identifier is required.", nameof(termId));
        }

        if (!Enum.IsDefined(scopeType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(scopeType),
                scopeType,
                "A declared registration-window scope is required.");
        }

        Id = id;
        TermId = termId;
        ScopeType = scopeType;
        ScopeValue = NormalizeScopeValue(scopeType, scopeValue);
        OpensAtUtc = AcademicContextPortGuard.Utc(opensAtUtc, nameof(opensAtUtc));
        ClosesAtUtc = AcademicContextPortGuard.Utc(closesAtUtc, nameof(closesAtUtc));
        if (ClosesAtUtc <= OpensAtUtc)
        {
            throw new ArgumentException(
                "The close instant must be after the open instant.",
                nameof(closesAtUtc));
        }

        _rowVersion = AcademicContextPortGuard.Version(rowVersion, nameof(rowVersion));
    }

    public Guid Id { get; }

    public Guid TermId { get; }

    public RegistrationWindowScopeType ScopeType { get; }

    public string? ScopeValue { get; }

    public DateTime OpensAtUtc { get; }

    public DateTime ClosesAtUtc { get; }

    public byte[] RowVersion => [.. _rowVersion];

    public bool AppliesTo(AcademicStudentScope studentScope)
    {
        ArgumentNullException.ThrowIfNull(studentScope);

        return ScopeType switch
        {
            RegistrationWindowScopeType.AllStudents => true,
            RegistrationWindowScopeType.Program => string.Equals(
                ScopeValue,
                studentScope.ProgramCode,
                StringComparison.OrdinalIgnoreCase),
            RegistrationWindowScopeType.Cohort => string.Equals(
                ScopeValue,
                studentScope.Cohort,
                StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static string? NormalizeScopeValue(
        RegistrationWindowScopeType scopeType,
        string? scopeValue)
    {
        if (scopeType is RegistrationWindowScopeType.AllStudents)
        {
            if (!string.IsNullOrWhiteSpace(scopeValue))
            {
                throw new ArgumentException(
                    "The all-students scope cannot have a scope value.",
                    nameof(scopeValue));
            }

            return null;
        }

        return AcademicContextPortGuard.Required(scopeValue, nameof(scopeValue), 100);
    }
}

internal static class AcademicContextPortGuard
{
    public static string Required(string? value, string parameterName, int maximumLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
        {
            throw new ArgumentException(
                $"A non-empty value no longer than {maximumLength} characters is required.",
                parameterName);
        }

        return normalized;
    }

    public static string IanaTimeZone(string? value, string parameterName)
    {
        var timeZoneId = Required(value, parameterName, 100);
        if (!timeZoneId.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("An IANA timezone identifier is required.", parameterName);
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ArgumentException("A known IANA timezone identifier is required.", parameterName, exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ArgumentException("A valid IANA timezone identifier is required.", parameterName, exception);
        }

        return timeZoneId;
    }

    public static DateTime Utc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC timestamp is required.", parameterName);
        }

        return value;
    }

    public static byte[] Version(byte[] value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (value.Length == 0)
        {
            throw new ArgumentException("A row version is required.", parameterName);
        }

        return [.. value];
    }
}
