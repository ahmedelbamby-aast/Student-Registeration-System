namespace StudentRegistration.StaffAdministration.Domain;

public enum ExportJobState
{
    Pending = 1,
    Running = 2,
    Complete = 3,
    Failed = 4,
    Expired = 5,
}

/// <summary>
/// Durable ownership and lease state for one bounded administrative export.
/// SQL persistence supplies the conditional update and rowversion checks; this
/// aggregate enforces the same lifecycle for every caller.
/// </summary>
public sealed class ExportJob
{
    public const int MaximumAttempts = 3;
    public const int MaximumFailureCodeLength = 100;
    public static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(60);

    private ExportJob()
    {
    }

    public ExportJob(
        Guid id,
        Guid ownerId,
        Guid clientRequestId,
        string scopeHash,
        string requestHash,
        DateTime createdAtUtc)
    {
        RequiredIdentifier(id, nameof(id));
        RequiredIdentifier(ownerId, nameof(ownerId));
        RequiredIdentifier(clientRequestId, nameof(clientRequestId));
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        Id = id;
        OwnerId = ownerId;
        ClientRequestId = clientRequestId;
        ScopeHash = Required(scopeHash, nameof(scopeHash), 200);
        RequestHash = Required(requestHash, nameof(requestHash), 200);
        CreatedAtUtc = createdAtUtc;
        State = ExportJobState.Pending;
    }

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public Guid ClientRequestId { get; private set; }

    public string ScopeHash { get; private set; } = string.Empty;

    public string RequestHash { get; private set; } = string.Empty;

    public ExportJobState State { get; private set; }

    public Guid? ArtifactId { get; private set; }

    public string? FailureCode { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public string? LeaseOwnerId { get; private set; }

    public DateTime? LeaseExpiresAtUtc { get; private set; }

    public int AttemptCount { get; private set; }

    public byte[] Version { get; private set; } = [];

    public bool TryClaim(string leaseOwnerId, DateTime claimedAtUtc)
    {
        var normalizedOwner = Required(
            leaseOwnerId,
            nameof(leaseOwnerId),
            200);
        EnsureUtc(claimedAtUtc, nameof(claimedAtUtc));

        var claimable = State is ExportJobState.Pending
            || State is ExportJobState.Running
                && LeaseExpiresAtUtc is not null
                && claimedAtUtc >= LeaseExpiresAtUtc.Value;
        if (!claimable)
        {
            return false;
        }

        if (AttemptCount >= MaximumAttempts)
        {
            State = ExportJobState.Failed;
            CompletedAtUtc ??= claimedAtUtc;
            FailureCode = "EXPORT_ATTEMPTS_EXHAUSTED";
            ClearLease();
            return false;
        }

        State = ExportJobState.Running;
        AttemptCount++;
        LeaseOwnerId = normalizedOwner;
        LeaseExpiresAtUtc = claimedAtUtc.Add(LeaseDuration);
        return true;
    }

    public void RenewLease(string leaseOwnerId, DateTime renewedAtUtc)
    {
        EnsureCurrentLease(leaseOwnerId, renewedAtUtc);
        LeaseExpiresAtUtc = renewedAtUtc.Add(LeaseDuration);
    }

    public void Complete(
        string leaseOwnerId,
        Guid artifactId,
        DateTime completedAtUtc,
        DateTime expiresAtUtc)
    {
        EnsureCurrentLease(leaseOwnerId, completedAtUtc);
        RequiredIdentifier(artifactId, nameof(artifactId));
        EnsureUtc(expiresAtUtc, nameof(expiresAtUtc));
        if (expiresAtUtc <= completedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAtUtc),
                "Artifact expiry must follow completion.");
        }

        if (ArtifactId is not null)
        {
            throw new InvalidOperationException(
                "An export job can publish only one artifact.");
        }

        ArtifactId = artifactId;
        CompletedAtUtc = completedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        State = ExportJobState.Complete;
        ClearLease();
    }

    public void RecordFailure(
        string leaseOwnerId,
        DateTime failedAtUtc,
        bool retryable,
        string failureCode)
    {
        EnsureCurrentLease(leaseOwnerId, failedAtUtc);
        var normalizedFailureCode = Required(
            failureCode,
            nameof(failureCode),
            MaximumFailureCodeLength);
        ClearLease();

        if (retryable && AttemptCount < MaximumAttempts)
        {
            State = ExportJobState.Pending;
            FailureCode = null;
            return;
        }

        State = ExportJobState.Failed;
        CompletedAtUtc = failedAtUtc;
        FailureCode = normalizedFailureCode;
    }

    public bool Expire(DateTime observedAtUtc)
    {
        EnsureUtc(observedAtUtc, nameof(observedAtUtc));
        if (State is not ExportJobState.Complete
            || ExpiresAtUtc is null
            || observedAtUtc < ExpiresAtUtc.Value)
        {
            return false;
        }

        State = ExportJobState.Expired;
        return true;
    }

    private void EnsureCurrentLease(string leaseOwnerId, DateTime observedAtUtc)
    {
        var normalizedOwner = Required(
            leaseOwnerId,
            nameof(leaseOwnerId),
            200);
        EnsureUtc(observedAtUtc, nameof(observedAtUtc));
        if (State is not ExportJobState.Running
            || !string.Equals(LeaseOwnerId, normalizedOwner, StringComparison.Ordinal)
            || LeaseExpiresAtUtc is null
            || observedAtUtc >= LeaseExpiresAtUtc.Value)
        {
            throw new InvalidOperationException(
                "Only the current unexpired lease owner may change this job.");
        }
    }

    private void ClearLease()
    {
        LeaseOwnerId = null;
        LeaseExpiresAtUtc = null;
    }

    private static void RequiredIdentifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }

    private static string Required(
        string value,
        string parameterName,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.Any(char.IsControl))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "The value exceeds its safe bound.");
        }

        return normalized;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must use UTC.", parameterName);
        }
    }
}
