using System.Collections.Concurrent;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

internal static class Spec010Scenario
{
    public static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    public static OfferingSnapshot CompleteOffering(
        string state = "draft",
        int capacity = 30,
        int enrolledCount = 0,
        bool registrationPaused = false) =>
        new(
            Id(1),
            state,
            [
                new OfferingGroupSnapshot(
                    Id(4),
                    "G01",
                    capacity,
                    enrolledCount,
                    registrationPaused,
                    state,
                    [8],
                    [
                        Meeting(
                            Id(5),
                            ActivityType.Lecture,
                            TeachingRole.Lecturer,
                            "Dr Lecturer",
                            "C-101",
                            "Main Campus",
                            DayOfWeek.Sunday,
                            new(9, 0),
                            new(10, 0)),
                        Meeting(
                            Id(7),
                            ActivityType.Tutorial,
                            TeachingRole.TeachingAssistant,
                            "TA Assistant",
                            "C-201",
                            "Main Campus",
                            DayOfWeek.Tuesday,
                            new(11, 0),
                            new(12, 0))
                    ])
            ]);

    public static OfferingMeetingSnapshot Meeting(
        Guid id,
        ActivityType activity,
        TeachingRole? role,
        string staffName,
        string roomCode,
        string location,
        DayOfWeek day,
        TimeOnly start,
        TimeOnly end)
    {
        var meeting = new MeetingSlot(
            id,
            Id(4),
            Id(id == Id(5) ? 6 : 8),
            activity,
            day,
            start,
            end);
        return new(
            meeting.Id,
            meeting.ActivityType.ToString(),
            (int)meeting.DayOfWeek,
            meeting.StartLocal,
            meeting.EndLocal,
            roomCode,
            location,
            role is null
                ? []
                : [new OfferingStaffSnapshot(
                    Id(id == Id(5) ? 9 : 10),
                    role.Value.ToString(),
                    staffName)]);
    }

    public static OfferingPublicationSnapshot Publication(
        bool roomOverlap = false,
        bool bundleComplete = true,
        bool staffAvailable = true,
        byte[]? groupVersion = null,
        byte[]? roomVersion = null,
        byte[]? staffVersion = null) =>
        FromDomain(
            new CourseOffering(
                Id(1),
                Id(2),
                Id(3),
                CourseOfferingState.Draft),
            new SectionGroup(
                Id(4),
                Id(1),
                "G01",
                30,
                0,
                SectionGroupState.Draft,
                false),
            new Room(
                Id(6),
                "C-101",
                "Main Campus",
                35,
                RoomAvailabilityState.Available),
            new StaffTermAvailability(
                Id(11),
                Id(9),
                Id(2),
                new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                [
                    new StaffAvailability(
                        Id(12),
                        Id(11),
                        DayOfWeek.Sunday,
                        new(8, 0),
                        new(16, 0),
                        AvailabilityKind.Available)
                ]),
            roomOverlap,
            bundleComplete,
            staffAvailable,
            groupVersion ?? [1],
            roomVersion ?? [1],
            staffVersion ?? [1]);

    public static ValidateOfferingCommand ValidateCommand(
        OfferingPublicationSnapshot snapshot,
        byte[]? expectedGroupVersion = null,
        byte[]? expectedRoomVersion = null,
        byte[]? expectedStaffVersion = null) =>
        new(
            snapshot.OfferingId,
            snapshot.OfferingVersion,
            new Dictionary<Guid, byte[]>
            {
                [snapshot.GroupId] =
                    expectedGroupVersion ?? snapshot.GroupVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.RoomId] =
                    expectedRoomVersion ?? snapshot.CurrentRoomVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.StaffTermAvailabilityId] =
                    expectedStaffVersion ?? snapshot.StaffVersion
            });

    public static PublishOfferingCommand PublishCommand(
        OfferingPublicationSnapshot snapshot,
        string requestId = "publish-1") =>
        new(
            snapshot.OfferingId,
            snapshot.OfferingVersion,
            new Dictionary<Guid, byte[]>
            {
                [snapshot.GroupId] = snapshot.GroupVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.RoomId] = snapshot.CurrentRoomVersion
            },
            new Dictionary<Guid, byte[]>
            {
                [snapshot.StaffTermAvailabilityId] = snapshot.StaffVersion
            },
            "preview-token",
            requestId,
            "publish offering");

    private static OfferingPublicationSnapshot FromDomain(
        CourseOffering offering,
        SectionGroup group,
        Room room,
        StaffTermAvailability availability,
        bool roomOverlap,
        bool bundleComplete,
        bool staffAvailable,
        byte[] groupVersion,
        byte[] roomVersion,
        byte[] staffVersion) =>
        new(
            offering.Id,
            OfferingVersion: [1],
            group.Id,
            GroupVersion: groupVersion,
            GroupCapacity: group.Capacity,
            room.Id,
            RoomCapacity: room.Capacity,
            RoomAvailable:
                room.AvailabilityState is RoomAvailabilityState.Available,
            RoomOverlap: roomOverlap,
            CurrentRoomVersion: roomVersion,
            availability.Id,
            StaffAvailable: staffAvailable,
            StaffOverlap: false,
            StaffVersion: staffVersion,
            SlotValid: true,
            BundleComplete: bundleComplete);
}

internal sealed class PublicationStoreFake : IOfferingPublicationStore
{
    private readonly ConcurrentQueue<OfferingPublicationSnapshot> _snapshots = new();

    public OfferingPublicationSnapshot Snapshot { get; set; } =
        Spec010Scenario.Publication();
    public PublicationDependencies Dependencies { get; set; } =
        new(
            Spec010Scenario.Id(1),
            [Spec010Scenario.Id(4)],
            [Spec010Scenario.Id(6)],
            [Spec010Scenario.Id(11)]);
    public List<PublicationLockRequest> LockRequests { get; } = [];
    public bool InsideTransaction { get; set; }
    public int ValidationCallsInsideTransaction { get; private set; }

    public void Queue(params OfferingPublicationSnapshot[] snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            _snapshots.Enqueue(snapshot);
        }
    }

    public Task<PublicationDependencies> GetDependenciesAsync(
        Guid offeringId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Dependencies);

    public Task<OfferingPublicationSnapshot> LockAndLoadAsync(
        PublicationLockRequest request,
        CancellationToken cancellationToken)
    {
        lock (LockRequests)
        {
            LockRequests.Add(request);
        }
        if (InsideTransaction)
        {
            ValidationCallsInsideTransaction++;
        }

        return Task.FromResult(
            _snapshots.TryDequeue(out var snapshot) ? snapshot : Snapshot);
    }
}

internal sealed class PublicationTransactionFake(
    PublicationStoreFake store,
    int groupCount = 1) : IOfferingPublicationTransaction
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public string OfferingState { get; private set; } = "draft";
    public IReadOnlyList<string> GroupStates { get; private set; } =
        Enumerable.Repeat("draft", groupCount).ToArray();
    public List<OfferingPublicationAudit> Audits { get; } = [];
    public int CommitCount { get; private set; }
    public bool FailBeforeCommit { get; init; }

    public async Task<OfferingPublicationResult> ExecuteAsync(
        PublishOfferingCommand command,
        Func<
            IOfferingPublicationStore,
            OfferingPublicationState,
            CancellationToken,
            Task<OfferingPublicationResult>> operation,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var staged = new OfferingPublicationState(
                OfferingState,
                GroupStates.ToArray(),
                []);
            store.InsideTransaction = true;
            OfferingPublicationResult result;
            try
            {
                result = await operation(store, staged, cancellationToken);
            }
            finally
            {
                store.InsideTransaction = false;
            }

            if (result.Outcome != OfferingPublicationOutcome.Published)
            {
                return result;
            }
            if (FailBeforeCommit)
            {
                return new(
                    OfferingPublicationOutcome.StorageFailure,
                    "SCHEDULING_UNAVAILABLE");
            }

            OfferingState = staged.OfferingState;
            GroupStates = staged.GroupStates.ToArray();
            Audits.AddRange(staged.Audits);
            CommitCount++;
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }
}
