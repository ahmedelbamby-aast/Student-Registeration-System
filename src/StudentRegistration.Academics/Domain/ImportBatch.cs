namespace StudentRegistration.Academics.Domain;

public enum ImportBatchState
{
    Uploaded = 1,
    Validating = 2,
    Invalid = 3,
    Validated = 4,
    Publishing = 5,
    Published = 6,
    Failed = 7,
}

public sealed record ImportRowError
{
    public ImportRowError(
        int? sourceRow,
        string? field,
        string code,
        string message)
    {
        if (sourceRow is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceRow));
        }

        SourceRow = sourceRow;
        Field = string.IsNullOrWhiteSpace(field) ? null : field.Trim();
        Code = DomainValue.Code(code, nameof(code));
        Message = DomainValue.Required(message, nameof(message));
    }

    public int? SourceRow { get; }

    public string? Field { get; }

    public string Code { get; }

    public string Message { get; }
}

public sealed class ImportBatch
{
    private ImportBatch()
    {
    }

    public ImportBatch(
        Guid id,
        Guid catalogueDraftId,
        string sourceReference,
        DateTime accessedAtUtc,
        string contentHash,
        int syntheticFieldCount,
        ImportBatchState state)
    {
        DomainValue.Identifier(id, nameof(id));
        DomainValue.Identifier(catalogueDraftId, nameof(catalogueDraftId));
        DomainValue.Utc(accessedAtUtc, nameof(accessedAtUtc));
        if (syntheticFieldCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(syntheticFieldCount));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        Id = id;
        CatalogueDraftId = catalogueDraftId;
        SourceReference = DomainValue.Required(sourceReference, nameof(sourceReference));
        AccessedAtUtc = accessedAtUtc;
        ContentHash = DomainValue.Required(contentHash, nameof(contentHash));
        SyntheticFieldCount = syntheticFieldCount;
        State = state;
    }

    public Guid Id { get; private set; }

    public Guid CatalogueDraftId { get; private set; }

    public string SourceReference { get; private set; } = string.Empty;

    public DateTime AccessedAtUtc { get; private set; }

    public string ContentHash { get; private set; } = string.Empty;

    public int SyntheticFieldCount { get; private set; }

    public ImportBatchState State { get; private set; }

    public IReadOnlyList<ImportRowError> Errors { get; private set; } = [];

    public Guid? PublishedVersionId { get; private set; }

    public byte[] Version { get; private set; } = [];

    public void MarkInvalid(IEnumerable<ImportRowError> errors)
    {
        var materialized = (errors ?? throw new ArgumentNullException(nameof(errors))).ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("At least one validation error is required.", nameof(errors));
        }

        if (State is not (ImportBatchState.Uploaded or ImportBatchState.Validating))
        {
            throw new InvalidOperationException("Only an uploaded or validating import can be invalidated.");
        }

        Errors = materialized;
        State = ImportBatchState.Invalid;
    }

    public void MarkValidated()
    {
        if (State is not (ImportBatchState.Uploaded or ImportBatchState.Validating))
        {
            throw new InvalidOperationException("Only an uploaded or validating import can be validated.");
        }

        Errors = [];
        State = ImportBatchState.Validated;
    }

    public void MarkPublishing()
    {
        if (State is not ImportBatchState.Validated)
        {
            throw new InvalidOperationException("Only a validated import can publish.");
        }

        State = ImportBatchState.Publishing;
    }

    public void MarkPublished(Guid publishedVersionId)
    {
        DomainValue.Identifier(publishedVersionId, nameof(publishedVersionId));
        if (State is not ImportBatchState.Publishing)
        {
            throw new InvalidOperationException("Only a publishing import can complete.");
        }

        PublishedVersionId = publishedVersionId;
        State = ImportBatchState.Published;
    }

    public void MarkFailed()
    {
        if (State is ImportBatchState.Published)
        {
            throw new InvalidOperationException("A published import cannot fail.");
        }

        State = ImportBatchState.Failed;
    }
}
