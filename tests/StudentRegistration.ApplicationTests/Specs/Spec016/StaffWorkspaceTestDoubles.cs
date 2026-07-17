using System.Security.Claims;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.ApplicationTests.Specs.Spec016;

internal sealed class StubStaffWorkspaceReader : IStaffWorkspaceReader
{
    public IReadOnlyList<StaffAssignment> Assignments { get; set; } = [];
    public IReadOnlyList<StaffAssignment> Timetable { get; set; } = [];
    public StaffRosterPageSnapshot? Roster { get; set; }
    public int AssignmentCalls { get; private set; }
    public int TimetableCalls { get; private set; }
    public int RosterCalls { get; private set; }
    public Guid LastActorId { get; private set; }
    public string? LastRole { get; private set; }
    public Guid LastGroupId { get; private set; }
    public int LastPage { get; private set; }
    public int LastPageSize { get; private set; }
    public string? LastSort { get; private set; }

    public Task<IReadOnlyList<StaffAssignment>> ListAssignmentsAsync(
        Guid actorApplicationUserId,
        string activeRole,
        CancellationToken cancellationToken = default)
    {
        AssignmentCalls++;
        Capture(actorApplicationUserId, activeRole);
        return Task.FromResult(Assignments);
    }

    public Task<IReadOnlyList<StaffAssignment>> ListTimetableAsync(
        Guid actorApplicationUserId,
        string activeRole,
        CancellationToken cancellationToken = default)
    {
        TimetableCalls++;
        Capture(actorApplicationUserId, activeRole);
        return Task.FromResult(Timetable);
    }

    public Task<StaffRosterPageSnapshot?> ReadRosterIfAssignedAsync(
        Guid actorApplicationUserId,
        string activeRole,
        Guid groupId,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default)
    {
        RosterCalls++;
        Capture(actorApplicationUserId, activeRole);
        LastGroupId = groupId;
        LastPage = page;
        LastPageSize = pageSize;
        LastSort = sort;
        return Task.FromResult(Roster);
    }

    private void Capture(Guid actorId, string role)
    {
        LastActorId = actorId;
        LastRole = role;
    }
}

internal sealed class CapturingStaffWorkspaceAuditWriter : IStaffWorkspaceAuditWriter
{
    public List<StaffRosterAuditEntry> Entries { get; } = [];

    public Task WriteRosterAccessAsync(
        StaffRosterAuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }
}

internal static class StaffWorkspaceTestData
{
    public static ClaimsPrincipal Principal(string role, Guid? id = null) => new(
        new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, (id ?? Guid.NewGuid()).ToString()),
                new Claim(ClaimTypes.Role, role)
            ],
            "test"));

    public static StaffAssignment Assignment(
        string role = StaffWorkspaceContract.LecturerRole,
        Guid? groupId = null,
        string groupCode = "L01")
    {
        var meetingId = Guid.NewGuid();
        var group = new GroupDto(
            groupId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            groupCode,
            30,
            17,
            false,
            "published",
            true,
            [],
            [new(meetingId, "Lecture", Guid.NewGuid(), role, "Staff Member")],
            [new(meetingId, "Lecture", DayOfWeek.Monday, new(9, 0), new(10, 30),
                Guid.NewGuid(), "A101", "Main Campus")],
            "group-v1");
        return new("CS401", "Distributed Systems", group, role, 17);
    }
}
