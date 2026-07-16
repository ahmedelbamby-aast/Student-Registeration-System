using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.IntegrationTests.Specs.Spec010.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public async Task Deadlock_retry_repeats_stable_locks_and_never_partially_publishes()
    {
        var validationStore = new ValidationStoreFake();
        var transaction = new DeadlockOnceTransactionFake(
            validationStore,
            groupCount: 2);
        var service = new OfferingPublicationService(
            new OfferingPublicationValidator(),
            transaction);

        var result = await service.PublishAsync(Command(validationStore));

        Assert.Equal(OfferingPublicationOutcome.Published, result.Outcome);
        Assert.Equal(2, transaction.Attempts);
        Assert.Equal(2, validationStore.LockRequests.Count);
        Assert.All(
            validationStore.LockRequests,
            request => Assert.Equal(
                [
                    validationStore.Dependencies.OfferingId,
                    Id(1),
                    Id(3),
                    Id(2),
                    Id(4),
                    Id(5),
                    Id(6)
                ],
                request.OrderedResourceIds));
        Assert.False(transaction.PartialStateAfterDeadlock);
        Assert.Equal("published", transaction.OfferingState);
        Assert.All(
            transaction.GroupStates,
            state => Assert.Equal("published", state));
        Assert.Single(transaction.Audits);
        Assert.Equal(1, transaction.CommitCount);
    }

    private static PublishOfferingCommand Command(ValidationStoreFake store) =>
        new(
            store.Snapshot.OfferingId,
            store.Snapshot.OfferingVersion,
            new Dictionary<Guid, byte[]>
            {
                [Id(1)] = [1],
                [Id(3)] = [1]
            },
            new Dictionary<Guid, byte[]>
            {
                [Id(2)] = [1],
                [Id(4)] = [1]
            },
            new Dictionary<Guid, byte[]>
            {
                [Id(5)] = [1],
                [Id(6)] = [1]
            },
            "preview-token",
            "publish-request",
            "publish offering");

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private sealed class ValidationStoreFake : IOfferingPublicationStore
    {
        public PublicationDependencies Dependencies { get; } =
            new(Id(90), [Id(3), Id(1)], [Id(4), Id(2)], [Id(6), Id(5)]);
        public OfferingPublicationSnapshot Snapshot { get; } =
            new(
                Id(90),
                OfferingVersion: [1],
                Id(1),
                GroupVersion: [1],
                GroupCapacity: 30,
                Id(2),
                RoomCapacity: 30,
                RoomAvailable: true,
                RoomOverlap: false,
                CurrentRoomVersion: [1],
                Id(5),
                StaffAvailable: true,
                StaffOverlap: false,
                StaffVersion: [1],
                SlotValid: true,
                BundleComplete: true);
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

    private sealed class DeadlockOnceTransactionFake(
        ValidationStoreFake scopedStore,
        int groupCount)
        : IOfferingPublicationTransaction
    {
        public string OfferingState { get; private set; } = "draft";
        public IReadOnlyList<string> GroupStates { get; private set; } =
            Enumerable.Repeat("draft", groupCount).ToArray();
        public List<OfferingPublicationAudit> Audits { get; } = [];
        public int Attempts { get; private set; }
        public int CommitCount { get; private set; }
        public bool PartialStateAfterDeadlock { get; private set; }

        public async Task<OfferingPublicationResult> ExecuteAsync(
            PublishOfferingCommand command,
            Func<
                IOfferingPublicationStore,
                OfferingPublicationState,
                CancellationToken,
                Task<OfferingPublicationResult>> operation,
            CancellationToken cancellationToken)
        {
            Attempts++;
            var staged = new OfferingPublicationState(
                OfferingState,
                GroupStates.ToArray(),
                []);
            var result = await operation(
                scopedStore,
                staged,
                cancellationToken);
            if (Attempts == 1)
            {
                PartialStateAfterDeadlock =
                    OfferingState == "published"
                    || GroupStates.Any(state => state == "published")
                    || Audits.Count > 0
                    || CommitCount > 0;
                throw new SchedulingDeadlockException();
            }

            OfferingState = staged.OfferingState;
            GroupStates = staged.GroupStates.ToArray();
            Audits.AddRange(staged.Audits);
            CommitCount++;
            return result;
        }
    }
}
