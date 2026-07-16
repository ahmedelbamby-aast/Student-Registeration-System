namespace StudentRegistration.Academics.Domain;

public enum CatalogueDraftState
{
    Editing = 1,
    Validated = 2,
    Published = 3,
    Abandoned = 4,
}

public sealed class CatalogueDraft
{
    private CatalogueDraft()
    {
    }

    public CatalogueDraft(
        Guid id,
        string scopeCode,
        Guid? basedOnVersionId,
        string canonicalContentHash,
        string contentJson,
        string validationSummaryJson,
        CatalogueDraftState state)
    {
        DomainValue.Identifier(id, nameof(id));
        if (basedOnVersionId == Guid.Empty)
        {
            throw new ArgumentException(
                "A based-on version identifier cannot be empty.",
                nameof(basedOnVersionId));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        Id = id;
        ScopeCode = DomainValue.Code(scopeCode, nameof(scopeCode));
        BasedOnVersionId = basedOnVersionId;
        CanonicalContentHash = DomainValue.Required(
            canonicalContentHash,
            nameof(canonicalContentHash));
        ContentJson = DomainValue.Required(contentJson, nameof(contentJson));
        ValidationSummaryJson = validationSummaryJson?.Trim() ?? string.Empty;
        State = state;
    }

    public Guid Id { get; private set; }

    public string ScopeCode { get; private set; } = string.Empty;

    public Guid? BasedOnVersionId { get; private set; }

    public string CanonicalContentHash { get; private set; } = string.Empty;

    public string ContentJson { get; private set; } = string.Empty;

    public string ValidationSummaryJson { get; private set; } = string.Empty;

    public CatalogueDraftState State { get; private set; }

    public byte[] Version { get; private set; } = [];

    public void ReplaceContent(string canonicalContentHash, string contentJson)
    {
        if (State is CatalogueDraftState.Published or CatalogueDraftState.Abandoned)
        {
            throw new InvalidOperationException("A closed catalogue draft is immutable.");
        }

        CanonicalContentHash = DomainValue.Required(
            canonicalContentHash,
            nameof(canonicalContentHash));
        ContentJson = DomainValue.Required(contentJson, nameof(contentJson));
        ValidationSummaryJson = string.Empty;
        State = CatalogueDraftState.Editing;
    }

    public void MarkValidated(string validationSummaryJson)
    {
        if (State is not CatalogueDraftState.Editing)
        {
            throw new InvalidOperationException("Only an editing draft can be validated.");
        }

        ValidationSummaryJson = DomainValue.Required(
            validationSummaryJson,
            nameof(validationSummaryJson));
        State = CatalogueDraftState.Validated;
    }

    public void MarkPublished()
    {
        if (State is not CatalogueDraftState.Validated)
        {
            throw new InvalidOperationException("Only a validated draft can be published.");
        }

        State = CatalogueDraftState.Published;
    }

    public void Abandon()
    {
        if (State is CatalogueDraftState.Published or CatalogueDraftState.Abandoned)
        {
            throw new InvalidOperationException("The catalogue draft is already closed.");
        }

        State = CatalogueDraftState.Abandoned;
    }
}
