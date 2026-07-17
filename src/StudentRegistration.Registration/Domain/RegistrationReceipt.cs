namespace StudentRegistration.Registration.Domain;

/// <summary>
/// Immutable read projection over an accepted canonical registration submission.
/// It is not an independently persisted aggregate or EF entity.
/// </summary>
public sealed record RegistrationReceipt
{
    public RegistrationReceipt(
        Guid submissionId,
        string reference,
        string resultCode,
        string receiptSnapshotJson,
        string decisionSnapshotJson,
        DateTime receivedAtUtc,
        DateTime completedAtUtc)
    {
        RegistrationPlanDomainGuard.Identifier(submissionId, nameof(submissionId));
        var normalizedReceivedAtUtc = NormalizeUtc(receivedAtUtc, nameof(receivedAtUtc));
        var normalizedCompletedAtUtc = NormalizeUtc(completedAtUtc, nameof(completedAtUtc));
        if (normalizedCompletedAtUtc < normalizedReceivedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedAtUtc),
                "Completion cannot precede receipt.");
        }

        SubmissionId = submissionId;
        Reference = RegistrationPlanDomainGuard.Required(reference, nameof(reference));
        ResultCode = RegistrationPlanDomainGuard.Required(resultCode, nameof(resultCode));
        ReceiptSnapshotJson = RegistrationPlanDomainGuard.Required(
            receiptSnapshotJson,
            nameof(receiptSnapshotJson));
        DecisionSnapshotJson = RegistrationPlanDomainGuard.Required(
            decisionSnapshotJson,
            nameof(decisionSnapshotJson));
        ReceivedAtUtc = normalizedReceivedAtUtc;
        CompletedAtUtc = normalizedCompletedAtUtc;
    }

    public Guid SubmissionId { get; }

    public string Reference { get; }

    public string ResultCode { get; }

    public string ReceiptSnapshotJson { get; }

    public string DecisionSnapshotJson { get; }

    public DateTime ReceivedAtUtc { get; }

    public DateTime CompletedAtUtc { get; }

    public static RegistrationReceipt From(RegistrationSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);
        if (submission.ProcessingState is not RegistrationSubmissionState.Accepted ||
            submission.Reference is null ||
            submission.ResultCode is null ||
            submission.ReceiptSnapshotJson is null ||
            submission.DecisionSnapshotJson is null ||
            submission.CompletedAtUtc is null)
        {
            throw new InvalidOperationException(
                "Only a final accepted registration submission can be projected as a receipt.");
        }

        return new RegistrationReceipt(
            submission.Id,
            submission.Reference,
            submission.ResultCode,
            submission.ReceiptSnapshotJson,
            submission.DecisionSnapshotJson,
            submission.ReceivedAtUtc,
            submission.CompletedAtUtc.Value);
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value.Kind is DateTimeKind.Local)
        {
            throw new ArgumentException("The timestamp must be UTC.", parameterName);
        }

        return value.Kind is DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value;
    }
}
