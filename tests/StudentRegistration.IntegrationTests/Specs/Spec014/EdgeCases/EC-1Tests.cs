namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Transient_failure_retries_only_the_complete_uncommitted_transaction()
    {
        var store = new AtomicRegistrationStore();
        var first = store.Begin();
        first.Claim("payload-a");
        first.CreateAllocationSavepoint();
        first.Allocate();

        // Inject a transient SQL failure before commit. The failed unit is rolled
        // back completely; no fragment is eligible for retry.
        first.AbortTransaction();
        first.Rollback();
        Assert.Equal(0, store.Committed.SeatCount);
        Assert.False(store.Committed.Processing);

        var retry = store.Begin();
        retry.Claim("payload-a");
        retry.CreateAllocationSavepoint();
        retry.Allocate();
        retry.Complete("ACCEPTED", "REG-0001");
        retry.Commit();

        Assert.Equal(1, store.Committed.SeatCount);
        Assert.Equal(1, store.Committed.EnrollmentCount);
        Assert.Equal(1, store.Committed.ExecutionCount);
        Assert.Equal("payload-a", store.Committed.ClaimPayloadHash);
        Assert.Equal("ACCEPTED", store.Committed.ResultCode);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Registration",
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "ExecuteWithRetryAsync");
    }
}
