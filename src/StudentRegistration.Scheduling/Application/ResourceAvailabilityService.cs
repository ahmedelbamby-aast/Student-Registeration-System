using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Scheduling.Application;

public enum ResourceAvailabilityOutcome
{
    Applied,
    Forbidden,
    NotFound,
    StaleVersion,
    ValidationError,
}

public sealed record AvailabilityRangeInput(
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string Kind);

public sealed record StaffAvailabilitySnapshot(
    Guid AvailabilityId,
    Guid StaffId,
    Guid TermId,
    byte[] RowVersion,
    IReadOnlyList<AvailabilityRangeInput> Ranges,
    bool AffectsPublishedGroup);

public sealed record RoomResourceSnapshot(
    Guid RoomId,
    byte[] RowVersion,
    int Capacity,
    string State,
    bool AffectsPublishedGroup);

public sealed record ReplaceStaffAvailabilityWrite(
    Guid AvailabilityId,
    Guid StaffId,
    Guid TermId,
    byte[] ExpectedVersion,
    IReadOnlyList<AvailabilityRangeInput> Ranges,
    DateTime ChangedAtUtc,
    string Reason,
    bool CreateImpactAlert,
    bool AppendAudit);

public sealed record UpdateRoomResourceWrite(
    Guid RoomId,
    byte[] ExpectedVersion,
    int Capacity,
    string State,
    string ActorId,
    string Reason,
    bool CreateImpactAlert,
    bool AppendAudit);

public sealed record ResourceAvailabilityResult(
    ResourceAvailabilityOutcome Outcome,
    string? ErrorCode = null);

public sealed class ResourceAvailabilityService(IResourceAvailabilityStore store)
{
    public Task<StaffAvailabilitySnapshot?> GetAdminPlanningInputAsync(
        Guid staffId,
        Guid termId,
        CancellationToken cancellationToken = default) =>
        store.LoadStaffAvailabilityAsync(staffId, termId, cancellationToken);

    public async Task<ResourceAvailabilityResult> ReplaceOwnAvailabilityAsync(
        Guid actorStaffId,
        Guid availabilityId,
        byte[] expectedVersion,
        IReadOnlyList<AvailabilityRangeInput> ranges,
        DateTime changedAtUtc,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var current = await store.LoadStaffAvailabilityByIdAsync(
            availabilityId,
            cancellationToken);
        if (current is null)
        {
            return new(ResourceAvailabilityOutcome.NotFound, "AVAILABILITY_NOT_FOUND");
        }
        if (current.StaffId != actorStaffId)
        {
            return new(ResourceAvailabilityOutcome.Forbidden, "AVAILABILITY_NOT_OWNED");
        }
        if (!expectedVersion.SequenceEqual(current.RowVersion))
        {
            return new(ResourceAvailabilityOutcome.StaleVersion, "STALE_VERSION");
        }
        if (!IsValid(ranges, changedAtUtc, reason))
        {
            return new(ResourceAvailabilityOutcome.ValidationError, "AVAILABILITY_INVALID");
        }

        await store.CommitAvailabilityAsync(
            new(
                current.AvailabilityId,
                current.StaffId,
                current.TermId,
                expectedVersion,
                ranges,
                changedAtUtc,
                reason,
                current.AffectsPublishedGroup,
                AppendAudit: true),
            cancellationToken);
        return new(ResourceAvailabilityOutcome.Applied);
    }

    public async Task<ResourceAvailabilityResult> UpdateRoomAsync(
        Guid roomId,
        byte[] expectedVersion,
        int capacity,
        RoomAvailabilityState state,
        string actorId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var current = await store.LoadRoomAsync(roomId, cancellationToken);
        if (current is null)
        {
            return new(ResourceAvailabilityOutcome.NotFound, "ROOM_NOT_FOUND");
        }
        if (!expectedVersion.SequenceEqual(current.RowVersion))
        {
            return new(ResourceAvailabilityOutcome.StaleVersion, "STALE_VERSION");
        }
        if (capacity < 0
            || string.IsNullOrWhiteSpace(actorId)
            || string.IsNullOrWhiteSpace(reason))
        {
            return new(ResourceAvailabilityOutcome.ValidationError, "ROOM_INVALID");
        }

        await store.CommitRoomAsync(
            new(
                roomId,
                expectedVersion,
                capacity,
                state.ToString().ToLowerInvariant(),
                actorId,
                reason,
                current.AffectsPublishedGroup,
                AppendAudit: true),
            cancellationToken);
        return new(ResourceAvailabilityOutcome.Applied);
    }

    private static bool IsValid(
        IReadOnlyList<AvailabilityRangeInput> ranges,
        DateTime changedAtUtc,
        string reason) =>
        ranges.Count > 0
        && changedAtUtc.Kind is DateTimeKind.Utc
        && !string.IsNullOrWhiteSpace(reason)
        && ranges.All(range =>
            range.DayOfWeek is >= 0 and <= 6
            && range.EndLocal > range.StartLocal
            && Enum.TryParse<AvailabilityKind>(
                range.Kind,
                ignoreCase: true,
                out _));
}
