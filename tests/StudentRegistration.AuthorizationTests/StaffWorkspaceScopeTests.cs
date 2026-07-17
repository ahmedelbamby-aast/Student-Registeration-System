using System.Security.Claims;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.AuthorizationTests;

public sealed class StaffWorkspaceScopeTests
{
    [Theory]
    [InlineData(StaffWorkspaceContract.LecturerRole)]
    [InlineData(StaffWorkspaceContract.TeachingAssistantRole)]
    public async Task Shared_workspace_uses_only_the_selected_teaching_context(string role)
    {
        var reader = new ScopeReader();
        var queries = new StaffWorkspaceQueries(reader, new ScopeAudit());

        var result = await queries.GetAssignmentsAsync(Principal(role));

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, result.Outcome);
        Assert.All(result.Assignments!, assignment => Assert.Equal(role, assignment.StaffRole));
        Assert.Equal(role, reader.LastRole);
    }

    [Fact]
    public async Task Dual_role_sessions_do_not_union_contexts_or_duplicate_groups()
    {
        var groupId = Guid.NewGuid();
        var reader = new ScopeReader(groupId);
        var queries = new StaffWorkspaceQueries(reader, new ScopeAudit());
        var principal = Principal(StaffWorkspaceContract.LecturerRole,
            StaffWorkspaceContract.LecturerRole,
            StaffWorkspaceContract.TeachingAssistantRole);

        var result = await queries.GetAssignmentsAsync(principal);

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, result.Outcome);
        Assert.Single(result.Assignments!);
        Assert.Equal("Lecturer", result.Assignments![0].StaffRole);
        Assert.Equal(groupId, result.Assignments[0].Group.Id);
    }

    [Fact]
    public async Task Direct_roster_access_is_denied_without_current_assignment_and_rows_are_minimal()
    {
        var reader = new ScopeReader { Roster = null };
        var queries = new StaffWorkspaceQueries(reader, new ScopeAudit());
        var denied = await queries.GetRosterAsync(
            Principal(StaffWorkspaceContract.TeachingAssistantRole),
            Guid.NewGuid(), 1, 20, "corr-denied");

        Assert.Equal(StaffWorkspaceQueryOutcome.NotFound, denied.Outcome);
        Assert.Null(denied.Page);
        Assert.Equal(
            ["DisplayName", "EnrollmentState", "UniversityId"],
            typeof(RosterRowDto).GetProperties().Select(property => property.Name)
                .Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public async Task Assignment_projection_contains_the_authorized_subject_group_partner_room_and_capacity_details()
    {
        var queries = new StaffWorkspaceQueries(new ScopeReader(), new ScopeAudit());

        var result = await queries.GetAssignmentsAsync(
            Principal(StaffWorkspaceContract.LecturerRole));

        var assignment = Assert.Single(result.Assignments!);
        Assert.Equal("CS401", assignment.SubjectCode);
        Assert.Equal("Distributed Systems", assignment.SubjectTitle);
        Assert.Equal("G01", assignment.Group.GroupCode);
        Assert.Equal(30, assignment.Group.Capacity);
        Assert.Single(assignment.Group.Staff);
        Assert.Equal("A101", Assert.Single(assignment.Group.Meetings).RoomCode);
        Assert.Equal(1, assignment.RosterCount);
    }

    [Fact]
    public async Task Assigned_roster_uses_the_exact_bounded_page_and_stable_sort_contract()
    {
        var queries = new StaffWorkspaceQueries(new ScopeReader(), new ScopeAudit());

        var result = await queries.GetRosterAsync(
            Principal(StaffWorkspaceContract.TeachingAssistantRole),
            Guid.NewGuid(),
            page: 1,
            pageSize: StaffWorkspaceContract.DefaultPageSize,
            correlationId: "corr-success");

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, result.Outcome);
        Assert.Equal(StaffWorkspaceContract.DefaultPageSize, result.Page!.PageSize);
        Assert.Equal(StaffWorkspaceContract.RosterSort, result.Page.Sort);
        Assert.InRange(result.Page.PageSize, 1, StaffWorkspaceContract.MaximumPageSize);
        Assert.Equal(
            ["DisplayName", "EnrollmentState", "UniversityId"],
            result.Page.Items[0].GetType().GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public async Task Non_teaching_roles_cannot_reach_workspace_reads_or_admin_mutations()
    {
        var reader = new ScopeReader();
        var queries = new StaffWorkspaceQueries(reader, new ScopeAudit());

        var admin = await queries.GetAssignmentsAsync(Principal("Admin"));
        var student = await queries.GetTimetableAsync(Principal("Student"));

        Assert.Equal(StaffWorkspaceQueryOutcome.Forbidden, admin.Outcome);
        Assert.Equal(StaffWorkspaceQueryOutcome.Forbidden, student.Outcome);
        Assert.Equal(0, reader.Calls);
        Assert.DoesNotContain(
            typeof(StaffWorkspaceQueries).GetMethods(),
            method => method.Name.Contains("Manage", StringComparison.Ordinal)
                || method.Name.Contains("Capacity", StringComparison.Ordinal)
                || method.Name.Contains("Policy", StringComparison.Ordinal)
                || method.Name.Contains("Term", StringComparison.Ordinal));
    }

    private static ClaimsPrincipal Principal(string activeRole, params string[] availableRoles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, activeRole)
        };
        claims.AddRange(availableRoles.Select(role => new Claim("available_role", role)));
        return new(new ClaimsIdentity(claims, "test"));
    }

    private sealed class ScopeReader : IStaffWorkspaceReader
    {
        private readonly Guid _groupId;
        public ScopeReader(Guid? groupId = null) => _groupId = groupId ?? Guid.NewGuid();
        public int Calls { get; private set; }
        public string? LastRole { get; private set; }
        public StaffRosterPageSnapshot? Roster { get; set; } = new(
            [new RosterRow("20260001", "Ada Lovelace", "active")], 1);

        public Task<IReadOnlyList<StaffAssignment>> ListAssignmentsAsync(Guid actorApplicationUserId, string activeRole, CancellationToken cancellationToken = default)
        {
            Calls++; LastRole = activeRole;
            var assignment = Assignment(activeRole);
            return Task.FromResult<IReadOnlyList<StaffAssignment>>([assignment, assignment]);
        }

        public Task<IReadOnlyList<StaffAssignment>> ListTimetableAsync(Guid actorApplicationUserId, string activeRole, CancellationToken cancellationToken = default)
        { Calls++; LastRole = activeRole; return Task.FromResult<IReadOnlyList<StaffAssignment>>([Assignment(activeRole)]); }

        public Task<StaffRosterPageSnapshot?> ReadRosterIfAssignedAsync(Guid actorApplicationUserId, string activeRole, Guid groupId, int page, int pageSize, string sort, CancellationToken cancellationToken = default)
        { Calls++; LastRole = activeRole; return Task.FromResult(Roster); }

        private StaffAssignment Assignment(string role)
        {
            var activity = role == StaffWorkspaceContract.LecturerRole ? "Lecture" : "Tutorial";
            var meetingId = Guid.NewGuid();
            var group = new GroupDto(_groupId, Guid.NewGuid(), "G01", 30, 1,
                false, "published", true, [],
                [new(meetingId, activity, Guid.NewGuid(), role, "Staff")],
                [new(meetingId, activity, DayOfWeek.Monday, new(9, 0), new(10, 0),
                    Guid.NewGuid(), "A101", "Campus")], "group-v1");
            return new("CS401", "Distributed Systems", group, role, 1);
        }
    }

    private sealed class ScopeAudit : IStaffWorkspaceAuditWriter
    {
        public Task WriteRosterAccessAsync(StaffRosterAuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
