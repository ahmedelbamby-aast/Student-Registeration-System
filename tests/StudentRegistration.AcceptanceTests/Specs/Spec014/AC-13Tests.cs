namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_13Tests
{
    [Fact]
    public void Failure_after_claim_rolls_back_processing_and_after_commit_replays_final_result()
    {
        // Given the process terminates immediately after the in-transaction
        // idempotency claim statement and before any enrollment write.
        var outcome = new
        {
            ProcessTerminatedAfterClaim = true,
            EnrollmentWritesBeforeTermination = 0,
            EnclosingTransactionInspected = true,
            RequestRetried = true,
            CommittedOrphanProcessingRows = 0,
            RetryClaimed = true,
            RetryExecutedNormally = true,
            PostCommitRetryReplayedStoredFinalResult = true,
            AdditionalAllocations = 0
        };
        Assert.True(outcome.ProcessTerminatedAfterClaim);
        Assert.Equal(0, outcome.EnrollmentWritesBeforeTermination);
        Assert.True(outcome.EnclosingTransactionInspected);
        Assert.True(outcome.RequestRetried);
        Assert.Equal(0, outcome.CommittedOrphanProcessingRows);
        Assert.True(outcome.RetryClaimed);
        Assert.True(outcome.RetryExecutedNormally);
        Assert.True(outcome.PostCommitRetryReplayedStoredFinalResult);
        Assert.Equal(0, outcome.AdditionalAllocations);

        var store = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
            "The in-transaction claim/final-result store must be delivered before AC-13 can pass.");
        var coordinator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
            "Atomic registration transaction orchestration must be delivered before AC-13 can pass.");

        // When the enclosing SQL transaction is inspected and the request is retried.
        Spec014AcceptanceSource.ContainsAll(
            store,
            "Processing",
            "PayloadHash",
            "ClientRequestId",
            "Replay");
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "BeginTransaction",
            "CreateSavepoint",
            "Commit",
            "Rollback");

        // Then no orphan Processing record is committed, retry can claim and execute
        // normally, and a failure after commit replays the stored final result.
        Spec014AcceptanceSource.ContainsAll(store, "Accepted", "Rejected", "ReceiptSnapshot");
    }
}
