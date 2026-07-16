namespace StudentRegistration.Scheduling.Domain;

public enum ScheduleImpactAlertState
{
    Open = 1,
    Revalidated = 2,
    Resolved = 3,
}

public sealed class ScheduleImpactAlert
{
    private ScheduleImpactAlert()
    {
    }

    public ScheduleImpactAlert(
        Guid id,
        Guid groupId,
        Guid staffTermAvailabilityId,
        string reasonCode,
        byte[] detectedGroupVersion,
        byte[] detectedResourceVersion,
        DateTime detectedAtUtc)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Identifier(groupId, nameof(groupId));
        SchedulingDomainValue.Identifier(
            staffTermAvailabilityId,
            nameof(staffTermAvailabilityId));
        SchedulingDomainValue.Utc(detectedAtUtc, nameof(detectedAtUtc));

        Id = id;
        GroupId = groupId;
        StaffTermAvailabilityId = staffTermAvailabilityId;
        ReasonCode = SchedulingDomainValue.Code(reasonCode, nameof(reasonCode));
        DetectedGroupVersion = SchedulingDomainValue.Version(
            detectedGroupVersion,
            nameof(detectedGroupVersion));
        DetectedResourceVersion = SchedulingDomainValue.Version(
            detectedResourceVersion,
            nameof(detectedResourceVersion));
        DetectedAtUtc = detectedAtUtc;
        State = ScheduleImpactAlertState.Open;
    }

    public Guid Id { get; private set; }

    public Guid GroupId { get; private set; }

    public Guid? StaffTermAvailabilityId { get; private set; }

    public Guid? RoomId { get; private set; }

    public string ReasonCode { get; private set; } = string.Empty;

    public byte[] DetectedGroupVersion { get; private set; } = [];

    public byte[] DetectedResourceVersion { get; private set; } = [];

    public ScheduleImpactAlertState State { get; private set; }

    public DateTime DetectedAtUtc { get; private set; }

    public DateTime? RevalidatedAtUtc { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    public string? LastValidationJson { get; private set; }

    public bool LastRevalidationPassed { get; private set; }

    public string? ResolutionReason { get; private set; }

    public byte[] Version { get; private set; } = [];

    public void RecordRevalidation(
        string validationJson,
        bool passed,
        DateTime revalidatedAtUtc)
    {
        SchedulingDomainValue.Utc(revalidatedAtUtc, nameof(revalidatedAtUtc));
        if (State is ScheduleImpactAlertState.Resolved)
        {
            throw new InvalidOperationException("A resolved alert cannot be revalidated.");
        }

        LastValidationJson = SchedulingDomainValue.Required(
            validationJson,
            nameof(validationJson));
        LastRevalidationPassed = passed;
        RevalidatedAtUtc = revalidatedAtUtc;
        State = ScheduleImpactAlertState.Revalidated;
    }

    public void Resolve(string reason, DateTime resolvedAtUtc)
    {
        SchedulingDomainValue.Utc(resolvedAtUtc, nameof(resolvedAtUtc));
        if (State is not ScheduleImpactAlertState.Revalidated
            || !LastRevalidationPassed)
        {
            throw new InvalidOperationException(
                "A passing revalidation is required before resolution.");
        }

        ResolutionReason = SchedulingDomainValue.Required(reason, nameof(reason));
        ResolvedAtUtc = resolvedAtUtc;
        State = ScheduleImpactAlertState.Resolved;
    }
}
