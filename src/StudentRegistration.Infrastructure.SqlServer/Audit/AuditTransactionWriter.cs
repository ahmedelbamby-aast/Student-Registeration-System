using StudentRegistration.Contracts.Auditing;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Infrastructure.SqlServer.Audit;

public sealed class AuditTransactionWriter : IAuditEventWriter
{
    private readonly StudentRegistrationDbContext _dbContext;

    public AuditTransactionWriter(StudentRegistrationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task AppendAsync(
        AuditEventDraft auditEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        cancellationToken.ThrowIfCancellationRequested();

        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "AUDIT_CALLER_TRANSACTION_REQUIRED: An active caller transaction is required.");
        }

        _dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            auditEvent.ActorReference,
            auditEvent.SubjectReference,
            auditEvent.Action,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.Reason,
            auditEvent.BeforeSummaryJson,
            auditEvent.AfterSummaryJson,
            auditEvent.CorrelationId,
            auditEvent.OccurredAtUtc));

        return Task.CompletedTask;
    }
}
