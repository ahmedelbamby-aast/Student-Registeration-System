namespace StudentRegistration.Registration.Domain;

public enum RegistrationSubmissionLineState
{
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Expired = 4,
}

public sealed class RegistrationSubmissionLine
{
    private RegistrationSubmissionLine()
    {
    }

    public RegistrationSubmissionLine(
        Guid id,
        Guid submissionId,
        Guid offeringId,
        Guid groupId,
        string courseCode,
        string subjectTitle,
        decimal credits)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(submissionId, nameof(submissionId));
        RegistrationPlanDomainGuard.Identifier(offeringId, nameof(offeringId));
        RegistrationPlanDomainGuard.Identifier(groupId, nameof(groupId));
        if (credits != 3m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(credits),
                "Every roadmap subject must be exactly three credits.");
        }

        Id = id;
        SubmissionId = submissionId;
        OfferingId = offeringId;
        GroupId = groupId;
        CourseCode = RegistrationPlanDomainGuard.Required(courseCode, nameof(courseCode));
        SubjectTitle = RegistrationPlanDomainGuard.Required(subjectTitle, nameof(subjectTitle));
        Credits = credits;
        State = RegistrationSubmissionLineState.PendingApproval;
    }

    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public Guid OfferingId { get; private set; }
    public Guid GroupId { get; private set; }
    public string CourseCode { get; private set; } = string.Empty;
    public string SubjectTitle { get; private set; } = string.Empty;
    public decimal Credits { get; private set; }
    public RegistrationSubmissionLineState State { get; private set; }
    public byte[] Version { get; private set; } = [];

    public void Approve() => TransitionTo(RegistrationSubmissionLineState.Approved);
    public void Reject() => TransitionTo(RegistrationSubmissionLineState.Rejected);
    public void Expire() => TransitionTo(RegistrationSubmissionLineState.Expired);

    private void TransitionTo(RegistrationSubmissionLineState target)
    {
        if (State is not RegistrationSubmissionLineState.PendingApproval)
        {
            throw new InvalidOperationException("A decided submission line is immutable.");
        }

        State = target;
    }
}
