namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_9Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-007 and SPEC-008: activate only after both contributor contracts and integration fixtures are approved, implemented, and version-pinned and SPEC-008's GET /api/context handler composes them in the real API host.";

    [Fact(Skip = DeferredReason)]
    public void Authenticated_context_is_complete_authorized_and_fails_safe_when_a_contributor_is_missing()
    {
        // Given SPEC-007 supplies the authenticated session/role context and SPEC-008 supplies authoritative time, term, and window context.
        // When GET /api/context composes both contributors for active, expiring, invalid role configuration, no-applicable-term, and contributor-failure cases.
        // Then every FR-10 field is present, roles are authorization-filtered, and serviceState/supportReferencePath are always present.
        // And activeRole equals the one authorized role, combined roles fail closed, teaching/registration terms may be independently authoritative nulls, and a missing contributor returns a safe unavailable error with no partial context.
        throw new NotImplementedException(
            "Run SPEC-007/SPEC-008 integration through SPEC-008's real handler; hand-composed DTOs cannot prove AC-9.");
    }
}
