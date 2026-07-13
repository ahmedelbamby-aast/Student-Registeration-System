namespace StudentRegistration.AcceptanceTests.Specs.Spec006;

public sealed class AC_4Tests
{
    private const string DeferredReason =
        "Deferred to SPEC-012 and SPEC-014: activate only after their approved, version-pinned plan-update and registration mutations run through the real API host with injected TimeProvider, request-body concurrency metadata, idempotency, and SQL Server commit boundaries.";

    [Fact(Skip = DeferredReason)]
    public void Versioned_time_dependent_mutation_is_concurrency_idempotency_and_cancellation_safe()
    {
        // Given an authorized time-dependent mutation with expectedRowVersion and its declared idempotency contract.
        // When the real request is stale, retried, canceled before commit, or loses its response after commit.
        // Then server time comes from TimeProvider and an authorized stale request returns 409 STALE_VERSION.
        // And unauthorized callers learn no version, pre-commit cancellation has no effect, and post-commit retry returns the one stored result.
        throw new NotImplementedException(
            "Exercise an approved versioned mutation through its real SQL transaction; doubles without commit behavior cannot prove AC-4.");
    }
}
