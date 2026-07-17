using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;

namespace StudentRegistration.Infrastructure.SqlServer.Admin;

public sealed class SqlAdminAuditReader : IAdminAuditReader
{
    private const string AuditStream = "audit";
    private const string IdentitySecurityStream = "identity-security";
    private const string RedactionVersion = "spec017-v1";
    private const int MaximumSummaryFields = 20;
    private const int MaximumDisplayValueLength = 200;

    private static readonly HashSet<string> AllowedSummaryFields = new(
        [
            "state",
            "status",
            "enabled",
            "roles",
            "capacity",
            "enrolledCount",
            "version",
            "lifecycle",
            "termId",
            "groupId",
            "offeringId",
            "policySetId",
            "reasonCode",
            "source"
        ],
        StringComparer.OrdinalIgnoreCase);

    private readonly StudentRegistrationDbContext _dbContext;

    public SqlAdminAuditReader(StudentRegistrationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<AuditEventPageDto> ReadAsync(
        AdminAuditReadRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Scope);

        var queries = new List<IQueryable<AuditRow>>(2);
        if (request.SourceStream is not AuditSourceStream.IdentitySecurity)
        {
            queries.Add(BuildAuditQuery(request));
        }

        if (request.Scope.IncludeIdentitySecurityEvents
            && request.SourceStream is not AuditSourceStream.Audit)
        {
            queries.Add(BuildSecurityQuery(request));
        }

        if (queries.Count == 0)
        {
            return new AuditEventPageDto([], request.Page, request.PageSize, 0);
        }

        var candidateLimit = checked(request.Page * request.PageSize);
        var totalCount = 0;
        var candidates = new List<AuditRow>();
        foreach (var query in queries)
        {
            totalCount += await query.CountAsync(cancellationToken);
            candidates.AddRange(await query
                .OrderByDescending(row => row.OccurredAtUtc)
                .ThenByDescending(row => row.Id)
                .Take(candidateLimit)
                .ToArrayAsync(cancellationToken));
        }

        var items = candidates
            .Select(ToDto)
            .OrderByDescending(item => item.OccurredAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();
        return new AuditEventPageDto(
            Array.AsReadOnly(items),
            request.Page,
            request.PageSize,
            totalCount);
    }

    private IQueryable<AuditRow> BuildAuditQuery(AdminAuditReadRequest request)
    {
        var query = _dbContext.AuditEvents.AsNoTracking();
        if (!request.Scope.IncludeAllSubjects)
        {
            var subjects = request.Scope.SubjectReferences.ToArray();
            query = query.Where(auditEvent => subjects.Contains(auditEvent.SubjectReference));
        }

        if (request.OccurredFromUtc is not null)
        {
            query = query.Where(auditEvent =>
                auditEvent.OccurredAtUtc >= request.OccurredFromUtc.Value);
        }

        if (request.OccurredToUtc is not null)
        {
            query = query.Where(auditEvent =>
                auditEvent.OccurredAtUtc <= request.OccurredToUtc.Value);
        }

        if (request.ActorId is not null)
        {
            query = query.Where(auditEvent =>
                auditEvent.ActorReference == request.ActorId);
        }

        if (request.Action is not null)
        {
            query = query.Where(auditEvent => auditEvent.Action == request.Action);
        }

        return query.Select(auditEvent => new AuditRow
        {
            Id = auditEvent.Id,
            OccurredAtUtc = auditEvent.OccurredAtUtc,
            ActorReference = auditEvent.ActorReference,
            Action = auditEvent.Action,
            EntityType = auditEvent.EntityType,
            EntityId = auditEvent.EntityId,
            Reason = auditEvent.Reason,
            BeforeSummaryJson = auditEvent.BeforeSummaryJson,
            AfterSummaryJson = auditEvent.AfterSummaryJson,
            CorrelationId = auditEvent.CorrelationId,
            SourceStream = AuditStream
        });
    }

    private IQueryable<AuditRow> BuildSecurityQuery(AdminAuditReadRequest request)
    {
        var query = _dbContext.Set<SecurityEvent>().AsNoTracking();
        if (!request.Scope.IncludeAllSubjects)
        {
            var subjects = request.Scope.SubjectReferences.ToArray();
            query = query.Where(securityEvent =>
                subjects.Contains(securityEvent.SubjectReference));
        }

        if (request.OccurredFromUtc is not null)
        {
            query = query.Where(securityEvent =>
                securityEvent.OccurredAtUtc >= request.OccurredFromUtc.Value);
        }

        if (request.OccurredToUtc is not null)
        {
            query = query.Where(securityEvent =>
                securityEvent.OccurredAtUtc <= request.OccurredToUtc.Value);
        }

        if (request.ActorId is not null)
        {
            query = query.Where(securityEvent =>
                securityEvent.ActorReference == request.ActorId);
        }

        if (request.Action is not null)
        {
            query = query.Where(securityEvent => securityEvent.EventType == request.Action);
        }

        return query.Select(securityEvent => new AuditRow
        {
            Id = securityEvent.Id,
            OccurredAtUtc = securityEvent.OccurredAtUtc,
            ActorReference = securityEvent.ActorReference,
            Action = securityEvent.EventType,
            EntityType = "SecurityEvent",
            EntityId = securityEvent.SubjectReference,
            ApplicationUserId = securityEvent.ApplicationUserId,
            Reason = securityEvent.Reason,
            BeforeSummaryJson = securityEvent.BeforeSummaryJson,
            AfterSummaryJson = securityEvent.AfterSummaryJson,
            CorrelationId = securityEvent.CorrelationId,
            SourceStream = IdentitySecurityStream
        });
    }

    private static AuditEventDto ToDto(AuditRow row) => new(
        row.Id,
        row.OccurredAtUtc,
        row.ActorReference,
        row.ActorReference,
        row.Action,
        row.EntityType,
        row.ApplicationUserId?.ToString() ?? row.EntityId,
        row.Reason,
        Redact(row.BeforeSummaryJson),
        Redact(row.AfterSummaryJson),
        row.CorrelationId,
        row.SourceStream);

    private static RedactedChangeSummaryDto Redact(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return EmptySummary();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind is not JsonValueKind.Object)
            {
                return EmptySummary();
            }

            var fields = document.RootElement
                .EnumerateObject()
                .Where(property => AllowedSummaryFields.Contains(property.Name))
                .Where(property => property.Value.ValueKind is
                    JsonValueKind.String
                    or JsonValueKind.Number
                    or JsonValueKind.True
                    or JsonValueKind.False
                    or JsonValueKind.Null)
                .OrderBy(property => property.Name, StringComparer.OrdinalIgnoreCase)
                .Take(MaximumSummaryFields)
                .Select(property => new RedactedChangeFieldDto(
                    Bound(property.Name, 64),
                    Bound(DisplayValue(property.Value), MaximumDisplayValueLength)))
                .ToArray();
            return new RedactedChangeSummaryDto(
                Array.AsReadOnly(fields),
                RedactionVersion);
        }
        catch (JsonException)
        {
            return EmptySummary();
        }
    }

    private static RedactedChangeSummaryDto EmptySummary() =>
        new([], RedactionVersion);

    private static string DisplayValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Null => string.Empty,
        _ => value.GetRawText()
    };

    private static string Bound(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..maximumLength];

    private sealed class AuditRow
    {
        public Guid Id { get; init; }
        public DateTime OccurredAtUtc { get; init; }
        public string ActorReference { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public string EntityType { get; init; } = string.Empty;
        public string EntityId { get; init; } = string.Empty;
        public Guid? ApplicationUserId { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string? BeforeSummaryJson { get; init; }
        public string? AfterSummaryJson { get; init; }
        public string CorrelationId { get; init; } = string.Empty;
        public string SourceStream { get; init; } = string.Empty;
    }
}
