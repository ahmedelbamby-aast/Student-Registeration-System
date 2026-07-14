namespace StudentRegistration.IdentityAccess.Domain;

public sealed class StudentActivation
{
    private StudentActivation()
    {
    }

    public StudentActivation(Guid id, Guid applicationUserId, DateTime provisionedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An activation identifier is required.", nameof(id));
        }

        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException("An application user is required.", nameof(applicationUserId));
        }

        Id = id;
        ApplicationUserId = applicationUserId;
        ProvisionedAtUtc = provisionedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationUserId { get; private set; }
    public DateTime ProvisionedAtUtc { get; private set; }
    public DateTime? ActivatedAtUtc { get; private set; }
    public int FailedAttemptCount { get; private set; }
    public byte[] Version { get; private set; } = [];

    public bool TryConsume(DateTime activatedAtUtc)
    {
        if (ActivatedAtUtc is not null)
        {
            return false;
        }

        ActivatedAtUtc = activatedAtUtc;
        return true;
    }

    public bool RecordFailedAttempt(int maximumAttempts)
    {
        if (maximumAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }

        if (ActivatedAtUtc is not null || FailedAttemptCount >= maximumAttempts)
        {
            return false;
        }

        FailedAttemptCount = checked(FailedAttemptCount + 1);
        return FailedAttemptCount < maximumAttempts;
    }
}
