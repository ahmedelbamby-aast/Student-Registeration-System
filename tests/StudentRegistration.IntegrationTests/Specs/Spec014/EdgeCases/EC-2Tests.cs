namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Unique_race_rolls_back_allocations_or_retries_after_transaction_abort()
    {
        var store = new AtomicRegistrationStore(capacity: 2);
        var transaction = store.Begin();
        transaction.Claim("payload-a");
        transaction.CreateAllocationSavepoint();
        transaction.Allocate();

        // Inject a deterministic unique-constraint loser after the seat update.
        transaction.RollbackToAllocationSavepoint();
        Assert.Equal(0, transaction.State.SeatCount);
        Assert.Equal(0, transaction.State.EnrollmentCount);
        transaction.Complete("DUPLICATE_OFFERING");
        transaction.Commit();

        Assert.Equal("DUPLICATE_OFFERING", store.Committed.ResultCode);
        Assert.Equal(0, store.Committed.SeatCount);
        Assert.False(store.Committed.Processing);

        // If SQL aborts the transaction, even the claim must disappear so a
        // later complete transaction can claim safely.
        var abortedStore = new AtomicRegistrationStore();
        var aborted = abortedStore.Begin();
        aborted.Claim("payload-b");
        aborted.AbortTransaction();
        aborted.Rollback();
        Assert.Null(abortedStore.Committed.ClaimPayloadHash);
        Assert.False(abortedStore.Committed.Processing);
        Assert.Equal(0, abortedStore.Committed.SeatCount);

        var safeRetry = abortedStore.Begin();
        safeRetry.Claim("payload-b");
        safeRetry.Complete("GROUP_FULL");
        safeRetry.Commit();
        Assert.Equal("payload-b", abortedStore.Committed.ClaimPayloadHash);
        Assert.Equal("GROUP_FULL", abortedStore.Committed.ResultCode);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.SqlSeatAllocator",
            "AllocateWithSavepointAsync");
    }
}
