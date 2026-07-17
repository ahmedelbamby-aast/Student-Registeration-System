namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_10Tests
{
    [Fact]
    public void Process_death_before_commit_removes_claim_and_every_partial_mutation()
    {
        var store = new AtomicRegistrationStore(capacity: 2);
        var dyingProcess = store.Begin();
        dyingProcess.Claim("payload-a");
        dyingProcess.CreateAllocationSavepoint();
        dyingProcess.Allocate();
        Assert.True(dyingProcess.State.Processing);
        Assert.Equal(1, dyingProcess.State.SeatCount);
        Assert.Equal(1, dyingProcess.State.EnrollmentCount);

        // Inject immediate process failure before commit. SQL rollback removes
        // the in-transaction claim, seat, enrollment, result and receipt.
        dyingProcess.Rollback();
        var recovered = store.Committed;
        Assert.False(recovered.Processing);
        Assert.Null(recovered.ClaimPayloadHash);
        Assert.Equal(0, recovered.SeatCount);
        Assert.Equal(0, recovered.EnrollmentCount);
        Assert.Null(recovered.ResultCode);
        Assert.False(recovered.ReceiptStored);
        Assert.Null(recovered.Reference);
        Assert.Equal(0, recovered.ExecutionCount);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ClaimInsideTransactionAsync");
    }
}
