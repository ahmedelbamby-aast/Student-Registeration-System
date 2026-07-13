namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

public sealed class EC_3Tests
{
    private const string DeferredReason =
        "Deferred rowversion proof: activate SPEC-010 SectionGroup, SchedulingModelConfiguration, its versioned command in S2CatalogueScheduling, and the SPEC-006 409 STALE_VERSION contract at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Stale_rowversion_returns_409_stale_version_with_current_version_and_does_not_lose_the_committed_update()
    {
        // Given two callers read the same real SectionGroup rowversion and the first commits an update.
        // When the second submits its stale expected version.
        // Then it receives 409 STALE_VERSION with the current version and the first update remains unchanged.
        throw new NotImplementedException(
            "Use two SQL-backed command scopes and the real API error mapper; comparing byte arrays in memory cannot prove optimistic concurrency.");
    }
}
