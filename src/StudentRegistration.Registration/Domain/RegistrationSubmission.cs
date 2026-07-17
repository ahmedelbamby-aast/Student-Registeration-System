namespace StudentRegistration.Registration.Domain;

public enum RegistrationSubmissionState
{
    Processing = 1,
    Accepted = 2,
    Rejected = 3,
}

public sealed class RegistrationSubmission
{
    private RegistrationSubmission()
    {
    }

    public RegistrationSubmission(
        Guid id,
        Guid studentId,
        Guid termId,
        Guid clientRequestId,
        string payloadHash,
        DateTime receivedAtUtc)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(studentId, nameof(studentId));
        RegistrationPlanDomainGuard.Identifier(termId, nameof(termId));
        RegistrationPlanDomainGuard.Identifier(clientRequestId, nameof(clientRequestId));
        EnsureUtc(receivedAtUtc, nameof(receivedAtUtc));

        Id = id;
        StudentId = studentId;
        TermId = termId;
        ClientRequestId = clientRequestId;
        PayloadHash = RegistrationPlanDomainGuard.Required(
            payloadHash,
            nameof(payloadHash));
        ProcessingState = RegistrationSubmissionState.Processing;
        ReceivedAtUtc = receivedAtUtc;
        UpdatedAtUtc = receivedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid StudentId { get; private set; }

    public Guid TermId { get; private set; }

    public Guid ClientRequestId { get; private set; }

    public string PayloadHash { get; private set; } = string.Empty;

    public RegistrationSubmissionState ProcessingState { get; private set; }

    public string? ResultCode { get; private set; }

    public string? Reference { get; private set; }

    public string? ReceiptSnapshotJson { get; private set; }

    public string? DecisionSnapshotJson { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public bool IsFinal =>
        ProcessingState is RegistrationSubmissionState.Accepted
            or RegistrationSubmissionState.Rejected;

    public void CompleteAccepted(
        string resultCode,
        string reference,
        string receiptSnapshotJson,
        string decisionSnapshotJson,
        DateTime completedAtUtc)
    {
        EnsureProcessing();
        var normalizedResultCode = RegistrationPlanDomainGuard.Required(
            resultCode,
            nameof(resultCode));
        var normalizedReference = RegistrationPlanDomainGuard.Required(
            reference,
            nameof(reference));
        var normalizedReceipt = RegistrationPlanDomainGuard.Required(
            receiptSnapshotJson,
            nameof(receiptSnapshotJson));
        var normalizedDecision = RegistrationPlanDomainGuard.Required(
            decisionSnapshotJson,
            nameof(decisionSnapshotJson));
        EnsureCompletionTime(completedAtUtc);

        ProcessingState = RegistrationSubmissionState.Accepted;
        ResultCode = normalizedResultCode;
        Reference = normalizedReference;
        ReceiptSnapshotJson = normalizedReceipt;
        DecisionSnapshotJson = normalizedDecision;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    public void CompleteRejected(
        string resultCode,
        string decisionSnapshotJson,
        DateTime completedAtUtc)
    {
        EnsureProcessing();
        var normalizedResultCode = RegistrationPlanDomainGuard.Required(
            resultCode,
            nameof(resultCode));
        var normalizedDecision = RegistrationPlanDomainGuard.Required(
            decisionSnapshotJson,
            nameof(decisionSnapshotJson));
        EnsureCompletionTime(completedAtUtc);

        ProcessingState = RegistrationSubmissionState.Rejected;
        ResultCode = normalizedResultCode;
        DecisionSnapshotJson = normalizedDecision;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    public void EnsureFinalForCommit()
    {
        if (!IsFinal)
        {
            throw new InvalidOperationException(
                "A processing submission cannot be committed as a final result.");
        }
    }

    private void EnsureProcessing()
    {
        if (ProcessingState is not RegistrationSubmissionState.Processing)
        {
            throw new InvalidOperationException(
                "A final registration submission cannot be changed.");
        }
    }

    private void EnsureCompletionTime(DateTime completedAtUtc)
    {
        EnsureUtc(completedAtUtc, nameof(completedAtUtc));
        if (completedAtUtc < ReceivedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedAtUtc),
                "Completion cannot precede receipt.");
        }
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", parameterName);
        }
    }
}
