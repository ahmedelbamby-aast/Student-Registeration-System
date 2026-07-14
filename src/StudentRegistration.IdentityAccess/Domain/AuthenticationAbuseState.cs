namespace StudentRegistration.IdentityAccess.Domain;

public sealed class AuthenticationAbuseState
{
    private AuthenticationAbuseState()
    {
    }

    public AuthenticationAbuseState(
        Guid id,
        string subjectKeyHash,
        string operation,
        DateTime windowStartedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An abuse-state identifier is required.", nameof(id));
        }

        Id = id;
        SubjectKeyHash = IdentityDomainGuard.Required(
            subjectKeyHash,
            nameof(subjectKeyHash),
            256);
        Operation = IdentityDomainGuard.Required(operation, nameof(operation), 50);
        WindowStartedAtUtc = windowStartedAtUtc;
    }

    public Guid Id { get; private set; }
    public string SubjectKeyHash { get; private set; } = string.Empty;
    public string Operation { get; private set; } = string.Empty;
    public int FailureCount { get; private set; }
    public DateTime WindowStartedAtUtc { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public byte[] Version { get; private set; } = [];

    public bool IsLockedAt(DateTime utcNow) =>
        LockedUntilUtc is { } lockedUntil && utcNow < lockedUntil;

    public bool RecordFailure(
        DateTime utcNow,
        int maximumFailures,
        TimeSpan window,
        TimeSpan lockout)
    {
        if (maximumFailures < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumFailures));
        }

        if (window <= TimeSpan.Zero || lockout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window));
        }

        if (IsLockedAt(utcNow))
        {
            return true;
        }

        if (LockedUntilUtc is not null || utcNow - WindowStartedAtUtc >= window)
        {
            FailureCount = 0;
            WindowStartedAtUtc = utcNow;
            LockedUntilUtc = null;
        }

        FailureCount = checked(FailureCount + 1);
        if (FailureCount >= maximumFailures)
        {
            LockedUntilUtc = utcNow.Add(lockout);
        }

        return IsLockedAt(utcNow);
    }

    public void Reset(DateTime utcNow)
    {
        FailureCount = 0;
        WindowStartedAtUtc = utcNow;
        LockedUntilUtc = null;
    }
}
