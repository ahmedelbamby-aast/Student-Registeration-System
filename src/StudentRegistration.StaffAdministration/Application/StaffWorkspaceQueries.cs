using System.Security.Claims;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.StaffAdministration.Application;

public enum StaffWorkspaceQueryOutcome
{
    Succeeded,
    Invalid,
    Unauthorized,
    Forbidden,
    NotFound,
    Unavailable
}

public sealed record StaffAssignmentsQueryResult(
    StaffWorkspaceQueryOutcome Outcome,
    IReadOnlyList<StaffAssignmentDto>? Assignments = null,
    string? ErrorCode = null);

public sealed record StaffTimetableQueryResult(
    StaffWorkspaceQueryOutcome Outcome,
    StaffTimetableDto? Timetable = null,
    string? ErrorCode = null);

public sealed record StaffRosterQueryResult(
    StaffWorkspaceQueryOutcome Outcome,
    Page<RosterRowDto>? Page = null,
    string? ErrorCode = null);

public sealed class StaffWorkspaceQueries(
    IStaffWorkspaceReader reader,
    IStaffWorkspaceAuditWriter auditWriter)
{
    private const string RosterPurpose = "assigned-group-roster";

    public async Task<StaffAssignmentsQueryResult> GetAssignmentsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var context = Context(principal);
        if (context.Outcome is not StaffWorkspaceQueryOutcome.Succeeded)
        {
            return new(context.Outcome, ErrorCode: context.ErrorCode);
        }

        var assignments = await reader.ListAssignmentsAsync(
            context.ActorId,
            context.ActiveRole!,
            cancellationToken);
        var projected = ProjectAssignments(assignments, context.ActiveRole!);
        return projected is null
            ? new(StaffWorkspaceQueryOutcome.Unavailable,
                ErrorCode: "STAFF_SCOPE_UNAVAILABLE")
            : new(StaffWorkspaceQueryOutcome.Succeeded, projected);
    }

    public async Task<StaffTimetableQueryResult> GetTimetableAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var context = Context(principal);
        if (context.Outcome is not StaffWorkspaceQueryOutcome.Succeeded)
        {
            return new(context.Outcome, ErrorCode: context.ErrorCode);
        }

        var assignments = await reader.ListTimetableAsync(
            context.ActorId,
            context.ActiveRole!,
            cancellationToken);
        var projected = ProjectAssignments(assignments, context.ActiveRole!);
        return projected is null
            ? new(StaffWorkspaceQueryOutcome.Unavailable,
                ErrorCode: "STAFF_SCOPE_UNAVAILABLE")
            : new(
                StaffWorkspaceQueryOutcome.Succeeded,
                new StaffTimetableDto(context.ActiveRole!, projected));
    }

    public async Task<StaffRosterQueryResult> GetRosterAsync(
        ClaimsPrincipal principal,
        Guid groupId,
        int page = 1,
        int pageSize = StaffWorkspaceContract.DefaultPageSize,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var context = Context(principal);
        if (context.Outcome is not StaffWorkspaceQueryOutcome.Succeeded)
        {
            return new(context.Outcome, ErrorCode: context.ErrorCode);
        }

        if (groupId == Guid.Empty)
        {
            return new(
                StaffWorkspaceQueryOutcome.Invalid,
                ErrorCode: "GROUP_ID_INVALID");
        }

        if (!ValidPage(page, pageSize))
        {
            return new(
                StaffWorkspaceQueryOutcome.Invalid,
                ErrorCode: "PAGE_SIZE_INVALID");
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return new(
                StaffWorkspaceQueryOutcome.Invalid,
                ErrorCode: "CORRELATION_REQUIRED");
        }

        var snapshot = await reader.ReadRosterIfAssignedAsync(
            context.ActorId,
            context.ActiveRole!,
            groupId,
            page,
            pageSize,
            StaffWorkspaceContract.RosterSort,
            cancellationToken);

        if (snapshot is null)
        {
            await AuditAsync(
                context.ActorId,
                groupId,
                StaffRosterAuditOutcome.Denied,
                0,
                correlationId,
                cancellationToken);
            return new(
                StaffWorkspaceQueryOutcome.NotFound,
                ErrorCode: "STAFF_GROUP_NOT_FOUND");
        }

        if (snapshot.Items is null
            || snapshot.TotalCount < snapshot.Items.Count
            || snapshot.Items.Count > pageSize)
        {
            await AuditAsync(
                context.ActorId,
                groupId,
                StaffRosterAuditOutcome.Failed,
                0,
                correlationId,
                cancellationToken);
            return new(
                StaffWorkspaceQueryOutcome.Unavailable,
                ErrorCode: "ROSTER_UNAVAILABLE");
        }

        try
        {
            var items = snapshot.Items
                .OrderBy(item => item.DisplayName, StringComparer.Ordinal)
                .ThenBy(item => item.UniversityId, StringComparer.Ordinal)
                .Select(item => item.ToDto())
                .ToArray();
            var result = new Page<RosterRowDto>(
                items,
                page,
                pageSize,
                snapshot.TotalCount,
                StaffWorkspaceContract.RosterSort);
            await AuditAsync(
                context.ActorId,
                groupId,
                StaffRosterAuditOutcome.Succeeded,
                items.Length,
                correlationId,
                cancellationToken);
            return new(StaffWorkspaceQueryOutcome.Succeeded, result);
        }
        catch (ArgumentException)
        {
            await AuditAsync(
                context.ActorId,
                groupId,
                StaffRosterAuditOutcome.Failed,
                0,
                correlationId,
                cancellationToken);
            return new(
                StaffWorkspaceQueryOutcome.Unavailable,
                ErrorCode: "ROSTER_UNAVAILABLE");
        }
    }

    private Task AuditAsync(
        Guid actorId,
        Guid groupId,
        StaffRosterAuditOutcome outcome,
        int rowCount,
        string correlationId,
        CancellationToken cancellationToken) =>
        auditWriter.WriteRosterAccessAsync(
            new StaffRosterAuditEntry(
                actorId,
                groupId,
                RosterPurpose,
                outcome,
                rowCount,
                correlationId),
            cancellationToken);

    private static IReadOnlyList<StaffAssignmentDto>? ProjectAssignments(
        IReadOnlyList<StaffAssignment>? assignments,
        string activeRole)
    {
        if (assignments is null
            || assignments.Any(assignment =>
                assignment is null
                || !string.Equals(
                    assignment.StaffRole,
                    activeRole,
                    StringComparison.Ordinal)))
        {
            return null;
        }

        return assignments
            .GroupBy(assignment => assignment.Group.Id)
            .Select(group => group.First())
            .OrderBy(assignment => assignment.SubjectCode, StringComparer.Ordinal)
            .ThenBy(assignment => assignment.Group.GroupCode, StringComparer.Ordinal)
            .ThenBy(assignment => assignment.Group.Id)
            .Select(assignment => assignment.ToDto())
            .ToArray();
    }

    private static StaffContext Context(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true
            || !Guid.TryParse(
                principal.FindFirstValue(ClaimTypes.NameIdentifier),
                out var actorId)
            || actorId == Guid.Empty)
        {
            return new(
                StaffWorkspaceQueryOutcome.Unauthorized,
                Guid.Empty,
                null,
                "UNAUTHORIZED");
        }

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (roles.Length != 1
            || roles[0] is not StaffWorkspaceContract.LecturerRole
                and not StaffWorkspaceContract.TeachingAssistantRole)
        {
            return new(
                StaffWorkspaceQueryOutcome.Forbidden,
                actorId,
                null,
                "FORBIDDEN");
        }

        return new(
            StaffWorkspaceQueryOutcome.Succeeded,
            actorId,
            roles[0],
            null);
    }

    private static bool ValidPage(int page, int pageSize) =>
        page >= 1
        && pageSize is >= 1 and <= StaffWorkspaceContract.MaximumPageSize
        && page <= int.MaxValue / pageSize;

    private sealed record StaffContext(
        StaffWorkspaceQueryOutcome Outcome,
        Guid ActorId,
        string? ActiveRole,
        string? ErrorCode);
}
