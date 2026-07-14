namespace StudentRegistration.IdentityAccess.Domain;

public sealed class SecurityEvent
{
    public SecurityEvent(
        Guid id,
        Guid? applicationUserId,
        string eventType,
        string actorReference,
        string subjectReference,
        string reason,
        string? beforeSummaryJson,
        string? afterSummaryJson,
        string? metadataJson,
        string correlationId,
        DateTime occurredAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A security-event identifier is required.", nameof(id));
        }

        Id = id;
        ApplicationUserId = applicationUserId;
        EventType = IdentityDomainGuard.Required(eventType, nameof(eventType), 100);
        ActorReference = IdentityDomainGuard.Required(actorReference, nameof(actorReference), 200);
        SubjectReference = IdentityDomainGuard.Required(subjectReference, nameof(subjectReference), 200);
        Reason = IdentityDomainGuard.Required(reason, nameof(reason), 1000);
        BeforeSummaryJson = IdentityDomainGuard.Optional(
            beforeSummaryJson,
            nameof(beforeSummaryJson),
            8000);
        AfterSummaryJson = IdentityDomainGuard.Optional(
            afterSummaryJson,
            nameof(afterSummaryJson),
            8000);
        MetadataJson = IdentityDomainGuard.Optional(metadataJson, nameof(metadataJson), 4000);
        CorrelationId = IdentityDomainGuard.Required(correlationId, nameof(correlationId), 100);
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }
    public Guid? ApplicationUserId { get; }
    public string EventType { get; }
    public string ActorReference { get; }
    public string SubjectReference { get; }
    public string Reason { get; }
    public string? BeforeSummaryJson { get; }
    public string? AfterSummaryJson { get; }
    public string? MetadataJson { get; }
    public string CorrelationId { get; }
    public DateTime OccurredAtUtc { get; }
}
