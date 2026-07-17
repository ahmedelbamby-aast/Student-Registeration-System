using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec015;

public sealed class RegistrationReceiptModelTests
{
    [Fact]
    public void Accepted_submission_projects_one_immutable_receipt_without_rewriting_snapshot()
    {
        var receivedAtUtc = new DateTime(2026, 7, 17, 9, 0, 0, DateTimeKind.Utc);
        var completedAtUtc = receivedAtUtc.AddSeconds(1);
        const string receiptSnapshot =
            """
            {"term":{"code":"2026-FALL"},"groups":[{"groupCode":"G01","roomCode":"C-101"}],"policyVersion":"DEMO-POC-2026.1","submittedAtUtc":"2026-07-17T09:00:00Z"}
            """;
        const string decisionSnapshot =
            """
            {"policyVersion":"DEMO-POC-2026.1","resultCode":"ACCEPTED"}
            """;
        var submission = new RegistrationSubmission(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "sha256:canonical",
            receivedAtUtc);
        submission.CompleteAccepted(
            "ACCEPTED",
            "REG-2026-000001",
            receiptSnapshot,
            decisionSnapshot,
            completedAtUtc);

        var receipt = RegistrationReceipt.From(submission);

        Assert.Equal(submission.Id, receipt.SubmissionId);
        Assert.Equal("REG-2026-000001", receipt.Reference);
        Assert.Equal("ACCEPTED", receipt.ResultCode);
        Assert.Equal(receiptSnapshot, receipt.ReceiptSnapshotJson);
        Assert.Equal(decisionSnapshot, receipt.DecisionSnapshotJson);
        Assert.Equal(receivedAtUtc, receipt.ReceivedAtUtc);
        Assert.Equal(completedAtUtc, receipt.CompletedAtUtc);

        using var snapshot = JsonDocument.Parse(receipt.ReceiptSnapshotJson);
        Assert.Equal(
            "2026-FALL",
            snapshot.RootElement.GetProperty("term").GetProperty("code").GetString());
        Assert.Equal(
            "C-101",
            snapshot.RootElement.GetProperty("groups")[0].GetProperty("roomCode").GetString());

        Assert.True(typeof(RegistrationReceipt).IsSealed);
        Assert.All(
            typeof(RegistrationReceipt).GetProperties(),
            property => Assert.False(property.SetMethod?.IsPublic == true));
    }

    [Fact]
    public void Processing_or_rejected_submission_cannot_be_projected_as_a_receipt()
    {
        var receivedAtUtc = new DateTime(2026, 7, 17, 9, 0, 0, DateTimeKind.Utc);
        var processing = NewSubmission(receivedAtUtc);
        var rejected = NewSubmission(receivedAtUtc);
        rejected.CompleteRejected(
            "GROUP_FULL",
            "{\"policyVersion\":\"DEMO-POC-2026.1\"}",
            receivedAtUtc.AddSeconds(1));

        Assert.Throws<InvalidOperationException>(() => RegistrationReceipt.From(processing));
        Assert.Throws<InvalidOperationException>(() => RegistrationReceipt.From(rejected));
    }

    private static RegistrationSubmission NewSubmission(DateTime receivedAtUtc) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "sha256:canonical",
            receivedAtUtc);
}
