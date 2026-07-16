namespace StudentRegistration.Academics.Domain;

public enum PolicySetState
{
    Draft = 1,
    Validated = 2,
    Published = 3,
    Superseded = 4,
}

public sealed class PolicySet
{
    private PolicySet()
    {
    }

    public PolicySet(
        Guid id,
        string versionCode,
        Guid termId,
        Guid? programId,
        string scopeCode,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        PolicySetState state)
    {
        DomainValue.Identifier(id, nameof(id));
        DomainValue.Identifier(termId, nameof(termId));
        DomainValue.Utc(effectiveFromUtc, nameof(effectiveFromUtc));
        if (effectiveToUtc is { } end)
        {
            DomainValue.Utc(end, nameof(effectiveToUtc));
            if (end <= effectiveFromUtc)
            {
                throw new ArgumentException(
                    "The effective end must be after the start.",
                    nameof(effectiveToUtc));
            }
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        Id = id;
        VersionCode = DomainValue.Required(versionCode, nameof(versionCode));
        TermId = termId;
        ProgramId = programId;
        ScopeCode = DomainValue.Code(scopeCode, nameof(scopeCode));
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        State = state;
    }

    public Guid Id { get; private set; }

    public string VersionCode { get; private set; } = string.Empty;

    public Guid TermId { get; private set; }

    public Guid? ProgramId { get; private set; }

    public string ScopeCode { get; private set; } = string.Empty;

    public DateTime EffectiveFromUtc { get; private set; }

    public DateTime? EffectiveToUtc { get; private set; }

    public PolicySetState State { get; private set; }

    public byte[] VersionToken { get; private set; } = [];

    public void MarkValidated()
    {
        if (State is not PolicySetState.Draft)
        {
            throw new InvalidOperationException("Only a draft policy set can be validated.");
        }

        State = PolicySetState.Validated;
    }

    public void Publish()
    {
        if (State is not PolicySetState.Validated)
        {
            throw new InvalidOperationException("Only a validated policy set can be published.");
        }

        State = PolicySetState.Published;
    }

    public void Supersede()
    {
        if (State is not PolicySetState.Published)
        {
            throw new InvalidOperationException("Only a published policy set can be superseded.");
        }

        State = PolicySetState.Superseded;
    }
}
