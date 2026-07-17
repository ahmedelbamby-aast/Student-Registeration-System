using StudentRegistration.StaffAdministration.Application.Ports;

namespace StudentRegistration.StaffAdministration.Application;

public enum AuditSourceStream
{
    Audit = 1,
    IdentitySecurity = 2,
}

public sealed record RedactedChangeFieldDto(
    string Name,
    string DisplayValue);

public sealed record RedactedChangeSummaryDto(
    IReadOnlyList<RedactedChangeFieldDto> Fields,
    string RedactionVersion);

public sealed record AuditEventDto(
    Guid Id,
    DateTime OccurredAtUtc,
    string ActorId,
    string ActorDisplay,
    string Action,
    string EntityType,
    string EntityId,
    string Reason,
    RedactedChangeSummaryDto BeforeSummary,
    RedactedChangeSummaryDto AfterSummary,
    string CorrelationId,
    string SourceStream);

public sealed record AuditEventPageDto(
    IReadOnlyList<AuditEventDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class AdminAuditScope
{
    private const int MaximumSubjectReferences = 500;

    private AdminAuditScope(
        bool includeAllSubjects,
        IReadOnlyList<string> subjectReferences,
        bool includeIdentitySecurityEvents)
    {
        IncludeAllSubjects = includeAllSubjects;
        SubjectReferences = subjectReferences;
        IncludeIdentitySecurityEvents = includeIdentitySecurityEvents;
    }

    public bool IncludeAllSubjects { get; }

    public IReadOnlyList<string> SubjectReferences { get; }

    public bool IncludeIdentitySecurityEvents { get; }

    public static AdminAuditScope All(bool includeIdentitySecurityEvents) =>
        new(true, [], includeIdentitySecurityEvents);

    public static AdminAuditScope Restricted(
        IEnumerable<string> subjectReferences,
        bool includeIdentitySecurityEvents)
    {
        ArgumentNullException.ThrowIfNull(subjectReferences);
        var normalized = subjectReferences
            .Select(reference => Required(reference, nameof(subjectReferences), 200))
            .Distinct(StringComparer.Ordinal)
            .Take(MaximumSubjectReferences + 1)
            .ToArray();
        if (normalized.Length is 0 or > MaximumSubjectReferences)
        {
            throw new ArgumentException(
                $"Between 1 and {MaximumSubjectReferences} subject references are required.",
                nameof(subjectReferences));
        }

        return new(false, Array.AsReadOnly(normalized), includeIdentitySecurityEvents);
    }

    private static string Required(
        string value,
        string parameterName,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.Any(char.IsControl))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return normalized;
    }
}

public sealed record AdminAuditQuery(
    AdminAuditScope Scope,
    int Page = 1,
    int PageSize = 20,
    DateTime? OccurredFromUtc = null,
    DateTime? OccurredToUtc = null,
    string? ActorId = null,
    string? Action = null,
    AuditSourceStream? SourceStream = null);

public sealed record AdminAuditReadRequest(
    AdminAuditScope Scope,
    int Page,
    int PageSize,
    DateTime? OccurredFromUtc,
    DateTime? OccurredToUtc,
    string? ActorId,
    string? Action,
    AuditSourceStream? SourceStream);

public sealed class AuditEventQueries
{
    private readonly IAdminAuditReader _reader;

    public AuditEventQueries(IAdminAuditReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Task<AuditEventPageDto> SearchAsync(
        AdminAuditQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(query.Scope);
        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Page));
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(query.PageSize));
        }

        if ((long)query.Page * query.PageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Page));
        }

        EnsureUtc(query.OccurredFromUtc, nameof(query.OccurredFromUtc));
        EnsureUtc(query.OccurredToUtc, nameof(query.OccurredToUtc));
        if (query.OccurredFromUtc is not null
            && query.OccurredToUtc is not null
            && query.OccurredFromUtc > query.OccurredToUtc)
        {
            throw new ArgumentException(
                "The start of the audit range cannot follow its end.",
                nameof(query));
        }

        var actorId = Optional(query.ActorId, nameof(query.ActorId), 200);
        var action = Optional(query.Action, nameof(query.Action), 100);
        var request = new AdminAuditReadRequest(
            query.Scope,
            query.Page,
            query.PageSize,
            query.OccurredFromUtc,
            query.OccurredToUtc,
            actorId,
            action,
            query.SourceStream);
        return _reader.ReadAsync(request, cancellationToken);
    }

    private static void EnsureUtc(DateTime? value, string parameterName)
    {
        if (value is not null && value.Value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("Audit timestamps must use UTC.", parameterName);
        }
    }

    private static string? Optional(
        string? value,
        string parameterName,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.Any(char.IsControl))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return normalized;
    }
}
