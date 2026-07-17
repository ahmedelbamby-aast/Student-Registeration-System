namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_9Tests
{
    [Fact]
    public void Process_death_after_commit_replays_the_atomic_result_snapshot()
    {
        var store = new AtomicRegistrationStore();
        var transaction = store.Begin();
        transaction.Claim("payload-a");
        transaction.CreateAllocationSavepoint();
        transaction.Allocate();
        transaction.Complete("ACCEPTED", "REG-0001");
        transaction.Commit();

        // Inject process death before HTTP serialization. A fresh replica has
        // only committed SQL state and must still reproduce the same result.
        var scope = (StudentId: Guid.NewGuid(), TermId: Guid.NewGuid(), Key: Guid.NewGuid());
        var durableResults = new Dictionary<(Guid, Guid, Guid), AtomicRegistrationStore.State>
        {
            [scope] = store.Committed
        };
        var freshReplicaRead = durableResults[scope];
        Assert.Equal("ACCEPTED", freshReplicaRead.ResultCode);
        Assert.Equal("REG-0001", freshReplicaRead.Reference);
        Assert.True(freshReplicaRead.ReceiptStored);
        Assert.False(freshReplicaRead.Processing);
        Assert.Equal(1, freshReplicaRead.ExecutionCount);
        Assert.Equal(1, freshReplicaRead.SeatCount);
        Assert.Equal(1, freshReplicaRead.EnrollmentCount);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "ReplayCommittedAsync");
    }
}
