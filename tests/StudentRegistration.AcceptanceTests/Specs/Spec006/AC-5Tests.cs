namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_5Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-011: activate only after its approved, version-pinned subject-discovery listing endpoint implements the SPEC-006 pagination contract in the real API host under the approved API-version policy.";

    [Fact(Skip = DeferredReason)]
    public void Listing_is_bounded_deterministic_and_governed_for_breaking_changes()
    {
        // Given a real approved listing endpoint and a client request with omitted, valid, or invalid pagination values.
        // When the endpoint validates the page and applies its allow-listed deterministic sort.
        // Then omitted values mean page 1 and size 20, size 100 is allowed, and invalid values return 400 PAGE_SIZE_INVALID without capping.
        // And Page.sort echoes a canonical sort ending in a unique-ID tie-breaker while breaking changes follow the versioning process.
        throw new NotImplementedException(
            "Exercise a version-pinned downstream listing over HTTP; a shared Page type alone cannot prove AC-5.");
    }
}
