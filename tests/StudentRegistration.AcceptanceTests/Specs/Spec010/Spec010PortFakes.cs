using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

internal sealed class OfferingStoreFake : IOfferingStore
{
    public OfferingSnapshot Snapshot { get; set; } =
        Spec010Scenario.CompleteOffering();
    public List<CreateOfferingStoreCommand> Creates { get; } = [];
    public List<UpdateGroupStoreCommand> Updates { get; } = [];
    public List<AdminOfferingQuery> ListQueries { get; } = [];

    public Task<OfferingSnapshot?> LoadAsync(
        Guid offeringId,
        CancellationToken cancellationToken) =>
        Task.FromResult<OfferingSnapshot?>(
            Snapshot.Id == offeringId ? Snapshot : null);

    public Task<OfferingGroupSnapshot?> LoadGroupAsync(
        Guid groupId,
        CancellationToken cancellationToken) =>
        Task.FromResult<OfferingGroupSnapshot?>(
            Snapshot.Groups.SingleOrDefault(g => g.Id == groupId));

    public Task<OfferingSnapshot> CreateAsync(
        CreateOfferingStoreCommand command,
        CancellationToken cancellationToken)
    {
        Creates.Add(command);
        Snapshot = new(
            Spec010Scenario.Id(20),
            "draft",
            command.Groups.Select(
                (group, index) => new OfferingGroupSnapshot(
                    Spec010Scenario.Id(21 + index),
                    group.Code,
                    group.Capacity,
                    0,
                    false,
                    "draft",
                    [1],
                    [])).ToArray());
        return Task.FromResult(Snapshot);
    }

    public Task<OfferingGroupSnapshot> UpdateGroupAsync(
        UpdateGroupStoreCommand command,
        CancellationToken cancellationToken)
    {
        Updates.Add(command);
        var group = Snapshot.Groups.Single(g => g.Id == command.GroupId);
        var updatedGroup = group with
        {
            GroupCode = command.GroupCode,
            Capacity = command.Capacity,
            RegistrationPaused = command.RegistrationPaused,
            RowVersion = [2]
        };
        var newGroups = Snapshot.Groups.Select(g => g.Id == command.GroupId ? updatedGroup : g).ToArray();
        Snapshot = Snapshot with { Groups = newGroups };
        return Task.FromResult(updatedGroup);
    }

    public Task<AdminOfferingPage> ListAsync(
        AdminOfferingQuery query,
        CancellationToken cancellationToken)
    {
        ListQueries.Add(query);
        return Task.FromResult(
            new AdminOfferingPage(
                [Snapshot],
                query.Page,
                query.PageSize,
                TotalCount: 250,
                Sort: query.Sort ?? "courseCode,id"));
    }
}

internal sealed class CapacityStoreFake(
    SectionGroup group,
    byte[]? version = null) : ISectionGroupCapacityStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private SectionGroup _group = group;

    public Guid GroupId => _group.Id;
    public int Capacity => _group.Capacity;
    public int EnrolledCount => _group.EnrolledCount;
    public byte[] Version { get; private set; } = version ?? [1];
    public int CommitCount { get; private set; }

    public Task<GroupCapacitySnapshot?> ReadAsync(
        Guid groupId,
        CancellationToken cancellationToken) =>
        Task.FromResult<GroupCapacitySnapshot?>(
            groupId == GroupId
                ? new(
                    GroupId,
                    _group.State.ToString().ToLowerInvariant(),
                    Capacity,
                    EnrolledCount,
                    _group.RegistrationPaused,
                    Version)
                : null);

    public async Task<GroupCapacityOutcome> ExecuteAsync(
        Guid groupId,
        byte[] expectedVersion,
        Func<GroupCapacityState, GroupCapacityOutcome> operation,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (groupId != GroupId || !expectedVersion.SequenceEqual(Version))
            {
                return GroupCapacityOutcome.StaleVersion;
            }

            var staged = new GroupCapacityState(Capacity, EnrolledCount);
            var outcome = operation(staged);
            if (outcome != GroupCapacityOutcome.Applied)
            {
                return outcome;
            }

            _group = new SectionGroup(
                _group.Id,
                _group.OfferingId,
                _group.GroupCode,
                staged.Capacity,
                staged.EnrolledCount,
                _group.State,
                _group.RegistrationPaused);
            Version = [(byte)(Version[0] + 1)];
            CommitCount++;
            return outcome;
        }
        finally
        {
            _gate.Release();
        }
    }
}

internal sealed class ResourceAvailabilityStoreFake : IResourceAvailabilityStore
{
    private StaffTermAvailability _availability = new(
        Spec010Scenario.Id(11),
        Spec010Scenario.Id(9),
        Spec010Scenario.Id(2),
        new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
        [
            new StaffAvailability(
                Spec010Scenario.Id(12),
                Spec010Scenario.Id(11),
                DayOfWeek.Sunday,
                new(8, 0),
                new(16, 0),
                AvailabilityKind.Available)
        ]);
    private Room _room = new(
        Spec010Scenario.Id(6),
        "C-101",
        "Main Campus",
        35,
        RoomAvailabilityState.Available);

    public Guid OwnerStaffId => _availability.StaffId;
    public Guid TermId => _availability.TermId;
    public Guid AvailabilityId => _availability.Id;
    public Guid RoomId => _room.Id;
    public byte[] AvailabilityVersion { get; private set; } = [1];
    public byte[] RoomVersion { get; private set; } = [1];
    public bool AffectsPublishedGroup { get; init; }
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
                    RoomId,
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
        _availability = new StaffTermAvailability(
            _availability.Id,
            _availability.StaffId,
            _availability.TermId,
            _availability.DeadlineUtc,
            write.Ranges.Select((range, index) =>
                new StaffAvailability(
                    Spec010Scenario.Id(30 + index),
                    _availability.Id,
                    (DayOfWeek)range.DayOfWeek,
                    range.StartLocal,
                    range.EndLocal,
                    Enum.Parse<AvailabilityKind>(
                        range.Kind,
                        ignoreCase: true))).ToArray());
        AvailabilityVersion = [(byte)(AvailabilityVersion[0] + 1)];
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
        RoomVersion = [(byte)(RoomVersion[0] + 1)];
        return Task.CompletedTask;
    }
}
