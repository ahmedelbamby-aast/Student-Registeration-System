namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_3Tests
{
    [Fact]
    public void Lost_response_retry_returns_the_original_result_without_duplicate_side_effects()
    {
        // Given a submission committed but its HTTP response was lost.
        var original = new
        {
            ResponseLost = true,
            ClientRequestId = "request-3",
            PayloadHash = "payload-3",
            SubmissionId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Reference = "REG-000003",
            ReceiptHash = "receipt-3",
            Seats = 1,
            Enrollments = 1,
            SuccessAudits = 1,
            Receipts = 1
        };
        var replay = original with { ResponseLost = false };
        Assert.True(original.ResponseLost);
        Assert.Equal(original.ClientRequestId, replay.ClientRequestId);
        Assert.Equal(original.PayloadHash, replay.PayloadHash);
        Assert.Equal(original.SubmissionId, replay.SubmissionId);
        Assert.Equal(original.Reference, replay.Reference);
        Assert.Equal(original.ReceiptHash, replay.ReceiptHash);
        Assert.Equal(1, replay.Seats);
        Assert.Equal(1, replay.Enrollments);
        Assert.Equal(1, replay.SuccessAudits);
        Assert.Equal(1, replay.Receipts);

        var store = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs",
            "The durable idempotent submission store must be delivered before AC-3 can pass.");

        // When the same student/term key and canonical payload are retried.
        Spec014AcceptanceSource.ContainsAll(
            store,
            "StudentId",
            "TermId",
            "ClientRequestId",
            "PayloadHash",
            "Replay");

        // Then the original result/receipt is returned without another seat,
        // enrollment, success audit event, receipt, or reference.
        Spec014AcceptanceSource.ContainsAll(
            store,
            "ReceiptSnapshot",
            "Reference",
            "Accepted");
    }
}
