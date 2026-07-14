namespace StudentRegistration.IdentityAccess.Domain;

public sealed class ApplicationUser
{
    private ApplicationUser()
    {
    }

    public ApplicationUser(
        Guid id,
        string userName,
        string normalizedUserName,
        string? universityId,
        string passwordHash,
        string securityStamp,
        bool isEnabled = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A user identifier is required.", nameof(id));
        }

        Id = id;
        UserName = IdentityDomainGuard.Required(userName, nameof(userName), 200);
        NormalizedUserName = IdentityDomainGuard.Required(
            normalizedUserName,
            nameof(normalizedUserName),
            200);
        UniversityId = IdentityDomainGuard.Optional(universityId, nameof(universityId), 50);
        PasswordHash = IdentityDomainGuard.Required(passwordHash, nameof(passwordHash), 1000);
        SecurityStamp = IdentityDomainGuard.Required(securityStamp, nameof(securityStamp), 200);
        IsEnabled = isEnabled;
    }

    public Guid Id { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string NormalizedUserName { get; private set; } = string.Empty;
    public string? UniversityId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public string SecurityStamp { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public byte[] Version { get; private set; } = [];

    public bool IsLockedAt(DateTime utcNow) =>
        LockoutEndUtc is { } lockoutEnd && lockoutEnd > utcNow;

    public void ReplacePasswordHash(string passwordHash, string newSecurityStamp)
    {
        PasswordHash = IdentityDomainGuard.Required(passwordHash, nameof(passwordHash), 1000);
        RotateSecurityStamp(newSecurityStamp);
        ResetAccessFailures();
    }

    public void RotateSecurityStamp(string newSecurityStamp)
    {
        SecurityStamp = IdentityDomainGuard.Required(
            newSecurityStamp,
            nameof(newSecurityStamp),
            200);
    }

    public void SetEnabled(bool enabled, string newSecurityStamp)
    {
        IsEnabled = enabled;
        RotateSecurityStamp(newSecurityStamp);
    }

    public int RecordFailedAccess(DateTime? lockoutEndUtc = null)
    {
        AccessFailedCount = checked(AccessFailedCount + 1);
        if (lockoutEndUtc is not null)
        {
            LockoutEndUtc = lockoutEndUtc;
        }

        return AccessFailedCount;
    }

    public bool RecordFailedAccess(
        DateTime utcNow,
        int maximumFailures,
        TimeSpan lockoutDuration)
    {
        if (maximumFailures < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumFailures));
        }

        if (lockoutDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lockoutDuration));
        }

        if (IsLockedAt(utcNow))
        {
            return true;
        }

        if (LockoutEndUtc is not null)
        {
            AccessFailedCount = 0;
            LockoutEndUtc = null;
        }

        AccessFailedCount = checked(AccessFailedCount + 1);
        if (AccessFailedCount >= maximumFailures)
        {
            LockoutEndUtc = utcNow.Add(lockoutDuration);
        }

        return IsLockedAt(utcNow);
    }

    public void ResetAccessFailures()
    {
        AccessFailedCount = 0;
        LockoutEndUtc = null;
    }
}
