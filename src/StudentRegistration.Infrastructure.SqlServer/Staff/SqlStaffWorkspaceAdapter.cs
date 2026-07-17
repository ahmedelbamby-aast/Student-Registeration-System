using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.StaffWorkspace;

/// <summary>
/// One SQL-backed read adapter for the bounded staff workspace. It joins
/// canonical module-owned entities without introducing another persistence
/// model or write owner.
/// </summary>
public sealed class SqlStaffWorkspaceAdapter(
    StudentRegistrationDbContext dbContext,
    TimeProvider timeProvider) :
    IStaffWorkspaceReader,
    IStaffWorkspaceAuditWriter,
    IStaffIdentityResolver
{
    public async Task<Guid?> ResolveActiveStaffIdAsync(
        Guid actorApplicationUserId,
        CancellationToken cancellationToken = default)
    {
        var staffId = await ActiveStaffIds(actorApplicationUserId)
            .SingleOrDefaultAsync(cancellationToken);
        return staffId == Guid.Empty ? null : staffId;
    }

    public async Task<IReadOnlyList<StaffAssignment>> ListAssignmentsAsync(
        Guid actorApplicationUserId,
        string activeRole,
        CancellationToken cancellationToken = default)
    {
        if (!TryRole(activeRole, out var teachingRole))
        {
            return [];
        }

        var resolvedStaffId = await ResolveActiveStaffIdAsync(
            actorApplicationUserId,
            cancellationToken);
        if (resolvedStaffId is null)
        {
            return [];
        }
        var staffId = resolvedStaffId.Value;

        var groupIds = await dbContext.Set<GroupStaffAssignment>()
            .AsNoTracking()
            .Where(assignment =>
                assignment.StaffId == staffId
                && assignment.TeachingRole == teachingRole)
            .Select(assignment => assignment.GroupId)
            .Distinct()
            .OrderBy(id => id)
            .ToArrayAsync(cancellationToken);
        return await ProjectAssignments(groupIds, activeRole, cancellationToken);
    }

    public Task<IReadOnlyList<StaffAssignment>> ListTimetableAsync(
        Guid actorApplicationUserId,
        string activeRole,
        CancellationToken cancellationToken = default) =>
        ListAssignmentsAsync(actorApplicationUserId, activeRole, cancellationToken);

    public async Task<StaffRosterPageSnapshot?> ReadRosterIfAssignedAsync(
        Guid actorApplicationUserId,
        string activeRole,
        Guid groupId,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default)
    {
        if (groupId == Guid.Empty
            || !TryRole(activeRole, out var teachingRole)
            || page < 1
            || pageSize is < 1 or > 100
            || !string.Equals(sort, "displayName:asc,universityId:asc", StringComparison.Ordinal))
        {
            return null;
        }

        // Authorization and group scope are part of this SQL predicate and
        // execute before any enrollment/student row is composed.
        var assigned = await (
            from staff in dbContext.Set<StudentRegistration.IdentityAccess.Domain.Staff>().AsNoTracking()
            join assignment in dbContext.Set<GroupStaffAssignment>().AsNoTracking()
                on staff.Id equals assignment.StaffId
            where staff.ApplicationUserId == actorApplicationUserId
                && staff.IsActive
                && assignment.GroupId == groupId
                && assignment.TeachingRole == teachingRole
            select assignment.GroupId)
            .AnyAsync(cancellationToken);
        if (!assigned)
        {
            return null;
        }

        var rows =
            from enrollment in dbContext.Set<Enrollment>().AsNoTracking()
            join student in dbContext.Set<Student>().AsNoTracking()
                on enrollment.StudentId equals student.Id
            join user in dbContext.Set<ApplicationUser>().AsNoTracking()
                on student.ApplicationUserId equals user.Id
            where enrollment.GroupId == groupId
                && enrollment.State == EnrollmentState.Active
                && student.IsActive
                && user.IsEnabled
                && user.UniversityId != null
            select new
            {
                UniversityId = user.UniversityId!,
                // ApplicationUser.UserName is the canonical persisted student
                // display label in the current demo identity model.
                DisplayName = user.UserName
            };

        var total = await rows.CountAsync(cancellationToken);
        var items = await rows
            .OrderBy(row => row.DisplayName)
            .ThenBy(row => row.UniversityId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new RosterRow(
                row.UniversityId,
                row.DisplayName,
                "active"))
            .ToArrayAsync(cancellationToken);
        return new StaffRosterPageSnapshot(items, total);
    }

    public async Task WriteRosterAccessAsync(
        StaffRosterAuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var metadata = JsonSerializer.Serialize(new
        {
            purpose = entry.Purpose,
            outcome = entry.Outcome.ToString(),
            rowCount = entry.RowCount
        });
        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            entry.ActorApplicationUserId.ToString("D"),
            entry.ActorApplicationUserId.ToString("D"),
            "staff-roster-access",
            "SectionGroup",
            entry.GroupId.ToString("D"),
            entry.Purpose,
            "{}",
            metadata,
            entry.CorrelationId,
            now));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Guid> ActiveStaffIds(Guid actorApplicationUserId) =>
        dbContext.Set<StudentRegistration.IdentityAccess.Domain.Staff>()
            .AsNoTracking()
            .Where(staff =>
                staff.ApplicationUserId == actorApplicationUserId
                && staff.IsActive)
            .Select(staff => staff.Id);

    private async Task<IReadOnlyList<StaffAssignment>> ProjectAssignments(
        IReadOnlyList<Guid> groupIds,
        string activeRole,
        CancellationToken cancellationToken)
    {
        if (groupIds.Count == 0)
        {
            return [];
        }

        var groups = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(group => groupIds.Contains(group.Id))
            .OrderBy(group => group.Id)
            .ToArrayAsync(cancellationToken);
        var offerings = await dbContext.Set<CourseOffering>()
            .AsNoTracking()
            .Where(offering => groups.Select(group => group.OfferingId).Contains(offering.Id))
            .ToDictionaryAsync(offering => offering.Id, cancellationToken);
        var courses = await dbContext.Set<Course>()
            .AsNoTracking()
            .Where(course => offerings.Values.Select(offering => offering.CourseId).Contains(course.Id))
            .ToDictionaryAsync(course => course.Id, cancellationToken);
        var meetings = await dbContext.Set<MeetingSlot>()
            .AsNoTracking()
            .Where(meeting => groupIds.Contains(meeting.GroupId))
            .OrderBy(meeting => meeting.DayOfWeek)
            .ThenBy(meeting => meeting.StartLocal)
            .ThenBy(meeting => meeting.Id)
            .ToArrayAsync(cancellationToken);
        var assignments = await dbContext.Set<GroupStaffAssignment>()
            .AsNoTracking()
            .Where(assignment => groupIds.Contains(assignment.GroupId))
            .OrderBy(assignment => assignment.GroupId)
            .ThenBy(assignment => assignment.MeetingSlotId)
            .ThenBy(assignment => assignment.StaffId)
            .ToArrayAsync(cancellationToken);
        var staffNames = await dbContext.Set<StudentRegistration.IdentityAccess.Domain.Staff>()
            .AsNoTracking()
            .Where(staff => assignments.Select(assignment => assignment.StaffId).Contains(staff.Id))
            .ToDictionaryAsync(staff => staff.Id, staff => staff.DisplayName, cancellationToken);
        var roomIds = meetings.Select(meeting => meeting.RoomId).Distinct().ToArray();
        var rooms = await dbContext.Set<Room>()
            .AsNoTracking()
            .Where(room => roomIds.Contains(room.Id))
            .ToDictionaryAsync(room => room.Id, cancellationToken);
        var rosterCounts = await dbContext.Set<Enrollment>()
            .AsNoTracking()
            .Where(enrollment =>
                groupIds.Contains(enrollment.GroupId)
                && enrollment.State == EnrollmentState.Active)
            .GroupBy(enrollment => enrollment.GroupId)
            .Select(group => new { GroupId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.GroupId, row => row.Count, cancellationToken);

        var result = new List<StaffAssignment>(groups.Length);
        foreach (var group in groups)
        {
            if (!offerings.TryGetValue(group.OfferingId, out var offering)
                || !courses.TryGetValue(offering.CourseId, out var course))
            {
                continue;
            }

            var groupMeetings = meetings.Where(meeting => meeting.GroupId == group.Id).ToArray();
            var meetingDtos = groupMeetings.Select(meeting =>
            {
                rooms.TryGetValue(meeting.RoomId, out var room);
                return new MeetingDto(
                    meeting.Id,
                    meeting.ActivityType.ToString(),
                    meeting.DayOfWeek,
                    meeting.StartLocal,
                    meeting.EndLocal,
                    meeting.RoomId,
                    room?.Code ?? "Unassigned",
                    room?.Location ?? "Unassigned");
            }).ToArray();
            var staffDtos = assignments
                .Where(assignment => assignment.GroupId == group.Id)
                .Select(assignment => new GroupStaffDto(
                    assignment.MeetingSlotId,
                    assignment.ActivityType.ToString(),
                    assignment.StaffId,
                    assignment.TeachingRole.ToString(),
                    staffNames.GetValueOrDefault(assignment.StaffId, "Assigned staff")))
                .ToArray();
            var groupDto = new GroupDto(
                group.Id,
                group.OfferingId,
                group.GroupCode,
                group.Capacity,
                group.EnrolledCount,
                group.RegistrationPaused,
                group.State.ToString(),
                group.IsSelectable,
                [],
                staffDtos,
                meetingDtos,
                Convert.ToBase64String(group.Version));
            result.Add(new StaffAssignment(
                course.Code,
                course.Title,
                groupDto,
                activeRole,
                rosterCounts.GetValueOrDefault(group.Id)));
        }

        return result;
    }

    private static bool TryRole(string activeRole, out TeachingRole teachingRole)
    {
        if (string.Equals(activeRole, "Lecturer", StringComparison.Ordinal))
        {
            teachingRole = TeachingRole.Lecturer;
            return true;
        }

        if (string.Equals(activeRole, "TeachingAssistant", StringComparison.Ordinal))
        {
            teachingRole = TeachingRole.TeachingAssistant;
            return true;
        }

        teachingRole = default;
        return false;
    }
}
