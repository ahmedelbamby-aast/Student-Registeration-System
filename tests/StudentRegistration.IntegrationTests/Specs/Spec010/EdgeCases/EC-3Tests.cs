using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public async Task Staff_unavailability_after_publication_requests_alert_without_moving_class()
    {
        var group = PublishedGroup();
        var before = group.Meetings
            .Select(meeting => (
                meeting.RoomId,
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal))
            .ToArray();
        var store = new ResourceStoreFake { AffectsPublishedGroup = true };
        var service = new ResourceAvailabilityService(store);

        var result = await service.ReplaceOwnAvailabilityAsync(
            store.OwnerStaffId,
            store.AvailabilityId,
            [1],
            [new(1, new(9, 0), new(12, 0), "unavailable")],
            new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc),
            "staff became unavailable");

        Assert.Equal(ResourceAvailabilityOutcome.Applied, result.Outcome);
        var write = Assert.Single(store.Writes);
        Assert.True(write.CreateImpactAlert);
        Assert.True(write.AppendAudit);
        Assert.Equal([2], store.AvailabilityVersion);
        Assert.Equal(SectionGroupState.Published, group.State);
        Assert.Equal(
            before,
            group.Meetings.Select(meeting => (
                meeting.RoomId,
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal)));
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
            [
                new(
                    group.Id,
                    meeting.Id,
                    ActivityType.Lecture,
                    Guid.NewGuid(),
                    TeachingRole.Lecturer)
            ]);
        return group;
    }

    private sealed class ResourceStoreFake : IResourceAvailabilityStore
    {
        public Guid OwnerStaffId { get; } = Guid.NewGuid();
        public Guid TermId { get; } = Guid.NewGuid();
        public Guid RoomId { get; } = Guid.NewGuid();
        public Guid AvailabilityId { get; } = Guid.NewGuid();
        public bool AffectsPublishedGroup { get; init; }
        public byte[] AvailabilityVersion { get; private set; } = [1];
        public List<ReplaceStaffAvailabilityWrite> Writes { get; } = [];

        public Task<StaffAvailabilitySnapshot?> LoadStaffAvailabilityAsync(
            Guid staffId,
            Guid termId,
            CancellationToken cancellationToken) =>
            Task.FromResult<StaffAvailabilitySnapshot?>(
                staffId == OwnerStaffId && termId == TermId
                    ? new(
                        AvailabilityId,
                        OwnerStaffId,
                        TermId,
                        AvailabilityVersion,
                        [new(1, new(8, 0), new(14, 0), "available")],
                        AffectsPublishedGroup)
                    : null);

        public Task<StaffAvailabilitySnapshot?> LoadStaffAvailabilityByIdAsync(
            Guid availabilityId,
            CancellationToken cancellationToken) =>
            LoadStaffAvailabilityAsync(
                availabilityId == AvailabilityId ? OwnerStaffId : Guid.Empty,
                TermId,
                cancellationToken);

        public Task<RoomResourceSnapshot?> LoadRoomAsync(
            Guid roomId,
            CancellationToken cancellationToken) =>
            Task.FromResult<RoomResourceSnapshot?>(null);

        public Task CommitAvailabilityAsync(
            ReplaceStaffAvailabilityWrite write,
            CancellationToken cancellationToken)
        {
            Writes.Add(write);
            AvailabilityVersion = [2];
            return Task.CompletedTask;
        }

        public Task CommitRoomAsync(
            UpdateRoomResourceWrite write,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
