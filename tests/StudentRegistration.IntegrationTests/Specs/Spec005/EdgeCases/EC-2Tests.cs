namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

public sealed class EC_2Tests
{
    private const string DeferredReason =
        "Deferred catalogue SQL/application proof: activate SPEC-009 ImportBatch, CoursePrerequisite, CatalogueModelConfiguration, validation/publish services, and S2CatalogueScheduling with SPEC-006 errors at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Import_with_a_missing_prerequisite_is_rejected_in_preview_and_cannot_be_published()
    {
        // Given an imported course row that references a prerequisite absent from its catalogue version.
        // When the real validation preview and publish boundary execute.
        // Then the row has a stable validation failure and the draft remains unpublished without partial catalogue writes.
        throw new NotImplementedException(
            "Run the SPEC-009 import preview and publish transaction against S2CatalogueScheduling; parsing a static fixture alone is not persistence proof.");
    }
}
