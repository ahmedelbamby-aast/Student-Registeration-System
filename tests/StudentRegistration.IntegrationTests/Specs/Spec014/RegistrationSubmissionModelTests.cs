using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class RegistrationSubmissionModelTests
{
    [Fact]
    public void Submission_preserves_its_scoped_key_canonical_payload_and_received_time()
    {
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var clientRequestId = Guid.NewGuid();
        var receivedAtUtc = new DateTime(2026, 7, 17, 8, 30, 0, DateTimeKind.Utc);

        var submission = Create(
            studentId: studentId,
            termId: termId,
            clientRequestId: clientRequestId,
            receivedAtUtc: receivedAtUtc);

        Assert.Equal(studentId, submission.StudentId);
        Assert.Equal(termId, submission.TermId);
        Assert.Equal(clientRequestId, submission.ClientRequestId);
        Assert.Equal("sha256:canonical-payload", submission.PayloadHash);
        Assert.Equal(receivedAtUtc, submission.ReceivedAtUtc);
        Assert.Equal(receivedAtUtc, submission.UpdatedAtUtc);
        Assert.Equal(RegistrationSubmissionState.Processing, submission.ProcessingState);
        Assert.False(submission.IsFinal);
        AssertPrivateSetter(nameof(RegistrationSubmission.StudentId));
        AssertPrivateSetter(nameof(RegistrationSubmission.TermId));
        AssertPrivateSetter(nameof(RegistrationSubmission.ClientRequestId));
        AssertPrivateSetter(nameof(RegistrationSubmission.PayloadHash));
        AssertPrivateSetter(nameof(RegistrationSubmission.ReceivedAtUtc));
    }

    [Fact]
    public void Processing_submission_cannot_be_committed_as_a_final_result()
    {
        var submission = Create();

        Assert.Throws<InvalidOperationException>(submission.EnsureFinalForCommit);
        Assert.Null(submission.ResultCode);
        Assert.Null(submission.CompletedAtUtc);
        Assert.Null(submission.Reference);
        Assert.Null(submission.ReceiptSnapshotJson);
    }

    [Fact]
    public void Self_service_submission_can_become_durably_pending_then_accept_only_after_every_line_approves()
    {
        var submission = Create(origin: RegistrationSubmissionOrigin.StudentSelfService, requestedCredits: 6m);
        var first = CreateLine(submission, "CS201");
        var second = CreateLine(submission, "CS202");
        submission.AddLine(first);
        submission.AddLine(second);

        submission.BeginPendingApproval(new DateTime(2026, 7, 17, 8, 31, 0, DateTimeKind.Utc));

        Assert.Equal(RegistrationSubmissionState.PendingApproval, submission.ProcessingState);
        Assert.Equal("PENDING_APPROVAL", submission.ResultCode);
        Assert.Equal(2, submission.Lines.Count);
        Assert.Throws<InvalidOperationException>(() => submission.CompleteAccepted(
            "ACCEPTED", "REG-1", "{}", "{}",
            new DateTime(2026, 7, 17, 8, 32, 0, DateTimeKind.Utc)));

        first.Approve();
        second.Approve();
        submission.CompleteAccepted(
            "ACCEPTED", "REG-1", "{}", "{}",
            new DateTime(2026, 7, 17, 8, 32, 0, DateTimeKind.Utc));

        Assert.Equal(RegistrationSubmissionState.Accepted, submission.ProcessingState);
    }

    [Fact]
    public void Pending_submission_rejects_or_expires_as_one_terminal_plan()
    {
        var rejected = Create(origin: RegistrationSubmissionOrigin.StudentSelfService, requestedCredits: 3m);
        var rejectedLine = CreateLine(rejected, "CS201");
        rejected.AddLine(rejectedLine);
        rejected.BeginPendingApproval(rejected.ReceivedAtUtc.AddMinutes(1));
        rejectedLine.Reject();
        rejected.CompleteRejected("LINE_REJECTED", "{}", rejected.ReceivedAtUtc.AddMinutes(2));

        Assert.Equal(RegistrationSubmissionState.Rejected, rejected.ProcessingState);
        Assert.True(rejected.IsFinal);
        Assert.All(rejected.Lines, line =>
            Assert.NotEqual(RegistrationSubmissionLineState.PendingApproval, line.State));

        var expired = Create(origin: RegistrationSubmissionOrigin.StudentSelfService, requestedCredits: 3m);
        var expiredLine = CreateLine(expired, "CS202");
        expired.AddLine(expiredLine);
        expired.BeginPendingApproval(expired.ReceivedAtUtc.AddMinutes(1));
        expiredLine.Expire();
        expired.CompleteExpired("REGISTRATION_WINDOW_CLOSED", "{}", expired.ReceivedAtUtc.AddMinutes(2));

        Assert.Equal(RegistrationSubmissionState.Expired, expired.ProcessingState);
        Assert.True(expired.IsFinal);
        Assert.All(expired.Lines, line =>
            Assert.NotEqual(RegistrationSubmissionLineState.PendingApproval, line.State));
        Assert.Throws<InvalidOperationException>(() => expired.CompleteRejected(
            "CHANGED", "{}", expired.ReceivedAtUtc.AddMinutes(3)));
    }

    [Fact]
    public void Accepted_result_requires_and_preserves_one_reference_receipt_and_decision()
    {
        var submission = Create();
        var completedAtUtc = new DateTime(2026, 7, 17, 8, 30, 1, DateTimeKind.Utc);

        submission.CompleteAccepted(
            "ACCEPTED",
            "REG-2026-000042",
            "{\"term\":\"2026-FALL\"}",
            "{\"policyVersion\":\"2026.1\"}",
            completedAtUtc);

        Assert.Equal(RegistrationSubmissionState.Accepted, submission.ProcessingState);
        Assert.Equal("ACCEPTED", submission.ResultCode);
        Assert.Equal("REG-2026-000042", submission.Reference);
        Assert.Equal("{\"term\":\"2026-FALL\"}", submission.ReceiptSnapshotJson);
        Assert.Equal(
            "{\"policyVersion\":\"2026.1\"}",
            submission.DecisionSnapshotJson);
        Assert.Equal(completedAtUtc, submission.CompletedAtUtc);
        Assert.Equal(completedAtUtc, submission.UpdatedAtUtc);
        Assert.True(submission.IsFinal);
        submission.EnsureFinalForCommit();
    }

    [Fact]
    public void Final_result_is_atomic_and_replay_immutable()
    {
        var submission = Create();
        var completedAtUtc = new DateTime(2026, 7, 17, 8, 30, 1, DateTimeKind.Utc);
        submission.CompleteRejected(
            "CAPACITY_FULL",
            "{\"groupVersion\":\"group-rv-9\"}",
            completedAtUtc);

        Assert.Throws<InvalidOperationException>(() => submission.CompleteAccepted(
            "ACCEPTED",
            "REG-2026-000043",
            "{\"term\":\"changed\"}",
            "{\"policyVersion\":\"changed\"}",
            completedAtUtc.AddSeconds(1)));

        Assert.Equal(RegistrationSubmissionState.Rejected, submission.ProcessingState);
        Assert.Equal("CAPACITY_FULL", submission.ResultCode);
        Assert.Null(submission.Reference);
        Assert.Null(submission.ReceiptSnapshotJson);
        Assert.Equal(
            "{\"groupVersion\":\"group-rv-9\"}",
            submission.DecisionSnapshotJson);
        Assert.Equal(completedAtUtc, submission.CompletedAtUtc);
    }

    [Fact]
    public void Submission_rejects_invalid_identity_hash_time_and_final_result_values()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(studentId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(clientRequestId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(payloadHash: " "));
        Assert.Throws<ArgumentException>(() => Create(
            receivedAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local)));

        var submission = Create();
        var completedAtUtc = submission.ReceivedAtUtc.AddSeconds(1);
        Assert.Throws<ArgumentException>(() => submission.CompleteAccepted(
            " ", "REG-1", "{}", "{}", completedAtUtc));
        Assert.Throws<ArgumentException>(() => submission.CompleteAccepted(
            "ACCEPTED", " ", "{}", "{}", completedAtUtc));
        Assert.Throws<ArgumentException>(() => submission.CompleteAccepted(
            "ACCEPTED", "REG-1", " ", "{}", completedAtUtc));
        Assert.Throws<ArgumentException>(() => submission.CompleteRejected(
            "REJECTED", " ", completedAtUtc));
        Assert.Throws<ArgumentOutOfRangeException>(() => submission.CompleteRejected(
            "REJECTED", "{}", submission.ReceivedAtUtc.AddTicks(-1)));
    }

    private static RegistrationSubmission Create(
        Guid? id = null,
        Guid? studentId = null,
        Guid? termId = null,
        Guid? clientRequestId = null,
        string payloadHash = "sha256:canonical-payload",
        DateTime? receivedAtUtc = null,
        RegistrationSubmissionOrigin origin = RegistrationSubmissionOrigin.StudentSelfService,
        decimal requestedCredits = 0m) =>
        new(
            id ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            clientRequestId ?? Guid.NewGuid(),
            payloadHash,
            receivedAtUtc ?? new DateTime(2026, 7, 17, 8, 30, 0, DateTimeKind.Utc),
            origin,
            requestedCredits);

    private static RegistrationSubmissionLine CreateLine(
        RegistrationSubmission submission,
        string code) => new(
            Guid.NewGuid(),
            submission.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            code,
            $"{code} title",
            3m);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(RegistrationSubmission).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
