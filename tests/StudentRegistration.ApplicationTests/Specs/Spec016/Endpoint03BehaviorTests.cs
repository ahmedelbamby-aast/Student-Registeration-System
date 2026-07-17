using StudentRegistration.Contracts.Staff;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.ApplicationTests.Specs.Spec016;

public sealed class Endpoint03BehaviorTests
{
    [Fact]
    public async Task Roster_validates_bounds_and_denies_unassigned_scope_before_materializing_rows()
    {
        var reader = new StubStaffWorkspaceReader();
        var audit = new CapturingStaffWorkspaceAuditWriter();
        var queries = new StaffWorkspaceQueries(reader, audit);
        var principal = StaffWorkspaceTestData.Principal(
            StaffWorkspaceContract.TeachingAssistantRole);

        var invalid = await queries.GetRosterAsync(
            principal, Guid.NewGuid(), 1, 101, "corr-invalid");
        Assert.Equal(StaffWorkspaceQueryOutcome.Invalid, invalid.Outcome);
        Assert.Equal(0, reader.RosterCalls);

        var denied = await queries.GetRosterAsync(
            principal, Guid.NewGuid(), 1, 20, "corr-denied");
        Assert.Equal(StaffWorkspaceQueryOutcome.NotFound, denied.Outcome);
        Assert.Equal("STAFF_GROUP_NOT_FOUND", denied.ErrorCode);
        Assert.Single(audit.Entries);
        Assert.Equal(StaffRosterAuditOutcome.Denied, audit.Entries[0].Outcome);
        Assert.Equal(0, audit.Entries[0].RowCount);
    }

    [Fact]
    public async Task Roster_returns_only_sorted_minimal_rows_and_audits_metadata()
    {
        var reader = new StubStaffWorkspaceReader
        {
            Roster = new StaffRosterPageSnapshot(
                [
                    new RosterRow("20260002", "Grace Hopper", "active"),
                    new RosterRow("20260003", "Ada Lovelace", "active"),
                    new RosterRow("20260001", "Ada Lovelace", "active")
                ],
                3)
        };
        var audit = new CapturingStaffWorkspaceAuditWriter();
        var queries = new StaffWorkspaceQueries(reader, audit);
        var groupId = Guid.NewGuid();

        var result = await queries.GetRosterAsync(
            StaffWorkspaceTestData.Principal(StaffWorkspaceContract.LecturerRole),
            groupId,
            1,
            20,
            "corr-success");

        Assert.Equal(StaffWorkspaceQueryOutcome.Succeeded, result.Outcome);
        Assert.Equal(["20260001", "20260003", "20260002"],
            result.Page!.Items.Select(item => item.UniversityId).ToArray());
        Assert.Equal(StaffWorkspaceContract.RosterSort, reader.LastSort);
        Assert.Equal(StaffWorkspaceContract.RosterSort, result.Page.Sort);
        var entry = Assert.Single(audit.Entries);
        Assert.Equal(groupId, entry.GroupId);
        Assert.Equal(3, entry.RowCount);
        Assert.Equal("corr-success", entry.CorrelationId);
        Assert.Equal(StaffRosterAuditOutcome.Succeeded, entry.Outcome);
        Assert.DoesNotContain("Ada", entry.ToString(), StringComparison.Ordinal);
    }
}
