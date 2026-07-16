using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.Scheduling.Application;

public enum GroupCapacityOutcome
{
    Applied,
    StaleVersion,
    CapacityBelowEnrolled,
    GroupFull,
}

public sealed record GroupCapacitySnapshot(
    Guid GroupId,
    string State,
    int Capacity,
    int EnrolledCount,
    bool RegistrationPaused,
    byte[] Version);

public sealed class GroupCapacityState(int capacity, int enrolledCount)
{
    public int Capacity { get; set; } = capacity;

    public int EnrolledCount { get; set; } = enrolledCount;

    public string? ActorId { get; set; }

    public string? Reason { get; set; }
}

public sealed record GroupSelectionResult(
    bool Selectable,
    IReadOnlyList<string> ReasonCodes);

public sealed class SectionGroupCapacityService(ISectionGroupCapacityStore store)
{
    public async Task<GroupSelectionResult> ReadSelectionAsync(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var group = await store.ReadAsync(groupId, cancellationToken);
        if (group is null)
        {
            return new(false, ["GROUP_NOT_FOUND"]);
        }

        var reason = group.State.ToLowerInvariant() switch
        {
            "draft" => "GROUP_UNPUBLISHED",
            "closed" => "GROUP_CLOSED",
            "cancelled" => "GROUP_CANCELLED",
            _ when group.EnrolledCount >= group.Capacity => "GROUP_FULL",
            _ when group.RegistrationPaused => "REGISTRATION_PAUSED",
            _ => null,
        };
        return reason is null
            ? new(true, [])
            : new(false, [reason]);
    }

    public Task<GroupCapacityOutcome> ChangeCapacityAsync(
        Guid groupId,
        byte[] expectedVersion,
        int capacity,
        string actorId,
        string reason,
        CancellationToken cancellationToken = default) =>
        store.ExecuteAsync(
            groupId,
            expectedVersion,
            state =>
            {
                if (capacity < state.EnrolledCount || capacity < 0)
                {
                    return GroupCapacityOutcome.CapacityBelowEnrolled;
                }

                state.Capacity = capacity;
                state.ActorId = actorId;
                state.Reason = reason;
                return GroupCapacityOutcome.Applied;
            },
            cancellationToken);

    public Task<GroupCapacityOutcome> AllocateSeatAsync(
        Guid groupId,
        byte[] expectedVersion,
        CancellationToken cancellationToken = default) =>
        store.ExecuteAsync(
            groupId,
            expectedVersion,
            state =>
            {
                if (state.EnrolledCount >= state.Capacity)
                {
                    return GroupCapacityOutcome.GroupFull;
                }

                state.EnrolledCount++;
                return GroupCapacityOutcome.Applied;
            },
            cancellationToken);
}
