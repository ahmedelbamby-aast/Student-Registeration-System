using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Scheduling;

public sealed class ResourcePublicationRaceTests
{
    [Fact]
    public async Task Validation_rejects_conflicts_unavailability_capacity_and_stale_versions()
    {
        var store = new OfferingPublicationStoreFake
        {
            Snapshot = PublicationSnapshot() with
            {
                RoomAvailable = false,
                RoomOverlap = true,
                StaffAvailable = false,
                StaffOverlap = true,
                RoomCapacity = 20,
                GroupCapacity = 30,
                CurrentRoomVersion = [2]
            }
        };
        var validator = new OfferingPublicationValidator();

        var result = await validator.ValidateAsync(
            new ValidateOfferingCommand(
                store.Snapshot.OfferingId,
                store.Snapshot.OfferingVersion,
                new Dictionary<Guid, byte[]>
                {
                    [store.Snapshot.GroupId] = store.Snapshot.GroupVersion
                },
                new Dictionary<Guid, byte[]>
                {
                    [store.Snapshot.RoomId] = [1]
                },
                new Dictionary<Guid, byte[]>
                {
                    [store.Snapshot.StaffTermAvailabilityId] =
                        store.Snapshot.StaffVersion
                }),
            store);

        Assert.False(result.Valid);
        Assert.Equal(
            [
                "STALE_DEPENDENCY",
                "ROOM_CONFLICT",
                "ROOM_UNAVAILABLE",
                "ROOM_CAPACITY_TOO_SMALL",
                "STAFF_CONFLICT",
                "STAFF_UNAVAILABLE"
            ],
            result.ReasonCodes);
    }

    [Fact]
    public async Task Validator_requests_stable_lock_order_before_revalidation()
    {
        var store = new OfferingPublicationStoreFake
        {
            Dependencies = new(
                Id(90),
                [Id(3), Id(1)],
                [Id(8), Id(4)],
                [Id(7), Id(2)])
        };
        store.Snapshot = PublicationSnapshot() with
        {
            OfferingId = store.Dependencies.OfferingId,
            GroupId = Id(1),
            RoomId = Id(4),
            StaffTermAvailabilityId = Id(2)
        };
        var validator = new OfferingPublicationValidator();

        await validator.ValidateAsync(CurrentCommand(store.Snapshot), store);

        Assert.Equal(
            [
                store.Dependencies.OfferingId,
                Id(1),
                Id(3),
                Id(4),
                Id(8),
                Id(2),
                Id(7)
            ],
            Assert.Single(store.LockRequests).OrderedResourceIds);
    }

    [Fact]
    public async Task Changed_parent_group_version_is_revalidated_as_a_dependency()
    {
        var store = new OfferingPublicationStoreFake
        {
            Snapshot = PublicationSnapshot() with { GroupVersion = [2] }
        };
        var validator = new OfferingPublicationValidator();

        var result = await validator.ValidateAsync(
            new ValidateOfferingCommand(
                store.Snapshot.OfferingId,
                store.Snapshot.OfferingVersion,
                new Dictionary<Guid, byte[]> { [store.Snapshot.GroupId] = [1] },
                new Dictionary<Guid, byte[]>
                {
                    [store.Snapshot.RoomId] = store.Snapshot.CurrentRoomVersion
                },
                new Dictionary<Guid, byte[]>
                {
                    [store.Snapshot.StaffTermAvailabilityId] =
                        store.Snapshot.StaffVersion
                }),
            store);

        Assert.False(result.Valid);
        Assert.Contains("STALE_DEPENDENCY", result.ReasonCodes);
    }

    private static ValidateOfferingCommand CurrentCommand(
        OfferingPublicationSnapshot snapshot) =>
        new(
            snapshot.OfferingId,
            snapshot.OfferingVersion,
            new Dictionary<Guid, byte[]> { [snapshot.GroupId] = snapshot.GroupVersion },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.RoomId] = snapshot.CurrentRoomVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.StaffTermAvailabilityId] = snapshot.StaffVersion
            });

    private static OfferingPublicationSnapshot PublicationSnapshot() =>
        FromDomain(
            new CourseOffering(Id(90), Id(91), Id(92), CourseOfferingState.Draft),
            new SectionGroup(
                Id(1),
                Id(90),
                "G01",
                30,
                0,
                SectionGroupState.Draft,
                false),
            new Room(
                Id(4),
                "C-101",
                "Main Campus",
                30,
                RoomAvailabilityState.Available),
            new StaffTermAvailability(
                Id(2),
                Id(20),
                Id(91),
                new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                [
                    new StaffAvailability(
                        Id(21),
                        Id(2),
                        DayOfWeek.Monday,
                        new(8, 0),
                        new(12, 0),
                        AvailabilityKind.Available)
                ]));

    private static OfferingPublicationSnapshot FromDomain(
        CourseOffering offering,
        SectionGroup group,
        Room room,
        StaffTermAvailability staffAvailability) =>
        new(
            offering.Id,
            OfferingVersion: [1],
            group.Id,
            GroupVersion: [1],
            GroupCapacity: group.Capacity,
            room.Id,
            RoomCapacity: room.Capacity,
            RoomAvailable: room.AvailabilityState == RoomAvailabilityState.Available,
            RoomOverlap: false,
            CurrentRoomVersion: [1],
            staffAvailability.Id,
            StaffAvailable: true,
            StaffOverlap: false,
            StaffVersion: [1],
            SlotValid: true,
            BundleComplete: true);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private sealed class OfferingPublicationStoreFake : IOfferingPublicationStore
    {
        public PublicationDependencies Dependencies { get; set; } =
            new(Id(90), [Id(1)], [Id(4)], [Id(2)]);
        public OfferingPublicationSnapshot Snapshot { get; set; } =
            PublicationSnapshot();
        public List<PublicationLockRequest> LockRequests { get; } = [];

        public Task<PublicationDependencies> GetDependenciesAsync(
            Guid offeringId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Dependencies);

        public Task<OfferingPublicationSnapshot> LockAndLoadAsync(
            PublicationLockRequest request,
            CancellationToken cancellationToken)
        {
            LockRequests.Add(request);
            return Task.FromResult(Snapshot);
        }

    }
}
