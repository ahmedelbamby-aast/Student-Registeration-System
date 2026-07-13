namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_8Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-008 and SPEC-014: activate only after their representative approved, version-pinned context query and registration command handlers delegate to focused module services in the real API host and consume the registered SPEC-006 T015 JSON policy.";

    [Fact(Skip = DeferredReason)]
    public void Real_endpoints_delegate_business_decisions_and_share_one_consistent_json_policy()
    {
        // Given representative approved command and query handlers with focused module-owned application services.
        // When real HTTP requests execute through the API composition root.
        // Then endpoints delegate business decisions rather than implementing them in transport code.
        // And camelCase names, documented enum strings, UTC timestamps, timezone identifiers, invariant decimals, and nullable fields serialize consistently.
        throw new NotImplementedException(
            "Exercise representative version-pinned handlers through the registered JSON policy; static contract serialization alone cannot prove AC-8.");
    }
}
