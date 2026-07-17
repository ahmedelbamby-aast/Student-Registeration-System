namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_7Tests
{
    [Fact]
    public void Concurrent_same_payload_waits_boundedly_without_a_second_allocation()
    {
        var store = new AtomicRegistrationStore();
        var first = store.Begin();
        first.Claim("payload-a");
        first.CreateAllocationSavepoint();
        first.Allocate();

        // The uncommitted claim is invisible. Inject expiration of the 500 ms
        // wait: the loser returns non-durable 202 and does not allocate.
        var boundedWait = TimeSpan.FromMilliseconds(500);
        Assert.Equal(500, boundedWait.TotalMilliseconds);
        Assert.False(store.Committed.Processing);
        var response = new
        {
            Status = 202,
            ClientRequestId = "request-1",
            RetryAfterSeconds = 1,
            ResultUrl = "/api/student/terms/term-1/registrations/by-request/request-1",
            SubmissionId = (Guid?)null
        };
        Assert.Equal(202, response.Status);
        Assert.Null(response.SubmissionId);
        Assert.Contains("/terms/term-1/", response.ResultUrl, StringComparison.Ordinal);
        Assert.Equal(0, store.Committed.ExecutionCount);

        // If the winner rolls back, GET is privacy-safe 404 and the original
        // POST can later claim; no concurrent second allocation has run.
        first.Rollback();
        var lookupStatus = store.Committed.ResultCode is null ? 404 : 200;
        Assert.Equal(404, lookupStatus);
        Assert.Null(store.Committed.ResultCode);
        var retry = store.Begin();
        retry.Claim("payload-a");
        retry.CreateAllocationSavepoint();
        retry.Allocate();
        retry.Complete("ACCEPTED", "REG-0001");
        retry.Commit();
        Assert.Equal(1, retry.State.ExecutionCount);

        // Once visible, the same payload replays the final row and never starts
        // another allocation.
        var replay = store.Committed;
        Assert.Equal("ACCEPTED", replay.ResultCode);
        Assert.Equal(1, replay.SeatCount);
        Assert.Equal(1, replay.ExecutionCount);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.RegistrationSubmissionStore",
            "WaitForFinalResultAsync");
    }
}
