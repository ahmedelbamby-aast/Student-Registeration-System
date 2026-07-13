namespace StudentRegistration.Contracts.Auditing;

public sealed record AuditEventDraft(
    string ActorReference,
    string SubjectReference,
    string Action,
    string EntityType,
    string EntityId,
    string Reason,
    string? BeforeSummaryJson,
    string? AfterSummaryJson,
    string CorrelationId,
    DateTime OccurredAtUtc);

public interface IAuditEventWriter
{
    Task AppendAsync(
        AuditEventDraft auditEvent,
        CancellationToken cancellationToken);
}
