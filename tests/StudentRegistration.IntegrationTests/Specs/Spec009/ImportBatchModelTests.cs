using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class ImportBatchModelTests
{
    private static readonly DateTime AccessedAtUtc =
        new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Import_batch_preserves_target_source_hash_classification_state_errors_and_version()
    {
        var draftId = Guid.NewGuid();
        var import = new ImportBatch(
            Guid.NewGuid(),
            draftId,
            "SRC-DATA-SCIENCE",
            AccessedAtUtc,
            "SHA256:CURRICULUM",
            38,
            ImportBatchState.Uploaded);

        Assert.Equal(draftId, import.CatalogueDraftId);
        Assert.Equal("SRC-DATA-SCIENCE", import.SourceReference);
        Assert.Equal(AccessedAtUtc, import.AccessedAtUtc);
        Assert.Equal("SHA256:CURRICULUM", import.ContentHash);
        Assert.Equal(38, import.SyntheticFieldCount);
        Assert.Equal(ImportBatchState.Uploaded, import.State);
        Assert.Empty(import.Errors);
        Assert.Empty(import.Version);

        import.MarkInvalid([new ImportRowError(12, "prerequisite", "MISSING_REFERENCE", "IN321 is missing.")]);
        Assert.Equal(ImportBatchState.Invalid, import.State);
        Assert.Single(import.Errors);
    }

    [Fact]
    public void Import_batch_requires_complete_utc_source_metadata_and_valid_counts()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(draftId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(sourceReference: " "));
        Assert.Throws<ArgumentException>(() => Create(contentHash: " "));
        Assert.Throws<ArgumentException>(() => Create(
            accessedAtUtc: DateTime.SpecifyKind(AccessedAtUtc, DateTimeKind.Local)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(syntheticFieldCount: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(state: (ImportBatchState)999));
    }

    private static ImportBatch Create(
        Guid? id = null,
        Guid? draftId = null,
        string sourceReference = "SRC-DATA-SCIENCE",
        DateTime? accessedAtUtc = null,
        string contentHash = "SHA256:CURRICULUM",
        int syntheticFieldCount = 38,
        ImportBatchState state = ImportBatchState.Uploaded) =>
        new(
            id ?? Guid.NewGuid(),
            draftId ?? Guid.NewGuid(),
            sourceReference,
            accessedAtUtc ?? AccessedAtUtc,
            contentHash,
            syntheticFieldCount,
            state);
}
