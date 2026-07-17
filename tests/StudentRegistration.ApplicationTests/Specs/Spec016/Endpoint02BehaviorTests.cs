using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec016;

public sealed class Endpoint02BehaviorTests
{
    [Fact]
    public async Task Timetable_returns_the_current_role_context_and_equivalent_assignment_data()
    {
        var assignment = StaffWorkspaceTestData.Assignment();
        var reader = new StubStaffWorkspaceReader { Timetable = [assignment] };
        var queries = new StaffWorkspaceQueries(reader, new CapturingStaffWorkspaceAuditWriter());

        var result = await queries.GetTimetableAsync(
            StaffWorkspaceTestData.Principal(StaffWorkspaceContract.LecturerRole));

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, result.Outcome);
        Assert.Equal("Lecturer", result.Timetable!.RoleContext);
        var returned = Assert.Single(result.Timetable.Assignments);
        Assert.Equal(assignment.Group.Meetings, returned.Group.Meetings);
        Assert.Equal(assignment.Group.Staff, returned.Group.Staff);
    }

    [Fact]
    public async Task Timetable_refresh_reflects_assignment_removal_as_an_empty_current_scope()
    {
        var reader = new StubStaffWorkspaceReader
        {
            Timetable = [StaffWorkspaceTestData.Assignment()]
        };
        var queries = new StaffWorkspaceQueries(reader, new CapturingStaffWorkspaceAuditWriter());
        var principal = StaffWorkspaceTestData.Principal(StaffWorkspaceContract.LecturerRole);

        Assert.Single((await queries.GetTimetableAsync(principal)).Timetable!.Assignments);
        reader.Timetable = [];
        var refreshed = await queries.GetTimetableAsync(principal);

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, refreshed.Outcome);
        Assert.Empty(refreshed.Timetable!.Assignments);
    }
}
