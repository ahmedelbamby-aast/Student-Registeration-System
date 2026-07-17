namespace StudentRegistration.Registration.Domain;

public enum EnrollmentState
{
    Active = 1,
}

public sealed class Enrollment
{
    private Enrollment()
    {
    }

    public Enrollment(
        Guid id,
        Guid studentId,
        Guid offeringId,
        Guid groupId,
        Guid submissionId,
        EnrollmentState state,
        DateTime registeredAtUtc)
    {
        RegistrationPlanDomainGuard.Identifier(id, nameof(id));
        RegistrationPlanDomainGuard.Identifier(studentId, nameof(studentId));
        RegistrationPlanDomainGuard.Identifier(offeringId, nameof(offeringId));
        RegistrationPlanDomainGuard.Identifier(groupId, nameof(groupId));
        RegistrationPlanDomainGuard.Identifier(submissionId, nameof(submissionId));
        RegistrationPlanDomainGuard.Defined(state, nameof(state));
        if (registeredAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The registration instant must be UTC.",
                nameof(registeredAtUtc));
        }

        Id = id;
        StudentId = studentId;
        OfferingId = offeringId;
        GroupId = groupId;
        SubmissionId = submissionId;
        State = state;
        RegisteredAtUtc = registeredAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid StudentId { get; private set; }

    public Guid OfferingId { get; private set; }

    public Guid GroupId { get; private set; }

    public Guid SubmissionId { get; private set; }

    public EnrollmentState State { get; private set; }

    public DateTime RegisteredAtUtc { get; private set; }

    public byte[] Version { get; private set; } = [];
}
