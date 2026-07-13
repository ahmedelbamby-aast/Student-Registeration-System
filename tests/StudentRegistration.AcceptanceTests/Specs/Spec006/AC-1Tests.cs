namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_1Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-014 and the API error runtime: activate only after SPEC-006 T034 is registered in the real StudentRegistration.Api host and an approved, version-pinned SPEC-014 registration endpoint runs against EF Core and SQL Server.";

    [Fact(Skip = DeferredReason)]
    public void Unexpected_database_failure_returns_only_a_generic_safe_correlated_error()
    {
        // Given a real, version-pinned EF-backed endpoint whose database operation throws unexpectedly.
        // When the request traverses the registered API error pipeline.
        // Then the response contains a generic code, safe message, and correlation ID.
        // And it contains no stack trace, SQL, connection information, secret, or internal identifier.
        throw new NotImplementedException(
            "Exercise the real API host and downstream database endpoint; a mock or source inspection cannot prove AC-1.");
    }
}
