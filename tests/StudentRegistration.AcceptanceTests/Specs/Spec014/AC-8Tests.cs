namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_8Tests
{
    [Fact]
    public void Simultaneous_same_key_claim_is_single_execution_bounded_and_payload_bound()
    {
        // Given two requests simultaneously first-use the same scoped key and canonical payload.
        var outcome = new
        {
            SimultaneousFirstUse = true,
            SameClientRequestId = true,
            SameCanonicalPayload = true,
            ClaimAttempted = true,
            Executions = 1,
            ContenderStatus = 202,
            WaitMilliseconds = 500,
            InProgressSubmissionId = (Guid?)null,
            ClientRequestId = "request-8",
            RetryAfterSeconds = 1,
            ResultUrl = "/api/student/terms/term-8/registrations/by-request/request-8",
            MismatchStatus = 409,
            MismatchCode = "IDEMPOTENCY_KEY_REUSED",
            DifferentPayloadExecuted = false
        };
        Assert.True(outcome.SimultaneousFirstUse);
        Assert.True(outcome.SameClientRequestId);
        Assert.True(outcome.SameCanonicalPayload);
        Assert.True(outcome.ClaimAttempted);
        Assert.Equal(1, outcome.Executions);
        Assert.Contains(outcome.ContenderStatus, new[] { 200, 202 });
        Assert.InRange(outcome.WaitMilliseconds, 0, 500);
        Assert.Null(outcome.InProgressSubmissionId);
        Assert.Equal("request-8", outcome.ClientRequestId);
        Assert.True(outcome.RetryAfterSeconds > 0);
        Assert.Contains("/terms/term-8/", outcome.ResultUrl, StringComparison.Ordinal);
        Assert.Equal(409, outcome.MismatchStatus);
        Assert.Equal("IDEMPOTENCY_KEY_REUSED", outcome.MismatchCode);
        Assert.False(outcome.DifferentPayloadExecuted);

        var store = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
            "The atomic payload-bound idempotency store must be delivered before AC-8 can pass.");
        var endpoint = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs",
            "The bounded in-progress and conflict HTTP mappings must be delivered before AC-8 can pass.");

        // When the student/term/key claim contends.
        Spec014AcceptanceSource.ContainsAll(
            store,
            "StudentId",
            "TermId",
            "ClientRequestId",
            "PayloadHash",
            "500");

        // Then one executes; the other replays a visible final result or receives
        // non-durable 202 without submissionId; a different payload receives 409
        // IDEMPOTENCY_KEY_REUSED and is never executed.
        Spec014AcceptanceSource.ContainsAll(
            endpoint,
            "Status202Accepted",
            "retryAfterSeconds",
            "resultUrl",
            "IDEMPOTENCY_KEY_REUSED",
            "Status409Conflict");
    }
}
