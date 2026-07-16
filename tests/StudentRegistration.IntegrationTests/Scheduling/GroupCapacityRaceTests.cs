using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Scheduling;

public sealed class GroupCapacityRaceTests
{
    [Theory]
    [InlineData("draft", 10, 0, false, "GROUP_UNPUBLISHED")]
    [InlineData("closed", 10, 0, false, "GROUP_CLOSED")]
    [InlineData("cancelled", 10, 0, false, "GROUP_CANCELLED")]
    [InlineData("published", 10, 10, false, "GROUP_FULL")]
    [InlineData("published", 10, 0, true, "REGISTRATION_PAUSED")]
    public async Task Selection_uses_scheduling_state_and_capacity_only(
        string state,
        int capacity,
        int enrolled,
        bool paused,
        string expectedReason)
    {
        var store = new CapacityStoreFake(state, capacity, enrolled, paused);
        var service = new SectionGroupCapacityService(store);

        var result = await service.ReadSelectionAsync(store.GroupId);

        Assert.False(result.Selectable);
        Assert.Equal([expectedReason], result.ReasonCodes);
        Assert.DoesNotContain("GROUP_CHANGED", result.ReasonCodes);
        Assert.DoesNotContain(
            result.ReasonCodes,
            reason => reason.Contains("PREREQUISITE", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Capacity_edit_and_seat_allocation_serialize_on_one_group_version()
    {
        var store = new CapacityStoreFake("published", 2, 1, false);
        var service = new SectionGroupCapacityService(store);

        var outcomes = await Task.WhenAll(
            service.ChangeCapacityAsync(
                store.GroupId,
                [1],
                1,
                "admin-1",
                "align capacity"),
            service.AllocateSeatAsync(store.GroupId, [1]));

        Assert.Single(outcomes, outcome => outcome == GroupCapacityOutcome.Applied);
        Assert.Single(
            outcomes,
            outcome => outcome is GroupCapacityOutcome.StaleVersion or
                GroupCapacityOutcome.GroupFull);
        Assert.InRange(store.EnrolledCount, 0, store.Capacity);
        Assert.Equal([2], store.Version);
        Assert.Equal(1, store.CommitCount);
    }

    [Fact]
    public async Task Capacity_below_enrollment_changes_nothing()
    {
        var store = new CapacityStoreFake("published", 30, 18, false);
        var service = new SectionGroupCapacityService(store);

        var result = await service.ChangeCapacityAsync(
            store.GroupId,
            [1],
            17,
            "admin-1",
            "invalid reduction");

        Assert.Equal(GroupCapacityOutcome.CapacityBelowEnrolled, result);
        Assert.Equal(30, store.Capacity);
        Assert.Equal(18, store.EnrolledCount);
        Assert.Equal([1], store.Version);
        Assert.Equal(0, store.CommitCount);
    }

    private sealed class CapacityStoreFake(
        string state,
        int capacity,
        int enrolledCount,
        bool paused) : ISectionGroupCapacityStore
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private SectionGroup _group = new(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000010"),
            "G01",
            capacity,
            enrolledCount,
            Enum.Parse<SectionGroupState>(state, ignoreCase: true),
            paused);

        public Guid GroupId => _group.Id;
        public string State => _group.State.ToString().ToLowerInvariant();
        public int Capacity => _group.Capacity;
        public int EnrolledCount => _group.EnrolledCount;
        public bool RegistrationPaused => _group.RegistrationPaused;
        public byte[] Version { get; private set; } = [1];
        public int CommitCount { get; private set; }

        public Task<GroupCapacitySnapshot?> ReadAsync(
            Guid groupId,
            CancellationToken cancellationToken) =>
            Task.FromResult<GroupCapacitySnapshot?>(
                groupId == GroupId
                    ? new(
                        GroupId,
                        State,
                        Capacity,
                        EnrolledCount,
                        RegistrationPaused,
                        Version)
                    : null);

        public async Task<GroupCapacityOutcome> ExecuteAsync(
            Guid groupId,
            byte[] expectedVersion,
            Func<GroupCapacityState, GroupCapacityOutcome> operation,
            CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (groupId != GroupId || !expectedVersion.SequenceEqual(Version))
                {
                    return GroupCapacityOutcome.StaleVersion;
                }

                var staged = new GroupCapacityState(
                    _group.Capacity,
                    _group.EnrolledCount);
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
                Version = [2];
                CommitCount++;
                return outcome;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
