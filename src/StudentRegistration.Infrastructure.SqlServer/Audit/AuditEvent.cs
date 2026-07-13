namespace StudentRegistration.Infrastructure.SqlServer.Audit;

public sealed class AuditEvent
{
    public AuditEvent(
        Guid id,
        string actorReference,
        string subjectReference,
        string action,
        string entityType,
        string entityId,
        string reason,
        string? beforeSummaryJson,
        string? afterSummaryJson,
        string correlationId,
        DateTime occurredAtUtc)
    {
        Id = id;
        ActorReference = actorReference;
        SubjectReference = subjectReference;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Reason = reason;
        BeforeSummaryJson = beforeSummaryJson;
        AfterSummaryJson = afterSummaryJson;
        CorrelationId = correlationId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }
    public string ActorReference { get; }
    public string SubjectReference { get; }
    public string Action { get; }
    public string EntityType { get; }
    public string EntityId { get; }
    public string Reason { get; }
    public string? BeforeSummaryJson { get; }
    public string? AfterSummaryJson { get; }
    public string CorrelationId { get; }
    public DateTime OccurredAtUtc { get; }
}
