using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.StaffAdministration.Application.Ports;

public sealed record StaffRosterPageSnapshot(
    IReadOnlyList<RosterRow> Items,
    int TotalCount);

/// <summary>
/// Reads current Scheduling/Registration/Identity projections for one
/// authenticated staff context. Implementations must apply the requested
/// roster ordering before paging and must verify current assignment in the
/// same query before materializing roster rows.
/// </summary>
public interface IStaffWorkspaceReader
{
    Task<IReadOnlyList<StaffAssignment>> ListAssignmentsAsync(
        Guid actorApplicationUserId,
        string activeRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StaffAssignment>> ListTimetableAsync(
        Guid actorApplicationUserId,
        string activeRole,
        CancellationToken cancellationToken = default);

    Task<StaffRosterPageSnapshot?> ReadRosterIfAssignedAsync(
        Guid actorApplicationUserId,
        string activeRole,
        Guid groupId,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default);
}
