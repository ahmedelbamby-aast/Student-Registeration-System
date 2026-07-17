using System.Reflection;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.StaffWorkspace;

public sealed class StaffAvailabilityConcurrencyTests
{
    private static readonly DateTime Deadline =
        new(2026, 8, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Complete_range_replacement_uses_parent_version_and_canonical_children()
    {
        var aggregate = Availability();
        var replacement = Command(
            [new(Guid.NewGuid(), DayOfWeek.Tuesday, new(9, 0), new(12, 0), AvailabilityKind.Available)]);

        var decision = StaffAvailabilityTransactionRules.Apply(
            aggregate,
            replacement,
            Deadline.AddMinutes(-1),
            []);

        Assert.Equal(StaffAvailabilityPortOutcome.Success, decision.Outcome);
        var range = Assert.Single(aggregate.Ranges);
        Assert.Equal(DayOfWeek.Tuesday, range.DayOfWeek);
        Assert.Equal(AvailabilityKind.Available, range.Kind);
        Assert.Equal(aggregate.Id, range.StaffTermAvailabilityId);
    }

    [Fact]
    public void Stale_deadline_and_overlapping_sets_leave_the_aggregate_unchanged()
    {
        var stale = Availability();
        var staleCommand = Command(
            ValidRanges(),
            expectedVersion: [9]);
        var staleDecision = StaffAvailabilityTransactionRules.Apply(
            stale,
            staleCommand,
            Deadline.AddMinutes(-1),
            []);
        Assert.Equal(StaffAvailabilityPortOutcome.StaleVersion, staleDecision.Outcome);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(stale.Ranges).DayOfWeek);

        var expired = Availability();
        var expiredDecision = StaffAvailabilityTransactionRules.Apply(
            expired,
            Command(ValidRanges()),
            Deadline.AddTicks(1),
            []);
        Assert.Equal(StaffAvailabilityPortOutcome.DeadlinePassed, expiredDecision.Outcome);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(expired.Ranges).DayOfWeek);

        var overlap = Availability();
        var overlapDecision = StaffAvailabilityTransactionRules.Apply(
            overlap,
            Command(
            [
                new(Guid.NewGuid(), DayOfWeek.Tuesday, new(9, 0), new(12, 0), AvailabilityKind.Available),
                new(Guid.NewGuid(), DayOfWeek.Tuesday, new(11, 0), new(13, 0), AvailabilityKind.Unavailable),
            ]),
            Deadline.AddMinutes(-1),
            []);
        Assert.Equal(StaffAvailabilityPortOutcome.ValidationFailed, overlapDecision.Outcome);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(overlap.Ranges).DayOfWeek);
    }

    [Fact]
    public async Task Concurrent_same_version_replacements_have_exactly_one_winner()
    {
        var port = new AtomicPort(Availability());
        var first = Command(ValidRanges());
        var second = Command(
            [new(Guid.NewGuid(), DayOfWeek.Wednesday, new(8, 0), new(10, 0), AvailabilityKind.Available)]);

        var results = await Task.WhenAll(
            port.ReplaceOwnAsync(first),
            port.ReplaceOwnAsync(second));

        Assert.Single(results, result => result.Outcome == StaffAvailabilityPortOutcome.Success);
        Assert.Single(results, result => result.Outcome == StaffAvailabilityPortOutcome.StaleVersion);
        Assert.All(results, result => Assert.NotNull(result.Availability));
    }

    private static StaffTermAvailability Availability()
    {
        var id = Guid.NewGuid();
        var aggregate = new StaffTermAvailability(
            id,
            StaffId,
            TermId,
            Deadline,
            [new StaffAvailability(Guid.NewGuid(), id, DayOfWeek.Monday, new(8, 0), new(12, 0), AvailabilityKind.Available)]);
        SetVersion(aggregate, [1]);
        return aggregate;
    }

    private static ReplaceOwnStaffAvailability Command(
        IReadOnlyList<StaffAvailabilityRangeInput> ranges,
        byte[]? expectedVersion = null) =>
        new(StaffId, TermId, expectedVersion ?? [1], ranges, "staff declaration", "correlation-1");

    private static IReadOnlyList<StaffAvailabilityRangeInput> ValidRanges() =>
        [new(Guid.NewGuid(), DayOfWeek.Tuesday, new(9, 0), new(12, 0), AvailabilityKind.Available)];

    private static void SetVersion(StaffTermAvailability aggregate, byte[] value) =>
        typeof(StaffTermAvailability).GetProperty(nameof(StaffTermAvailability.Version))!
            .SetValue(aggregate, value);

    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid TermId = Guid.NewGuid();

    private sealed class AtomicPort(StaffTermAvailability current) : IStaffAvailabilityPort
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private byte[] _version = current.Version.ToArray();

        public Task<StaffAvailabilityPortResult> GetOwnAsync(Guid staffId, Guid termId, CancellationToken cancellationToken = default) =>
            Task.FromResult(StaffAvailabilityPortResult.Success(Snapshot(), [], Deadline.AddMinutes(-1)));

        public async Task<StaffAvailabilityPortResult> ReplaceOwnAsync(ReplaceOwnStaffAvailability command, CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                SetVersion(current, _version);
                var decision = StaffAvailabilityTransactionRules.Apply(
                    current,
                    command,
                    Deadline.AddMinutes(-1),
                    []);
                if (decision.Outcome == StaffAvailabilityPortOutcome.StaleVersion)
                {
                    return StaffAvailabilityPortResult.Stale(Snapshot(), Deadline.AddMinutes(-1));
                }

                _version = [(byte)(_version[0] + 1)];
                SetVersion(current, _version);
                return StaffAvailabilityPortResult.Success(Snapshot(), [], Deadline.AddMinutes(-1));
            }
            finally
            {
                _gate.Release();
            }
        }

        private StaffTermAvailabilitySnapshot Snapshot() =>
            new(
                current.Id,
                current.StaffId,
                current.TermId,
                current.DeadlineUtc,
                current.Version.ToArray(),
                current.Ranges.Select(range => new StaffAvailabilityRangeSnapshot(
                    range.Id,
                    range.DayOfWeek,
                    range.StartLocal,
                    range.EndLocal,
                    range.Kind)).ToArray());
    }
}
