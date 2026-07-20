namespace StudentRegistration.Registration.Domain;

public enum RegistrationSeatHoldState
{
    Active = 1,
    Consumed = 2,
    Released = 3,
    Expired = 4,
}

public sealed class RegistrationSeatHold
{
    private RegistrationSeatHold()
    {
    }

    public RegistrationSeatHold(
        Guid id,
        Guid submissionLineId,
        Guid studentId,
        Guid termId,
        Guid offeringId,
        Guid groupId,
        DateTime heldAtUtc)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(submissionLineId, nameof(submissionLineId));
        RegistrationPlanDomainGuard.Identifier(studentId, nameof(studentId));
        RegistrationPlanDomainGuard.Identifier(termId, nameof(termId));
        RegistrationPlanDomainGuard.Identifier(offeringId, nameof(offeringId));
        RegistrationPlanDomainGuard.Identifier(groupId, nameof(groupId));
        EnsureUtc(heldAtUtc, nameof(heldAtUtc));

        Id = id;
        SubmissionLineId = submissionLineId;
        StudentId = studentId;
        TermId = termId;
        OfferingId = offeringId;
        GroupId = groupId;
        State = RegistrationSeatHoldState.Active;
        HeldAtUtc = heldAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid SubmissionLineId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid TermId { get; private set; }
    public Guid OfferingId { get; private set; }
    public Guid GroupId { get; private set; }
    public RegistrationSeatHoldState State { get; private set; }
    public DateTime HeldAtUtc { get; private set; }
    public DateTime? ReleasedAtUtc { get; private set; }
    public string? ReleaseReason { get; private set; }
    public byte[] Version { get; private set; } = [];

    public void Consume(DateTime atUtc) => Transition(
        RegistrationSeatHoldState.Consumed, null, atUtc);

    public void Release(string reason, DateTime atUtc) => Transition(
        RegistrationSeatHoldState.Released,
        RegistrationPlanDomainGuard.Required(reason, nameof(reason)),
        atUtc);

    public void Expire(string reason, DateTime atUtc) => Transition(
        RegistrationSeatHoldState.Expired,
        RegistrationPlanDomainGuard.Required(reason, nameof(reason)),
        atUtc);

    private void Transition(
        RegistrationSeatHoldState target,
        string? reason,
        DateTime atUtc)
    {
        if (State is not RegistrationSeatHoldState.Active)
        {
            throw new InvalidOperationException("A terminal seat hold is immutable.");
        }

        EnsureUtc(atUtc, nameof(atUtc));
        if (atUtc < HeldAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(atUtc));
        }

        State = target;
        ReleaseReason = reason;
        ReleasedAtUtc = atUtc;
    }

    private static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", name);
        }
    }
}
