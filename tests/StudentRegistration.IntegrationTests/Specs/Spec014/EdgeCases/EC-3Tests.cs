namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Lost_response_after_commit_replays_without_compensation()
    {
        var store = new AtomicRegistrationStore();
        var request = store.Begin();
        request.Claim("payload-a");
        request.CreateAllocationSavepoint();
        request.Allocate();
        request.Complete("ACCEPTED", "REG-0001");
        request.Commit();

        // Inject cancellation/network loss after the database commit. Recovery
        // reads the durable final record; it never decrements the valid seat.
        var scope = (StudentId: Guid.NewGuid(), TermId: Guid.NewGuid(), Key: Guid.NewGuid());
        var durableResults = new Dictionary<(Guid, Guid, Guid), AtomicRegistrationStore.State>
        {
            [scope] = store.Committed
        };
        var recovered = durableResults[scope];
        Assert.Equal("ACCEPTED", recovered.ResultCode);
        Assert.Equal("REG-0001", recovered.Reference);
        Assert.True(recovered.ReceiptStored);
        Assert.Equal(1, recovered.SeatCount);
        Assert.Equal(1, recovered.ExecutionCount);
        Assert.Same(recovered, durableResults[scope]);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ReplayAsync");
    }
}
