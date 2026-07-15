using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace StudentRegistration.Contracts.Academics;

[JsonConverter(typeof(JsonStringEnumConverter<RegistrationWindowScope>))]
public enum RegistrationWindowScope
{
    [JsonStringEnumMemberName("all-students")]
    AllStudents,

    [JsonStringEnumMemberName("program")]
    Program,

    [JsonStringEnumMemberName("cohort")]
    Cohort
}

[JsonConverter(typeof(JsonStringEnumConverter<RegistrationWindowLifecycle>))]
public enum RegistrationWindowLifecycle
{
    [JsonStringEnumMemberName("draft")]
    Draft,

    [JsonStringEnumMemberName("published")]
    Published,

    [JsonStringEnumMemberName("emergencyClosed")]
    EmergencyClosed,

    [JsonStringEnumMemberName("superseded")]
    Superseded
}

public sealed record AdminRegistrationWindowDto
{
    public AdminRegistrationWindowDto(
        string id,
        RegistrationWindowScope scopeType,
        string? scopeValue,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        RegistrationWindowLifecycle lifecycleState,
        RegistrationWindowState computedState,
        string rowVersion)
    {
        AcademicTermContractValidation.EnsureScope(scopeType, scopeValue);
        AcademicTermContractValidation.EnsureUtc(opensAtUtc, nameof(opensAtUtc));
        AcademicTermContractValidation.EnsureUtc(closesAtUtc, nameof(closesAtUtc));
        if (closesAtUtc <= opensAtUtc)
        {
            throw new ArgumentException("The close instant must be after the open instant.", nameof(closesAtUtc));
        }

        if (!Enum.IsDefined(lifecycleState))
        {
            throw new ArgumentOutOfRangeException(nameof(lifecycleState));
        }

        if (!Enum.IsDefined(computedState) || computedState is RegistrationWindowState.None)
        {
            throw new ArgumentOutOfRangeException(nameof(computedState));
        }

        Id = AcademicTermContractValidation.Required(id, nameof(id), 100);
        ScopeType = scopeType;
        ScopeValue = AcademicTermContractValidation.Optional(scopeValue, nameof(scopeValue), 100);
        OpensAtUtc = opensAtUtc;
        ClosesAtUtc = closesAtUtc;
        LifecycleState = lifecycleState;
        ComputedState = computedState;
        RowVersion = AcademicTermContractValidation.Required(rowVersion, nameof(rowVersion), 128);
    }

    public string Id { get; }
    public RegistrationWindowScope ScopeType { get; }
    public string? ScopeValue { get; }
    public DateTime OpensAtUtc { get; }
    public DateTime ClosesAtUtc { get; }
    public RegistrationWindowLifecycle LifecycleState { get; }
    public RegistrationWindowState ComputedState { get; }
    public string RowVersion { get; }
}

public sealed record AdminTermDto
{
    public AdminTermDto(
        string id,
        string code,
        string displayName,
        string timeZoneId,
        DateOnly teachingStartsOn,
        DateOnly teachingEndsOn,
        TermState state,
        string rowVersion,
        IReadOnlyList<AdminRegistrationWindowDto> windows)
    {
        ArgumentNullException.ThrowIfNull(windows);
        if (windows.Count > 20)
        {
            throw new ArgumentException("A term may contain at most 20 registration windows.", nameof(windows));
        }

        if (teachingEndsOn <= teachingStartsOn)
        {
            throw new ArgumentException("The teaching end date must be after the start date.", nameof(teachingEndsOn));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        Id = AcademicTermContractValidation.Required(id, nameof(id), 100);
        Code = AcademicTermContractValidation.Required(code, nameof(code), 50);
        DisplayName = AcademicTermContractValidation.Required(displayName, nameof(displayName), 200);
        TimeZoneId = AcademicTermContractValidation.Required(timeZoneId, nameof(timeZoneId), 100);
        TeachingStartsOn = teachingStartsOn;
        TeachingEndsOn = teachingEndsOn;
        State = state;
        RowVersion = AcademicTermContractValidation.Required(rowVersion, nameof(rowVersion), 128);
        Windows = Array.AsReadOnly(windows.ToArray());
    }

    public string Id { get; }
    public string Code { get; }
    public string DisplayName { get; }
    public string TimeZoneId { get; }
    public DateOnly TeachingStartsOn { get; }
    public DateOnly TeachingEndsOn { get; }
    public TermState State { get; }
    public string RowVersion { get; }
    public IReadOnlyList<AdminRegistrationWindowDto> Windows { get; }
}

public sealed record TermWindowInput
{
    public TermWindowInput(
        string? id,
        RegistrationWindowScope scopeType,
        string? scopeValue,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        RegistrationWindowLifecycle lifecycleState)
    {
        AcademicTermContractValidation.EnsureScope(scopeType, scopeValue);
        AcademicTermContractValidation.EnsureUtc(opensAtUtc, nameof(opensAtUtc));
        AcademicTermContractValidation.EnsureUtc(closesAtUtc, nameof(closesAtUtc));
        if (closesAtUtc <= opensAtUtc)
        {
            throw new ArgumentException("The close instant must be after the open instant.", nameof(closesAtUtc));
        }

        if (!Enum.IsDefined(lifecycleState))
        {
            throw new ArgumentOutOfRangeException(nameof(lifecycleState));
        }

        Id = AcademicTermContractValidation.Optional(id, nameof(id), 100);
        ScopeType = scopeType;
        ScopeValue = AcademicTermContractValidation.Optional(scopeValue, nameof(scopeValue), 100);
        OpensAtUtc = opensAtUtc;
        ClosesAtUtc = closesAtUtc;
        LifecycleState = lifecycleState;
    }

    public string? Id { get; }
    public RegistrationWindowScope ScopeType { get; }
    public string? ScopeValue { get; }
    public DateTime OpensAtUtc { get; }
    public DateTime ClosesAtUtc { get; }
    public RegistrationWindowLifecycle LifecycleState { get; }
}

public sealed record TermInput
{
    public TermInput(
        string code,
        string displayName,
        string timeZoneId,
        DateOnly teachingStartsOn,
        DateOnly teachingEndsOn,
        TermState state)
    {
        if (teachingEndsOn <= teachingStartsOn)
        {
            throw new ArgumentException("The teaching end date must be after the start date.", nameof(teachingEndsOn));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        Code = AcademicTermContractValidation.Required(code, nameof(code), 50);
        DisplayName = AcademicTermContractValidation.Required(displayName, nameof(displayName), 200);
        TimeZoneId = AcademicTermContractValidation.Required(timeZoneId, nameof(timeZoneId), 100);
        TeachingStartsOn = teachingStartsOn;
        TeachingEndsOn = teachingEndsOn;
        State = state;
    }

    public string Code { get; }
    public string DisplayName { get; }
    public string TimeZoneId { get; }
    public DateOnly TeachingStartsOn { get; }
    public DateOnly TeachingEndsOn { get; }
    public TermState State { get; }
}

public sealed record CreateTermRequest
{
    public CreateTermRequest(
        Guid clientRequestId,
        string reason,
        string source,
        TermInput term,
        IReadOnlyList<TermWindowInput> windows)
    {
        if (clientRequestId == Guid.Empty)
        {
            throw new ArgumentException("A client request ID is required.", nameof(clientRequestId));
        }

        ArgumentNullException.ThrowIfNull(term);
        Windows = AcademicTermContractValidation.Windows(windows, nameof(windows));
        ClientRequestId = clientRequestId;
        Reason = AcademicTermContractValidation.Reason(reason, nameof(reason));
        Source = AcademicTermContractValidation.Required(source, nameof(source), 200);
        Term = term;
    }

    public Guid ClientRequestId { get; }
    public string Reason { get; }
    public string Source { get; }
    public TermInput Term { get; }
    public IReadOnlyList<TermWindowInput> Windows { get; }
}

public sealed record UpdateTermRequest
{
    public UpdateTermRequest(
        string expectedTermRowVersion,
        IReadOnlyDictionary<string, string> expectedWindowRowVersions,
        string reason,
        string source,
        TermInput term,
        IReadOnlyList<TermWindowInput> windows)
    {
        ArgumentNullException.ThrowIfNull(expectedWindowRowVersions);
        ArgumentNullException.ThrowIfNull(term);
        if (expectedWindowRowVersions.Count > 20)
        {
            throw new ArgumentException("At most 20 window versions are allowed.", nameof(expectedWindowRowVersions));
        }

        var versions = expectedWindowRowVersions.ToDictionary(
            pair => AcademicTermContractValidation.Required(pair.Key, nameof(expectedWindowRowVersions), 100),
            pair => AcademicTermContractValidation.Required(pair.Value, nameof(expectedWindowRowVersions), 128),
            StringComparer.Ordinal);

        ExpectedTermRowVersion = AcademicTermContractValidation.Required(expectedTermRowVersion, nameof(expectedTermRowVersion), 128);
        ExpectedWindowRowVersions = new ReadOnlyDictionary<string, string>(versions);
        Reason = AcademicTermContractValidation.Reason(reason, nameof(reason));
        Source = AcademicTermContractValidation.Required(source, nameof(source), 200);
        Term = term;
        Windows = AcademicTermContractValidation.Windows(windows, nameof(windows));
    }

    public string ExpectedTermRowVersion { get; }
    public IReadOnlyDictionary<string, string> ExpectedWindowRowVersions { get; }
    public string Reason { get; }
    public string Source { get; }
    public TermInput Term { get; }
    public IReadOnlyList<TermWindowInput> Windows { get; }
}

public sealed record PublishRegistrationWindowRequest
{
    public PublishRegistrationWindowRequest(
        string expectedTermRowVersion,
        string expectedWindowRowVersion,
        string reason,
        string source)
    {
        ExpectedTermRowVersion = AcademicTermContractValidation.Required(expectedTermRowVersion, nameof(expectedTermRowVersion), 128);
        ExpectedWindowRowVersion = AcademicTermContractValidation.Required(expectedWindowRowVersion, nameof(expectedWindowRowVersion), 128);
        Reason = AcademicTermContractValidation.Reason(reason, nameof(reason));
        Source = AcademicTermContractValidation.Required(source, nameof(source), 200);
    }

    public string ExpectedTermRowVersion { get; }
    public string ExpectedWindowRowVersion { get; }
    public string Reason { get; }
    public string Source { get; }
}

internal static class AcademicTermContractValidation
{
    public static string Required(string? value, string parameterName, int maximumLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length > maximumLength)
        {
            throw new ArgumentException($"A non-empty value no longer than {maximumLength} characters is required.", parameterName);
        }

        return trimmed;
    }

    public static string? Optional(string? value, string parameterName, int maximumLength) =>
        value is null ? null : Required(value, parameterName, maximumLength);

    public static string Reason(string value, string parameterName)
    {
        var reason = Required(value, parameterName, 500);
        if (reason.Length < 10)
        {
            throw new ArgumentException("A reason must contain at least 10 characters.", parameterName);
        }

        return reason;
    }

    public static IReadOnlyList<TermWindowInput> Windows(
        IReadOnlyList<TermWindowInput> windows,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(windows);
        if (windows.Count > 20)
        {
            throw new ArgumentException("At most 20 windows are allowed.", parameterName);
        }

        if (windows.Any(window => window is null))
        {
            throw new ArgumentException("Window entries cannot be null.", parameterName);
        }

        return Array.AsReadOnly(windows.ToArray());
    }

    public static void EnsureScope(RegistrationWindowScope scopeType, string? scopeValue)
    {
        if (!Enum.IsDefined(scopeType))
        {
            throw new ArgumentOutOfRangeException(nameof(scopeType));
        }

        if (scopeType is RegistrationWindowScope.AllStudents && scopeValue is not null ||
            scopeType is not RegistrationWindowScope.AllStudents && string.IsNullOrWhiteSpace(scopeValue))
        {
            throw new ArgumentException("Scope and scope value do not form a valid normalized scope.", nameof(scopeValue));
        }
    }

    public static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC timestamp is required.", parameterName);
        }
    }
}
