namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_3Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-008 and the OpenAPI runtime: activate only after its real context handlers are approved and version-pinned and SPEC-006 T049-T051 generate and enforce their semantic OpenAPI baseline.";

    [Fact(Skip = DeferredReason)]
    public void Approved_success_and_error_contracts_match_real_http_responses_and_generated_openapi()
    {
        // Given approved, version-pinned endpoint contracts and their generated OpenAPI operations.
        // When integration responses and the semantic OpenAPI drift gate are evaluated.
        // Then every documented success and error status uses exactly the approved response shape.
        // And unapproved operation, schema, status-code, or security drift is rejected.
        throw new NotImplementedException(
            "Run against real downstream handlers and generated OpenAPI after T049-T051; a hand-authored or empty document cannot prove AC-3.");
    }
}
