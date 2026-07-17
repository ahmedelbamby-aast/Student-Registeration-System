using System.Reflection;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.StaffWorkspace;

public sealed class AvailabilityPublicationRaceTests
{
    [Fact]
    public void Conflicting_published_assignment_is_reported_without_moving_the_group()
    {
        var group = PublishedGroup();
        var aggregate = Availability();
        var before = group.Meetings.Select(meeting => (
            meeting.RoomId,
            meeting.DayOfWeek,
            meeting.StartLocal,
            meeting.EndLocal)).ToArray();
        var meeting = Assert.Single(group.Meetings);
        var command = new ReplaceOwnStaffAvailability(
            aggregate.StaffId,
            aggregate.TermId,
            [1],
            [new(Guid.NewGuid(), DayOfWeek.Monday, new(8, 0), new(9, 0), AvailabilityKind.Available)],
            "reduced availability",
            "correlation-1");

        var decision = StaffAvailabilityTransactionRules.Apply(
            aggregate,
            command,
            aggregate.DeadlineUtc.AddMinutes(-1),
            [new(group.Id, [8], meeting.DayOfWeek, meeting.StartLocal, meeting.EndLocal)]);

        Assert.Equal(StaffAvailabilityPortOutcome.Success, decision.Outcome);
        Assert.Equal(group.Id, Assert.Single(decision.ImpactedMeetings).GroupId);
        Assert.Equal(SectionGroupState.Published, group.State);
        Assert.Equal(before, group.Meetings.Select(item => (
            item.RoomId,
            item.DayOfWeek,
            item.StartLocal,
            item.EndLocal)));
    }

    [Fact]
    public void Production_adapter_uses_one_SQL_transaction_for_availability_alert_and_audit()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Scheduling/SqlStaffAvailabilityPort.cs");

        RepositoryFiles.ContainsAll(
            source,
            "IsolationLevel.Serializable",
            "UPDLOCK, HOLDLOCK",
            "StaffAvailabilityTransactionRules.Apply",
            "new ScheduleImpactAlert",
            "dbContext.AuditEvents.Add",
            "transaction.CommitAsync",
            "transaction.RollbackAsync",
            "DbUpdateConcurrencyException");
        Assert.DoesNotContain("MarkPublished", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ReplaceSchedule", source, StringComparison.Ordinal);

        var transaction = source.IndexOf("BeginTransactionAsync", StringComparison.Ordinal);
        var scheduleRead = source.IndexOf("LoadPublishedMeetings", transaction, StringComparison.Ordinal);
        var staffLock = source.IndexOf("UPDLOCK, HOLDLOCK", scheduleRead, StringComparison.Ordinal);
        var rules = source.IndexOf("StaffAvailabilityTransactionRules.Apply", staffLock, StringComparison.Ordinal);
        var alert = source.IndexOf("AddMissingImpactAlerts", rules, StringComparison.Ordinal);
        var commit = source.IndexOf("transaction.CommitAsync", alert, StringComparison.Ordinal);
        Assert.True(transaction < scheduleRead && scheduleRead < staffLock && staffLock < rules && rules < alert && alert < commit);
    }

    [Fact]
    public void Staff_workspace_exposes_only_alert_IDs_from_the_Scheduling_result()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var result = new StaffAvailabilityPortResult(
            StaffAvailabilityPortOutcome.Success,
            null,
            ids,
            DateTime.UtcNow,
            null,
            null);

        Assert.Equal(ids, ScheduleImpactQuery.From(result));
    }

    private static StaffTermAvailability Availability()
    {
        var id = Guid.NewGuid();
        var aggregate = new StaffTermAvailability(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 31, 12, 0, 0, DateTimeKind.Utc),
            [new StaffAvailability(Guid.NewGuid(), id, DayOfWeek.Monday, new(8, 0), new(12, 0), AvailabilityKind.Available)]);
        typeof(StaffTermAvailability).GetProperty(nameof(StaffTermAvailability.Version))!
            .SetValue(aggregate, new byte[] { 1 });
        return aggregate;
    }

    private static SectionGroup PublishedGroup()
    {
        var group = new SectionGroup(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "G01",
            30,
            0,
            SectionGroupState.Published,
            false);
        var meeting = new MeetingSlot(
            Guid.NewGuid(),
            group.Id,
            Guid.NewGuid(),
            ActivityType.Lecture,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0));
        group.ReplaceSchedule(
            [meeting],
            [new GroupStaffAssignment(group.Id, meeting.Id, ActivityType.Lecture, Guid.NewGuid(), TeachingRole.Lecturer)]);
        return group;
    }
}
