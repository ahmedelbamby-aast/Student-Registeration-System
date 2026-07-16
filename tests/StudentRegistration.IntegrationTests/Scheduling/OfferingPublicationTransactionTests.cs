using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Scheduling;

public sealed class OfferingPublicationTransactionTests
{
    [Fact]
    public async Task Publication_commits_offering_groups_and_audit_as_one_unit()
    {
        var validatorStore = new PublicationValidationStoreFake();
        var validator = new OfferingPublicationValidator();
        var transaction = new PublicationTransactionFake(validatorStore, groupCount: 2);
        var service = new OfferingPublicationService(validator, transaction);

        var result = await service.PublishAsync(Command(validatorStore.Snapshot));

        Assert.Equal(OfferingPublicationOutcome.Published, result.Outcome);
        Assert.Equal("published", transaction.OfferingState);
        Assert.All(transaction.GroupStates, state => Assert.Equal("published", state));
        Assert.Single(transaction.Audits);
        Assert.Equal(1, transaction.CommitCount);
        Assert.Equal(1, validatorStore.ValidationCallsInsideTransaction);
    }

    [Fact]
    public async Task Validation_or_store_failure_rolls_back_state_and_audit()
    {
        var invalidStore = new PublicationValidationStoreFake
        {
            Snapshot = PublicationValidationStoreFake.ValidSnapshot() with
            {
                BundleComplete = false
            }
        };
        var invalidTransaction = new PublicationTransactionFake(
            invalidStore,
            groupCount: 2);
        var invalidService = new OfferingPublicationService(
            new OfferingPublicationValidator(),
            invalidTransaction);

        var invalid = await invalidService.PublishAsync(Command(invalidStore.Snapshot));

        Assert.Equal(OfferingPublicationOutcome.ValidationFailed, invalid.Outcome);
        AssertDraftAndUnaudited(invalidTransaction);

        var validStore = new PublicationValidationStoreFake();
        var failedTransaction = new PublicationTransactionFake(validStore, groupCount: 2)
        {
            FailBeforeCommit = true
        };
        var failedService = new OfferingPublicationService(
            new OfferingPublicationValidator(),
            failedTransaction);

        var failed = await failedService.PublishAsync(Command(validStore.Snapshot));

        Assert.Equal(OfferingPublicationOutcome.StorageFailure, failed.Outcome);
        AssertDraftAndUnaudited(failedTransaction);
    }

    private static PublishOfferingCommand Command(OfferingPublicationSnapshot snapshot) =>
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
            },
            "preview-token",
            "publish-request",
            "publish offering");

    private static void AssertDraftAndUnaudited(PublicationTransactionFake transaction)
    {
        Assert.Equal("draft", transaction.OfferingState);
        Assert.All(transaction.GroupStates, state => Assert.Equal("draft", state));
        Assert.Empty(transaction.Audits);
        Assert.Equal(0, transaction.CommitCount);
    }

    private sealed class PublicationValidationStoreFake : IOfferingPublicationStore
    {
        public OfferingPublicationSnapshot Snapshot { get; set; } = ValidSnapshot();
        public bool InsideTransaction { get; set; }
        public int ValidationCallsInsideTransaction { get; private set; }

        public static OfferingPublicationSnapshot ValidSnapshot() =>
            new(
                Id(90),
                OfferingVersion: [1],
                Id(1),
                GroupVersion: [1],
                GroupCapacity: 30,
                Id(4),
                RoomCapacity: 30,
                RoomAvailable: true,
                RoomOverlap: false,
                CurrentRoomVersion: [1],
                Id(2),
                StaffAvailable: true,
                StaffOverlap: false,
                StaffVersion: [1],
                SlotValid: true,
                BundleComplete: true);

        public Task<PublicationDependencies> GetDependenciesAsync(
            Guid offeringId,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new PublicationDependencies(
                    Snapshot.OfferingId,
                    [Snapshot.GroupId],
                    [Snapshot.RoomId],
                    [Snapshot.StaffTermAvailabilityId]));

        public Task<OfferingPublicationSnapshot> LockAndLoadAsync(
            PublicationLockRequest request,
            CancellationToken cancellationToken)
        {
            if (InsideTransaction)
            {
                ValidationCallsInsideTransaction++;
            }
            return Task.FromResult(Snapshot);
        }

    }

    private sealed class PublicationTransactionFake(
        PublicationValidationStoreFake scopedStore,
        int groupCount)
        : IOfferingPublicationTransaction
    {
        public string OfferingState { get; private set; } = "draft";
        public IReadOnlyList<string> GroupStates { get; private set; } =
            Enumerable.Repeat("draft", groupCount).ToArray();
        public List<OfferingPublicationAudit> Audits { get; } = [];
        public int CommitCount { get; private set; }
        public bool FailBeforeCommit { get; init; }
        public CourseOffering Offering { get; } = new(
            Id(90),
            Id(91),
            Id(92),
            CourseOfferingState.Draft);
        public IReadOnlyList<SectionGroup> Groups { get; } =
            Enumerable.Range(1, groupCount)
                .Select(index => new SectionGroup(
                    Id(index),
                    Id(90),
                    $"G{index:00}",
                    30,
                    0,
                    SectionGroupState.Draft,
                    false))
                .ToArray();

        public async Task<OfferingPublicationResult> ExecuteAsync(
            PublishOfferingCommand command,
            Func<
                IOfferingPublicationStore,
                OfferingPublicationState,
                CancellationToken,
                Task<OfferingPublicationResult>> operation,
            CancellationToken cancellationToken)
        {
            var staged = new OfferingPublicationState(
                OfferingState,
                GroupStates.ToArray(),
                []);
            scopedStore.InsideTransaction = true;
            OfferingPublicationResult result;
            try
            {
                result = await operation(scopedStore, staged, cancellationToken);
            }
            finally
            {
                scopedStore.InsideTransaction = false;
            }
            if (result.Outcome != OfferingPublicationOutcome.Published)
            {
                return result;
            }
            if (FailBeforeCommit)
            {
                return new OfferingPublicationResult(
                    OfferingPublicationOutcome.StorageFailure,
                    "SCHEDULING_UNAVAILABLE");
            }

            OfferingState = staged.OfferingState;
            GroupStates = staged.GroupStates.ToArray();
            Audits.AddRange(staged.Audits);
            CommitCount++;
            return result;
        }
    }

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
