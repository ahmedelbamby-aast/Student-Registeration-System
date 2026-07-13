namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_7Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-008: activate only after its GET /api/public/context handler and authoritative academic-context contributor runtime are approved, implemented in the real API host, and version-pinned to SPEC-006 PublicContextDto.";

    [Fact(Skip = DeferredReason)]
    public void Public_context_returns_only_the_approved_privacy_safe_fields()
    {
        // Given an unauthenticated visitor and SPEC-008's authoritative public-context contribution.
        // When GET /api/public/context succeeds through the real API host.
        // Then the response contains only server time/timezone, public term labels, registration-window state, and service state.
        // And it excludes user, role, student, capacity, internal-health, credential, and other personal or internal data.
        throw new NotImplementedException(
            "Exercise SPEC-008's real unauthenticated handler and contributor; constructing PublicContextDto directly cannot prove AC-7.");
    }
}
