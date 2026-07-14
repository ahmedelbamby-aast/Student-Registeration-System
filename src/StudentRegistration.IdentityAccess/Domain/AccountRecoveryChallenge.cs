namespace StudentRegistration.IdentityAccess.Domain;

public sealed class AccountRecoveryChallenge
{
    private AccountRecoveryChallenge()
    {
    }

    public AccountRecoveryChallenge(
        Guid id,
        Guid applicationUserId,
        string tokenHash,
        string deliveryReferenceHash,
        DateTime expiresAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A challenge identifier is required.", nameof(id));
        }

        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException("An application user is required.", nameof(applicationUserId));
        }

        Id = id;
        ApplicationUserId = applicationUserId;
        TokenHash = IdentityDomainGuard.Required(tokenHash, nameof(tokenHash), 256);
        DeliveryReferenceHash = IdentityDomainGuard.Required(
            deliveryReferenceHash,
            nameof(deliveryReferenceHash),
            256);
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationUserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public string DeliveryReferenceHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public int FailedAttemptCount { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }
    public byte[] Version { get; private set; } = [];

    public bool TryConsume(DateTime consumedAtUtc, int maximumAttempts)
    {
        if (maximumAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }

        if (ConsumedAtUtc is not null ||
            consumedAtUtc >= ExpiresAtUtc ||
            FailedAttemptCount >= maximumAttempts)
        {
            return false;
        }

        ConsumedAtUtc = consumedAtUtc;
        return true;
    }

    public bool RecordFailedAttempt(int maximumAttempts)
    {
        if (maximumAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }

        if (ConsumedAtUtc is not null || FailedAttemptCount >= maximumAttempts)
        {
            return false;
        }

        FailedAttemptCount = checked(FailedAttemptCount + 1);
        return FailedAttemptCount < maximumAttempts;
    }
}
