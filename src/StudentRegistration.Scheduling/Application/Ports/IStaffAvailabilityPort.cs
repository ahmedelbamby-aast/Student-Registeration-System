using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Scheduling.Application.Ports;

public interface IStaffAvailabilityPort
{
    Task<StaffAvailabilityPortResult> GetOwnAsync(
        Guid staffId,
        Guid termId,
        CancellationToken cancellationToken = default);

    Task<StaffAvailabilityPortResult> ReplaceOwnAsync(
        ReplaceOwnStaffAvailability command,
        CancellationToken cancellationToken = default);
}

public enum StaffAvailabilityPortOutcome
{
    Success,
    NotFound,
    StaleVersion,
    DeadlinePassed,
    ValidationFailed,
}

public sealed record StaffAvailabilityRangeInput(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    AvailabilityKind Kind);

public sealed record StaffAvailabilityRangeSnapshot(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    AvailabilityKind Kind);

public sealed record StaffTermAvailabilitySnapshot(
    Guid Id,
    Guid StaffId,
    Guid TermId,
    DateTime DeadlineUtc,
    byte[] RowVersion,
    IReadOnlyList<StaffAvailabilityRangeSnapshot> Ranges);

public sealed record ReplaceOwnStaffAvailability(
    Guid StaffId,
    Guid TermId,
    byte[] ExpectedStaffTermVersion,
    IReadOnlyList<StaffAvailabilityRangeInput> Ranges,
    string Reason,
    string CorrelationId);

public sealed record StaffAvailabilityPortResult(
    StaffAvailabilityPortOutcome Outcome,
    StaffTermAvailabilitySnapshot? Availability,
    IReadOnlyList<Guid> ImpactAlertIds,
    DateTime ServerTimeUtc,
    DateTime? DeadlineUtc,
    string? ErrorCode)
{
    public static StaffAvailabilityPortResult Success(
        StaffTermAvailabilitySnapshot availability,
        IReadOnlyList<Guid> impactAlertIds,
        DateTime serverTimeUtc) =>
        new(
            StaffAvailabilityPortOutcome.Success,
            availability,
            impactAlertIds,
            serverTimeUtc,
            availability.DeadlineUtc,
            null);

    public static StaffAvailabilityPortResult NotFound(DateTime serverTimeUtc) =>
        new(
            StaffAvailabilityPortOutcome.NotFound,
            null,
            [],
            serverTimeUtc,
            null,
            "AVAILABILITY_NOT_FOUND");

    public static StaffAvailabilityPortResult Stale(
        StaffTermAvailabilitySnapshot current,
        DateTime serverTimeUtc) =>
        new(
            StaffAvailabilityPortOutcome.StaleVersion,
            current,
            [],
            serverTimeUtc,
            current.DeadlineUtc,
            "STALE_VERSION");

    public static StaffAvailabilityPortResult DeadlinePassed(
        StaffTermAvailabilitySnapshot current,
        DateTime serverTimeUtc) =>
        new(
            StaffAvailabilityPortOutcome.DeadlinePassed,
            current,
            [],
            serverTimeUtc,
            current.DeadlineUtc,
            "AVAILABILITY_DEADLINE_PASSED");

    public static StaffAvailabilityPortResult Invalid(
        StaffTermAvailabilitySnapshot? current,
        DateTime serverTimeUtc) =>
        new(
            StaffAvailabilityPortOutcome.ValidationFailed,
            current,
            [],
            serverTimeUtc,
            current?.DeadlineUtc,
            "VALIDATION_ERROR");
}
