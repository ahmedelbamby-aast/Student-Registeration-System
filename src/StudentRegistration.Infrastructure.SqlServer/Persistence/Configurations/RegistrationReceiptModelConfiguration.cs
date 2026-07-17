using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

/// <summary>
/// Defines the SPEC-015 read projection over the SPEC-014-owned submission.
/// This type deliberately implements no EF configuration interface: the
/// canonical RegistrationModelConfiguration remains the sole table mapping.
/// </summary>
public static class RegistrationReceiptModelConfiguration
{
    public static IQueryable<RegistrationReceipt> Project(
        IQueryable<RegistrationSubmission> submissions)
    {
        ArgumentNullException.ThrowIfNull(submissions);

        return submissions
            .Where(submission =>
                submission.ProcessingState == RegistrationSubmissionState.Accepted &&
                submission.Reference != null &&
                submission.ResultCode != null &&
                submission.ReceiptSnapshotJson != null &&
                submission.DecisionSnapshotJson != null &&
                submission.CompletedAtUtc != null)
            .Select(submission => new RegistrationReceipt(
                submission.Id,
                submission.Reference!,
                submission.ResultCode!,
                submission.ReceiptSnapshotJson!,
                submission.DecisionSnapshotJson!,
                submission.ReceivedAtUtc,
                submission.CompletedAtUtc!.Value));
    }
}
