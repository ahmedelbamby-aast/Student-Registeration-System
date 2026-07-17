namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_6Tests
{
    private const string DeferredReason =
        "Deferred real-SQL concurrency proof: activate SPEC-014 RegistrationSubmission, Enrollment, RegistrationModelConfiguration, store/coordinator, and S6Registration while consuming SPEC-008 StudentTermAcademicState through ExecuteRegistrationBoundaryAsync, with SPEC-006 stable 409 semantics at entity-ownership 2.0.8 and persistence-manifest 2.1.2.";

    [Fact(Skip = DeferredReason)]
    public void Shared_boundary_and_idempotency_claim_allow_one_payload_reject_mismatch_with_409_and_replay_after_restart()
    {
        // Given the Code First registration model migrated to SQL Server.
        // When parallel transactions enter the shared StudentTermAcademicState boundary and claim one scoped idempotency key.
        // Then one canonical payload owns the durable result, a different payload receives 409 IDEMPOTENCY_KEY_REUSED,
        // and the same deterministic result is replayed after the application process restarts.
        throw new NotImplementedException(
            "Use two real SQL transactions and restart the application host before replay; an in-memory dictionary or mocked store is not evidence.");
    }
}
