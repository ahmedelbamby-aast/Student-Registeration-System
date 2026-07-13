namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_6Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-014: activate only after its approved, version-pinned registration command persists atomic idempotency claims/results in SQL Server and publishes the behavior in generated OpenAPI.";

    [Fact(Skip = DeferredReason)]
    public void Retryable_command_enforces_the_complete_durable_idempotency_protocol()
    {
        // Given one owner/scope/key and a server-canonical command payload at a real SQL persistence boundary.
        // When requests claim concurrently, replay the same payload, reuse the key with a different payload, cancel before commit, or lose the response after commit.
        // Then exactly one atomic first claim exists and same-key/same-payload processing or replay follows the declared final-result policy.
        // And a different payload returns 409 IDEMPOTENCY_KEY_REUSED, pre-commit cancellation leaves no effect, and post-commit retry returns the stored result.
        throw new NotImplementedException(
            "Run against the canonical retryable command, durable idempotency record, and generated OpenAPI; an in-memory key set cannot prove AC-6.");
    }
}
