using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

public enum SeatAllocationStatus
{
    Allocated = 1,
    Rejected = 2,
}

public sealed record SeatAllocationResult(
    Guid GroupId,
    SeatAllocationStatus Status,
    string? ReasonCode)
{
    public bool IsAllocated => Status is SeatAllocationStatus.Allocated;
}

public sealed record SeatAllocationBatchResult(
    bool IsAccepted,
    string? ReasonCode,
    IReadOnlyList<Guid> AllocatedGroupIds);

/// <summary>
/// Performs only the SQL seat-counter part of registration. The caller owns
/// the registration transaction, idempotency claim, Enrollment rows, and final
/// result so all of them share one commit boundary.
/// </summary>
public sealed class SqlSeatAllocator
{
    public const string LockWaitMetricName = "sql.lock_wait.duration.ms";
    public const string DeadlockMetricName = "sql.deadlocks";
    public const string CapacityConflictMetricName = "registration.capacity_conflicts";

    private const string AllocationSavepoint = "registration_allocation";
    private const string HoldSavepoint = "registration_hold";
    private const string MeterName = "StudentRegistration.Operations";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Histogram<double> LockWaitMetric =
        Meter.CreateHistogram<double>(LockWaitMetricName, "ms");
    private static readonly Counter<long> DeadlockMetric =
        Meter.CreateCounter<long>(DeadlockMetricName);
    private static readonly Counter<long> CapacityConflictMetric =
        Meter.CreateCounter<long>(CapacityConflictMetricName);
    private readonly StudentRegistrationDbContext _dbContext;

    public SqlSeatAllocator(StudentRegistrationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<SeatAllocationResult> AllocateAsync(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(groupId, nameof(groupId));
        RequireCallerTransaction();
        cancellationToken.ThrowIfCancellationRequested();

        var startedAt = Stopwatch.GetTimestamp();
        int affected;
        try
        {
            affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE [scheduling].[SectionGroups] WITH (UPDLOCK, ROWLOCK)
                SET [EnrolledCount] = [EnrolledCount] + 1
                WHERE [Id] = {groupId}
                  AND [State] = N'published'
                  AND [RegistrationPaused] = 0
                  AND [EnrolledCount] + [HeldSeatCount] < [Capacity]
                """,
                cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 1205)
        {
            DeadlockMetric.Add(1, SafeTags("failure", "CAPACITY_CONFLICT"));
            throw;
        }
        finally
        {
            LockWaitMetric.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                SafeTags("success", null));
        }

        if (affected != 1)
        {
            CapacityConflictMetric.Add(1, SafeTags("conflict", "GROUP_FULL"));
        }

        return affected == 1
            ? new SeatAllocationResult(groupId, SeatAllocationStatus.Allocated, null)
            : new SeatAllocationResult(groupId, SeatAllocationStatus.Rejected, "GROUP_FULL");
    }

    public async Task<bool> ReduceCapacityAsync(
        Guid groupId,
        int newCapacity,
        CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(groupId, nameof(groupId));
        if (newCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newCapacity));
        }

        RequireCallerTransaction();
        cancellationToken.ThrowIfCancellationRequested();
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [scheduling].[SectionGroups] WITH (UPDLOCK, HOLDLOCK)
            SET [Capacity] = {newCapacity}
            WHERE [Id] = {groupId}
              AND [EnrolledCount] + [HeldSeatCount] <= {newCapacity}
            """,
            cancellationToken);
        return affected == 1;
    }

    public async Task<SeatAllocationBatchResult> AllocateWithSavepointAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        RequireCallerTransaction();
        var orderedGroupIds = ValidateAndOrder(groupIds);
        cancellationToken.ThrowIfCancellationRequested();

        var transaction = _dbContext.Database.CurrentTransaction!;
        await transaction.CreateSavepointAsync(
            AllocationSavepoint,
            cancellationToken);

        var allocated = new List<Guid>(orderedGroupIds.Length);
        foreach (var groupId in orderedGroupIds)
        {
            var result = await AllocateAsync(groupId, cancellationToken);
            if (!result.IsAllocated)
            {
                await transaction.RollbackToSavepointAsync(
                    AllocationSavepoint,
                    cancellationToken);
                return new SeatAllocationBatchResult(
                    false,
                    result.ReasonCode,
                    Array.Empty<Guid>());
            }

            allocated.Add(groupId);
        }

        return new SeatAllocationBatchResult(true, null, allocated.AsReadOnly());
    }

    public Task<SeatAllocationBatchResult> AllocateAllOrRejectAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken = default) =>
        AllocateWithSavepointAsync(groupIds, cancellationToken);

    public async Task<SeatAllocationBatchResult> HoldAllOrRejectAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        RequireCallerTransaction();
        var orderedGroupIds = ValidateAndOrder(groupIds);
        cancellationToken.ThrowIfCancellationRequested();
        var transaction = _dbContext.Database.CurrentTransaction!;
        await transaction.CreateSavepointAsync(HoldSavepoint, cancellationToken);

        var held = new List<Guid>(orderedGroupIds.Length);
        foreach (var groupId in orderedGroupIds)
        {
            var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE [scheduling].[SectionGroups] WITH (UPDLOCK, ROWLOCK)
                SET [HeldSeatCount] = [HeldSeatCount] + 1
                WHERE [Id] = {groupId}
                  AND [State] = N'published'
                  AND [RegistrationPaused] = 0
                  AND [EnrolledCount] + [HeldSeatCount] < [Capacity]
                """,
                cancellationToken);
            if (affected != 1)
            {
                await transaction.RollbackToSavepointAsync(
                    HoldSavepoint,
                    cancellationToken);
                CapacityConflictMetric.Add(1, SafeTags("conflict", "GROUP_FULL"));
                return new SeatAllocationBatchResult(
                    false,
                    "GROUP_FULL",
                    Array.Empty<Guid>());
            }

            held.Add(groupId);
        }

        return new SeatAllocationBatchResult(true, null, held.AsReadOnly());
    }

    public async Task ConvertHoldsToEnrollmentsAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        RequireCallerTransaction();
        foreach (var groupId in ValidateAndOrder(groupIds))
        {
            var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE [scheduling].[SectionGroups] WITH (UPDLOCK, ROWLOCK)
                SET [HeldSeatCount] = [HeldSeatCount] - 1,
                    [EnrolledCount] = [EnrolledCount] + 1
                WHERE [Id] = {groupId}
                  AND [HeldSeatCount] > 0
                  AND [EnrolledCount] + [HeldSeatCount] <= [Capacity]
                """,
                cancellationToken);
            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "HELD_SEAT_CHANGED: Every selected group must retain its held seat until conversion.");
            }
        }
    }

    public async Task ReleaseHoldsAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        RequireCallerTransaction();
        foreach (var groupId in ValidateAndOrder(groupIds))
        {
            var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE [scheduling].[SectionGroups] WITH (UPDLOCK, ROWLOCK)
                SET [HeldSeatCount] = [HeldSeatCount] - 1
                WHERE [Id] = {groupId}
                  AND [HeldSeatCount] > 0
                """,
                cancellationToken);
            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "HELD_SEAT_CHANGED: Every active hold must be released exactly once.");
            }
        }
    }

    public async Task RollbackAllocationAsync(
        CancellationToken cancellationToken = default)
    {
        RequireCallerTransaction();
        await _dbContext.Database.CurrentTransaction!.RollbackToSavepointAsync(
            AllocationSavepoint,
            cancellationToken);
    }

    private void RequireCallerTransaction()
    {
        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "REGISTRATION_TRANSACTION_REQUIRED: Seat allocation must run inside the caller's registration SQL transaction.");
        }
    }

    private static Guid[] ValidateAndOrder(IReadOnlyCollection<Guid> groupIds)
    {
        if (groupIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one group is required for allocation.",
                nameof(groupIds));
        }

        if (groupIds.Any(groupId => groupId == Guid.Empty))
        {
            throw new ArgumentException(
                "Group identifiers must be non-empty.",
                nameof(groupIds));
        }

        var ordered = groupIds.OrderBy(groupId => groupId).ToArray();
        if (ordered.Distinct().Count() != ordered.Length)
        {
            throw new ArgumentException(
                "A group may be allocated only once per submission.",
                nameof(groupIds));
        }

        return ordered;
    }

    private static void EnsureIdentifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("The identifier is required.", parameterName);
        }
    }

    private static KeyValuePair<string, object?>[] SafeTags(
        string outcome,
        string? code) => code is null
        ?
        [
            new("module", "registration"),
            new("operation", "seat-allocation"),
            new("outcome", outcome)
        ]
        :
        [
            new("module", "registration"),
            new("operation", "seat-allocation"),
            new("outcome", outcome),
            new("code", code)
        ];
}
