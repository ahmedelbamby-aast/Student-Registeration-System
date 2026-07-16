namespace StudentRegistration.Academics.Domain;

public enum CatalogueVersionState
{
    Published = 1,
    Superseded = 2,
}

public sealed class CatalogueVersion
{
    private CatalogueVersion()
    {
    }

    public CatalogueVersion(
        Guid id,
        Guid sourceDraftId,
        Guid? supersedesId,
        string scopeCode,
        string versionCode,
        string sourceReference,
        DateTime effectiveFromUtc,
        DateTime publishedAtUtc,
        string publishedBy,
        CatalogueVersionState state)
    {
        DomainValue.Identifier(id, nameof(id));
        DomainValue.Identifier(sourceDraftId, nameof(sourceDraftId));
        if (supersedesId == Guid.Empty)
        {
            throw new ArgumentException(
                "A superseded version identifier cannot be empty.",
                nameof(supersedesId));
        }

        DomainValue.Utc(effectiveFromUtc, nameof(effectiveFromUtc));
        DomainValue.Utc(publishedAtUtc, nameof(publishedAtUtc));
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        Id = id;
        SourceDraftId = sourceDraftId;
        SupersedesId = supersedesId;
        ScopeCode = DomainValue.Code(scopeCode, nameof(scopeCode));
        VersionCode = DomainValue.Required(versionCode, nameof(versionCode));
        SourceReference = DomainValue.Required(sourceReference, nameof(sourceReference));
        EffectiveFromUtc = effectiveFromUtc;
        PublishedAtUtc = publishedAtUtc;
        PublishedBy = DomainValue.Required(publishedBy, nameof(publishedBy));
        State = state;
    }

    public Guid Id { get; private set; }

    public Guid SourceDraftId { get; private set; }

    public Guid? SupersedesId { get; private set; }

    public string ScopeCode { get; private set; } = string.Empty;

    public string VersionCode { get; private set; } = string.Empty;

    public string SourceReference { get; private set; } = string.Empty;

    public DateTime EffectiveFromUtc { get; private set; }

    public DateTime PublishedAtUtc { get; private set; }

    public string PublishedBy { get; private set; } = string.Empty;

    public CatalogueVersionState State { get; private set; }

    public byte[] Version { get; private set; } = [];

    public void MarkSuperseded()
    {
        if (State is not CatalogueVersionState.Published)
        {
            throw new InvalidOperationException("Only a published catalogue version can be superseded.");
        }

        State = CatalogueVersionState.Superseded;
    }
}
