using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Scheduling;

public sealed class ResourceAvailabilityTests
{
    [Fact]
    public async Task Staff_replaces_only_own_complete_range_set_while_admin_reads()
    {
        var store = new ResourceAvailabilityStoreFake();
        var service = new ResourceAvailabilityService(store);
        var ranges = new[]
        {
            new AvailabilityRangeInput(1, new(8, 0), new(12, 0), "available"),
            new AvailabilityRangeInput(3, new(10, 0), new(14, 0), "unavailable")
        };

        var applied = await service.ReplaceOwnAvailabilityAsync(
            store.OwnerStaffId,
            store.AvailabilityId,
            [1],
            ranges,
            ChangedAtUtc,
            "staff availability update");
        var denied = await service.ReplaceOwnAvailabilityAsync(
            Id(99),
            store.AvailabilityId,
            [2],
            ranges,
            ChangedAtUtc,
            "admin-like override");
        var adminView = await service.GetAdminPlanningInputAsync(
            store.OwnerStaffId,
            store.TermId);

        Assert.Equal(ResourceAvailabilityOutcome.Applied, applied.Outcome);
        Assert.Equal(ResourceAvailabilityOutcome.Forbidden, denied.Outcome);
        Assert.Equal(2, adminView!.Ranges.Count);
        Assert.Equal([2], adminView.RowVersion);
        Assert.Single(store.AvailabilityWrites);
        Assert.True(Assert.Single(store.AvailabilityWrites).AppendAudit);
    }

    [Fact]
    public async Task Published_resource_change_requests_durable_alert_and_atomic_audit()
    {
        var store = new ResourceAvailabilityStoreFake
        {
            AffectsPublishedGroup = true
        };
        var service = new ResourceAvailabilityService(store);

        var room = await service.UpdateRoomAsync(
            store.RoomId,
            [1],
            20,
            RoomAvailabilityState.Available,
            "admin-1",
            "capacity correction");
        var availability = await service.ReplaceOwnAvailabilityAsync(
            store.OwnerStaffId,
            store.AvailabilityId,
            [1],
            [new(1, new(8, 0), new(9, 0), "unavailable")],
            ChangedAtUtc,
            "availability correction");

        Assert.Equal(ResourceAvailabilityOutcome.Applied, room.Outcome);
        Assert.Equal(ResourceAvailabilityOutcome.Applied, availability.Outcome);
        Assert.True(Assert.Single(store.RoomWrites).CreateImpactAlert);
        Assert.True(Assert.Single(store.RoomWrites).AppendAudit);
        Assert.True(Assert.Single(store.AvailabilityWrites).CreateImpactAlert);
        Assert.True(Assert.Single(store.AvailabilityWrites).AppendAudit);
    }

    [Fact]
    public void Application_surface_has_no_Admin_availability_override_command()
    {
        var methods = typeof(ResourceAvailabilityService)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(ResourceAvailabilityService))
            .Select(method => method.Name)
            .ToArray();

        Assert.DoesNotContain(
            methods,
            name => name.Contains("AdminOverride", StringComparison.Ordinal) ||
                name.Contains("AdminReplace", StringComparison.Ordinal) ||
                name.Contains("AdminUpdateAvailability", StringComparison.Ordinal));
    }

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
    private static readonly DateTime ChangedAtUtc =
        new(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);

    private sealed class ResourceAvailabilityStoreFake : IResourceAvailabilityStore
    {
        public Guid OwnerStaffId { get; } = Id(1);
        public Guid TermId { get; } = Id(2);
        public Guid RoomId { get; } = Id(3);
        public Guid AvailabilityId => _availability.Id;
        private StaffTermAvailability _availability = new(
            Id(4),
            Id(1),
            Id(2),
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            [
                new StaffAvailability(
                    Id(5),
                    Id(4),
                    DayOfWeek.Monday,
                    new(8, 0),
                    new(12, 0),
                    AvailabilityKind.Available)
            ]);
        private Room _room = new(
            Id(3),
            "C-101",
            "Main Campus",
            30,
            RoomAvailabilityState.Available);
        public byte[] AvailabilityVersion { get; private set; } = [1];
        public byte[] RoomVersion { get; private set; } = [1];
        public bool AffectsPublishedGroup { get; init; }
        public List<AvailabilityRangeInput> Ranges { get; } = [];
        public List<ReplaceStaffAvailabilityWrite> AvailabilityWrites { get; } = [];
        public List<UpdateRoomResourceWrite> RoomWrites { get; } = [];

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
                        _availability.Ranges.Select(
                            range => new AvailabilityRangeInput(
                                (int)range.DayOfWeek,
                                range.StartLocal,
                                range.EndLocal,
                                range.Kind.ToString().ToLowerInvariant())).ToArray(),
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
            Task.FromResult<RoomResourceSnapshot?>(
                roomId == RoomId
                    ? new(
                        _room.Id,
                        RoomVersion,
                        _room.Capacity,
                        _room.AvailabilityState.ToString().ToLowerInvariant(),
                        AffectsPublishedGroup)
                    : null);

        public Task CommitAvailabilityAsync(
            ReplaceStaffAvailabilityWrite write,
            CancellationToken cancellationToken)
        {
            AvailabilityWrites.Add(write);
            Ranges.Clear();
            Ranges.AddRange(write.Ranges);
            _availability = new StaffTermAvailability(
                _availability.Id,
                _availability.StaffId,
                _availability.TermId,
                _availability.DeadlineUtc,
                write.Ranges.Select((range, index) =>
                    new StaffAvailability(
                        Id(10 + index),
                        _availability.Id,
                        (DayOfWeek)range.DayOfWeek,
                        range.StartLocal,
                        range.EndLocal,
                        Enum.Parse<AvailabilityKind>(
                            range.Kind,
                            ignoreCase: true))).ToArray());
            AvailabilityVersion = [2];
            return Task.CompletedTask;
        }

        public Task CommitRoomAsync(
            UpdateRoomResourceWrite write,
            CancellationToken cancellationToken)
        {
            RoomWrites.Add(write);
            _room.Update(
                _room.Code,
                _room.Location,
                write.Capacity,
                Enum.Parse<RoomAvailabilityState>(
                    write.State,
                    ignoreCase: true));
            RoomVersion = [2];
            return Task.CompletedTask;
        }
    }
}
