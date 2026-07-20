namespace StudentRegistration.Registration.Domain;

public enum RegistrationSubmissionState
{
    Processing = 1,
    PendingApproval = 2,
    Accepted = 3,
    Rejected = 4,
    Expired = 5,
}

public enum RegistrationSubmissionOrigin
{
    StudentSelfService = 1,
    FirstTermAutomatic = 2,
}

public sealed class RegistrationSubmission
{
    private readonly List<RegistrationSubmissionLine> _lines = [];

    private RegistrationSubmission()
    {
    }

    public RegistrationSubmission(
        Guid id,
        Guid studentId,
        Guid termId,
        Guid clientRequestId,
        string payloadHash,
        DateTime receivedAtUtc,
        RegistrationSubmissionOrigin origin = RegistrationSubmissionOrigin.StudentSelfService,
        decimal requestedCredits = 0m)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(studentId, nameof(studentId));
        RegistrationPlanDomainGuard.Identifier(termId, nameof(termId));
        RegistrationPlanDomainGuard.Identifier(clientRequestId, nameof(clientRequestId));
        EnsureUtc(receivedAtUtc, nameof(receivedAtUtc));
        RegistrationPlanDomainGuard.Defined(origin, nameof(origin));
        if (requestedCredits < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedCredits));
        }

        Id = id;
        StudentId = studentId;
        TermId = termId;
        ClientRequestId = clientRequestId;
        PayloadHash = RegistrationPlanDomainGuard.Required(
            payloadHash,
            nameof(payloadHash));
        Origin = origin;
        RequestedCredits = requestedCredits;
        ProcessingState = RegistrationSubmissionState.Processing;
        ReceivedAtUtc = receivedAtUtc;
        UpdatedAtUtc = receivedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid StudentId { get; private set; }

    public Guid TermId { get; private set; }

    public Guid ClientRequestId { get; private set; }

    public string PayloadHash { get; private set; } = string.Empty;

    public RegistrationSubmissionOrigin Origin { get; private set; }

    public decimal RequestedCredits { get; private set; }

    public RegistrationSubmissionState ProcessingState { get; private set; }

    public string? ResultCode { get; private set; }

    public string? Reference { get; private set; }

    public string? ReceiptSnapshotJson { get; private set; }

    public string? DecisionSnapshotJson { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyList<RegistrationSubmissionLine> Lines => _lines;

    public bool IsFinal =>
        ProcessingState is RegistrationSubmissionState.Accepted
            or RegistrationSubmissionState.Rejected
            or RegistrationSubmissionState.Expired;

    public void AddLine(RegistrationSubmissionLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        EnsureProcessing();
        if (line.SubmissionId != Id)
        {
            throw new ArgumentException("The line must belong to this submission.", nameof(line));
        }

        if (_lines.Any(existing =>
            existing.Id == line.Id || existing.OfferingId == line.OfferingId))
        {
            throw new ArgumentException(
                "Submission line and offering identifiers must be unique.",
                nameof(line));
        }

        _lines.Add(line);
    }

    public void BeginPendingApproval(DateTime updatedAtUtc)
    {
        EnsureProcessing();
        EnsureUpdateTime(updatedAtUtc);
        if (Origin is not RegistrationSubmissionOrigin.StudentSelfService)
        {
            throw new InvalidOperationException(
                "First-term automatic registration does not use approval holds.");
        }

        if (_lines.Count == 0 || _lines.Any(line =>
            line.State is not RegistrationSubmissionLineState.PendingApproval))
        {
            throw new InvalidOperationException(
                "A pending submission requires at least one undecided line.");
        }

        if (RequestedCredits <= 0m || _lines.Sum(line => line.Credits) != RequestedCredits)
        {
            throw new InvalidOperationException(
                "Requested credits must equal the authoritative submission lines.");
        }

        ProcessingState = RegistrationSubmissionState.PendingApproval;
        ResultCode = "PENDING_APPROVAL";
        UpdatedAtUtc = updatedAtUtc;
    }

    public void CompleteAccepted(
        string resultCode,
        string reference,
        string receiptSnapshotJson,
        string decisionSnapshotJson,
        DateTime completedAtUtc)
    {
        EnsureCompletable();
        if (ProcessingState is RegistrationSubmissionState.PendingApproval &&
            (_lines.Count == 0 || _lines.Any(line =>
                line.State is not RegistrationSubmissionLineState.Approved)))
        {
            throw new InvalidOperationException(
                "Every submission line must be approved before acceptance.");
        }
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
        EnsureCompletable();
        var normalizedResultCode = RegistrationPlanDomainGuard.Required(
            resultCode,
            nameof(resultCode));
        var normalizedDecision = RegistrationPlanDomainGuard.Required(
            decisionSnapshotJson,
            nameof(decisionSnapshotJson));
        EnsureCompletionTime(completedAtUtc);
        if (ProcessingState is RegistrationSubmissionState.PendingApproval)
        {
            if (_lines.All(line => line.State is not RegistrationSubmissionLineState.Rejected))
            {
                throw new InvalidOperationException(
                    "A pending submission requires a rejected line before plan rejection.");
            }

            foreach (var line in _lines.Where(line =>
                line.State is RegistrationSubmissionLineState.PendingApproval))
            {
                line.Reject();
            }
        }

        ProcessingState = RegistrationSubmissionState.Rejected;
        ResultCode = normalizedResultCode;
        DecisionSnapshotJson = normalizedDecision;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    public void CompleteExpired(
        string resultCode,
        string decisionSnapshotJson,
        DateTime completedAtUtc)
    {
        if (ProcessingState is not RegistrationSubmissionState.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only a pending submission can expire.");
        }

        var normalizedResultCode = RegistrationPlanDomainGuard.Required(
            resultCode,
            nameof(resultCode));
        var normalizedDecision = RegistrationPlanDomainGuard.Required(
            decisionSnapshotJson,
            nameof(decisionSnapshotJson));
        EnsureCompletionTime(completedAtUtc);
        foreach (var line in _lines.Where(line =>
            line.State is RegistrationSubmissionLineState.PendingApproval))
        {
            line.Expire();
        }

        if (_lines.Any(line => line.State is RegistrationSubmissionLineState.PendingApproval
            or RegistrationSubmissionLineState.Rejected))
        {
            throw new InvalidOperationException("Expired plans cannot contain pending or rejected lines.");
        }

        ProcessingState = RegistrationSubmissionState.Expired;
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

    private void EnsureCompletable()
    {
        if (ProcessingState is not RegistrationSubmissionState.Processing &&
            ProcessingState is not RegistrationSubmissionState.PendingApproval)
        {
            throw new InvalidOperationException(
                "A final registration submission cannot be changed.");
        }
    }

    private void EnsureUpdateTime(DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        if (updatedAtUtc < UpdatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAtUtc),
                "An update cannot precede the current submission state.");
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
