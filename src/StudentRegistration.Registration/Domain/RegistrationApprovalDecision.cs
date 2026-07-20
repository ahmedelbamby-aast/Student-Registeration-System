namespace StudentRegistration.Registration.Domain;

public enum RegistrationApprovalActorRole
{
    Admin = 1,
    Lecturer = 2,
    TeachingAssistant = 3,
}

public enum RegistrationApprovalDecisionValue
{
    Approved = 1,
    Rejected = 2,
}

public sealed class RegistrationApprovalDecision
{
    private RegistrationApprovalDecision()
    {
    }

    public RegistrationApprovalDecision(
        Guid id,
        Guid submissionLineId,
        Guid actorId,
        RegistrationApprovalActorRole actorRole,
        RegistrationApprovalDecisionValue decision,
        string reason,
        Guid? assignmentScopeGroupId,
        Guid clientRequestId,
        string payloadHash,
        Guid policySetId,
        string policyVersion,
        string correlationId,
        DateTime decidedAtUtc)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(submissionLineId, nameof(submissionLineId));
        RegistrationPlanDomainGuard.Identifier(actorId, nameof(actorId));
        RegistrationPlanDomainGuard.Identifier(clientRequestId, nameof(clientRequestId));
        RegistrationPlanDomainGuard.Identifier(policySetId, nameof(policySetId));
        RegistrationPlanDomainGuard.Defined(actorRole, nameof(actorRole));
        RegistrationPlanDomainGuard.Defined(decision, nameof(decision));
        if (assignmentScopeGroupId == Guid.Empty)
        {
            throw new ArgumentException("Assignment scope cannot be empty.", nameof(assignmentScopeGroupId));
        }

        if (actorRole is RegistrationApprovalActorRole.Admin && assignmentScopeGroupId is not null)
        {
            throw new ArgumentException("An Admin decision is globally scoped.", nameof(assignmentScopeGroupId));
        }

        if (actorRole is not RegistrationApprovalActorRole.Admin && assignmentScopeGroupId is null)
        {
            throw new ArgumentException("A staff decision requires its assigned group scope.", nameof(assignmentScopeGroupId));
        }

        if (decidedAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", nameof(decidedAtUtc));
        }

        Id = id;
        SubmissionLineId = submissionLineId;
        ActorId = actorId;
        ActorRole = actorRole;
        Decision = decision;
        Reason = RegistrationPlanDomainGuard.Required(reason, nameof(reason));
        AssignmentScopeGroupId = assignmentScopeGroupId;
        ClientRequestId = clientRequestId;
        PayloadHash = RegistrationPlanDomainGuard.Required(payloadHash, nameof(payloadHash));
        PolicySetId = policySetId;
        PolicyVersion = RegistrationPlanDomainGuard.Required(policyVersion, nameof(policyVersion));
        CorrelationId = RegistrationPlanDomainGuard.Required(correlationId, nameof(correlationId));
        DecidedAtUtc = decidedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid SubmissionLineId { get; private set; }
    public Guid ActorId { get; private set; }
    public RegistrationApprovalActorRole ActorRole { get; private set; }
    public RegistrationApprovalDecisionValue Decision { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid? AssignmentScopeGroupId { get; private set; }
    public Guid ClientRequestId { get; private set; }
    public string PayloadHash { get; private set; } = string.Empty;
    public Guid PolicySetId { get; private set; }
    public string PolicyVersion { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTime DecidedAtUtc { get; private set; }
}
