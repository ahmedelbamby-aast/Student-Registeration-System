using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public async Task Capacity_change_racing_enrollment_preserves_the_capacity_floor()
    {
        var store = new CapacityStoreFake(capacity: 2, enrolledCount: 1);
        var service = new SectionGroupCapacityService(store);
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var outcomesTask = Task.WhenAll(
            AfterStart(start.Task, () => service.ChangeCapacityAsync(
                store.GroupId,
                [1],
                1,
                "admin-1",
                "race capacity change")),
            AfterStart(start.Task, () => service.AllocateSeatAsync(
                store.GroupId,
                [1])));

        start.SetResult();
        var outcomes = await outcomesTask;

        Assert.Single(outcomes, outcome => outcome == GroupCapacityOutcome.Applied);
        Assert.Single(
            outcomes,
            outcome => outcome is GroupCapacityOutcome.StaleVersion or
                GroupCapacityOutcome.GroupFull);
        Assert.True(store.Capacity >= store.EnrolledCount);
        Assert.Equal([2], store.Version);
        Assert.Equal(1, store.CommitCount);
    }

    private static async Task<GroupCapacityOutcome> AfterStart(
        Task start,
        Func<Task<GroupCapacityOutcome>> action)
    {
        await start;
        return await action();
    }

    private sealed class CapacityStoreFake(int capacity, int enrolledCount)
        : ISectionGroupCapacityStore
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private SectionGroup _group = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "G01",
            capacity,
            enrolledCount,
            SectionGroupState.Published,
            false);

        public Guid GroupId => _group.Id;
        public int Capacity => _group.Capacity;
        public int EnrolledCount => _group.EnrolledCount;
        public byte[] Version { get; private set; } = [1];
        public int CommitCount { get; private set; }

        public Task<GroupCapacitySnapshot?> ReadAsync(
            Guid groupId,
            CancellationToken cancellationToken) =>
            Task.FromResult<GroupCapacitySnapshot?>(
                groupId == GroupId
                    ? new(
                        GroupId,
                        "published",
                        Capacity,
                        EnrolledCount,
                        false,
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
                Version = [2];
                CommitCount++;
                return outcome;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
