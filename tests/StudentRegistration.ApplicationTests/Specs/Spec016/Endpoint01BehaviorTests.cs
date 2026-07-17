using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec016;

public sealed class Endpoint01BehaviorTests
{
    [Fact]
    public async Task Assignments_use_authenticated_active_staff_context_and_fail_closed_on_scope_drift()
    {
        var reader = new StubStaffWorkspaceReader
        {
            Assignments = [StaffWorkspaceTestData.Assignment()]
        };
        var queries = new StaffWorkspaceQueries(reader, new CapturingStaffWorkspaceAuditWriter());
        var actorId = Guid.NewGuid();

        var result = await queries.GetAssignmentsAsync(
            StaffWorkspaceTestData.Principal(StaffWorkspaceContract.LecturerRole, actorId));

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, result.Outcome);
        Assert.Single(result.Assignments!);
        Assert.Equal(actorId, reader.LastActorId);
        Assert.Equal("Lecturer", reader.LastRole);

        reader.Assignments = [StaffWorkspaceTestData.Assignment(
            StaffWorkspaceContract.TeachingAssistantRole)];
        var drift = await queries.GetAssignmentsAsync(
            StaffWorkspaceTestData.Principal(StaffWorkspaceContract.LecturerRole));
        Assert.Equal(StaffWorkspaceQueryOutcome.Unavailable, drift.Outcome);
        Assert.Equal("STAFF_SCOPE_UNAVAILABLE", drift.ErrorCode);
    }

    [Fact]
    public async Task Assignments_reject_anonymous_and_non_teaching_contexts_before_reading()
    {
        var reader = new StubStaffWorkspaceReader();
        var queries = new StaffWorkspaceQueries(reader, new CapturingStaffWorkspaceAuditWriter());

        var anonymous = await queries.GetAssignmentsAsync(new System.Security.Claims.ClaimsPrincipal());
        var admin = await queries.GetAssignmentsAsync(StaffWorkspaceTestData.Principal("Admin"));

        Assert.Equal(StaffWorkspaceQueryOutcome.Unauthorized, anonymous.Outcome);
        Assert.Equal(StaffWorkspaceQueryOutcome.Forbidden, admin.Outcome);
        Assert.Equal(0, reader.AssignmentCalls);
    }
}
