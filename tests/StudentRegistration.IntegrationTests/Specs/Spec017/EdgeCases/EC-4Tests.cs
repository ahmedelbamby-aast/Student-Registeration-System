using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec017.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Partially_invalid_import_reports_preview_errors_and_cannot_publish()
    {
        var upstream = new Spec009.ImportBatchModelTests();
        upstream.Import_batch_preserves_target_source_hash_classification_state_errors_and_version();

        var import = new ImportBatch(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SRC-PARTIALLY-INVALID",
            new DateTime(2026, 7, 17, 12, 0, 0, DateTimeKind.Utc),
            "SHA256:PARTIALLY-INVALID",
            0,
            ImportBatchState.Uploaded);
        import.MarkInvalid(
        [
            new ImportRowError(
                2,
                "courseCode",
                "DUPLICATE_CODE",
                "The course code is duplicated."),
            new ImportRowError(
                5,
                "prerequisite",
                "MISSING_REFERENCE",
                "The prerequisite does not exist.")
        ]);

        Assert.Equal(ImportBatchState.Invalid, import.State);
        Assert.Equal(2, import.Errors.Count);
        Assert.Null(import.PublishedVersionId);
        Assert.Throws<InvalidOperationException>(import.MarkPublishing);
        Assert.Equal(ImportBatchState.Invalid, import.State);
        Assert.Null(import.PublishedVersionId);
    }
}
