namespace StudentRegistration.IdentityAccess.Domain;

public sealed class RoleAssignment
{
    private RoleAssignment()
    {
    }

    public RoleAssignment(
        Guid id,
        Guid applicationUserId,
        string roleCode,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string assignedByReference)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A role assignment identifier is required.", nameof(id));
        }

        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException("An application user is required.", nameof(applicationUserId));
        }

        if (effectiveToUtc is not null && effectiveToUtc <= effectiveFromUtc)
        {
            throw new ArgumentException(
                "The effective end must be after the effective start.",
                nameof(effectiveToUtc));
        }

        Id = id;
        ApplicationUserId = applicationUserId;
        RoleCode = IdentityDomainGuard.Required(roleCode, nameof(roleCode), 50);
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        AssignedByReference = IdentityDomainGuard.Required(
            assignedByReference,
            nameof(assignedByReference),
            200);
    }

    public Guid Id { get; private set; }
    public Guid ApplicationUserId { get; private set; }
    public string RoleCode { get; private set; } = string.Empty;
    public DateTime EffectiveFromUtc { get; private set; }
    public DateTime? EffectiveToUtc { get; private set; }
    public string AssignedByReference { get; private set; } = string.Empty;
    public byte[] Version { get; private set; } = [];

    public bool IsEffectiveAt(DateTime utcNow) =>
        EffectiveFromUtc <= utcNow &&
        (EffectiveToUtc is null || utcNow < EffectiveToUtc);

    public void EndAt(DateTime effectiveToUtc)
    {
        if (effectiveToUtc <= EffectiveFromUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveToUtc));
        }

        EffectiveToUtc = effectiveToUtc;
    }
}
