using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

public sealed record ReconciliationServiceIdentity
{
    public ReconciliationServiceIdentity(
        string subject,
        bool isServiceIdentity,
        IReadOnlyCollection<string> permissions)
    {
        Subject = string.IsNullOrWhiteSpace(subject)
            ? throw new ArgumentException("A service subject is required.", nameof(subject))
            : subject.Trim();
        IsServiceIdentity = isServiceIdentity;
        Permissions = new ReadOnlyCollection<string>(
            (permissions ?? throw new ArgumentNullException(nameof(permissions)))
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.Ordinal)
            .ToArray());
    }

    public string Subject { get; }
    public bool IsServiceIdentity { get; }
    public IReadOnlyCollection<string> Permissions { get; }

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.Ordinal);
}

public sealed record CounterRepairRequest(
    Guid GroupId,
    byte[] ObservedGroupVersion,
    string EnrollmentEvidenceHash,
    string CorrelationId);

public sealed record CounterReconciliationResult(
    Guid GroupId,
    bool MismatchDetected,
    bool RegistrationPaused,
    int StoredCount,
    int ActiveEnrollmentCount,
    byte[] ObservedGroupVersion,
    string EnrollmentEvidenceHash);

public sealed record CounterRepairResult(
    Guid GroupId,
    int RepairedCount,
    bool RegistrationPaused,
    bool IsReplay);

/// <summary>
/// Scheduled/internal containment and repair service. No endpoint or Admin
/// command is defined here; repair requires the exact operations identity and
/// permission on every invocation. The RowVersion and EvidenceHash form the
/// repair scope; Audit facts are atomic, Verify precedes Resume, and the alert
/// contains only the affected group and counter facts.
/// </summary>
public sealed class EnrollmentCounterReconciler
{
    public const string CounterMismatchMetricName =
        "registration.counter_mismatches";
    public const string ReconcilePermission = "Registration.Reconcile";

    private const string MeterName = "StudentRegistration.Operations";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> CounterMismatchMetric =
        Meter.CreateCounter<long>(CounterMismatchMetricName);

    private readonly StudentRegistrationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public EnrollmentCounterReconciler(
        StudentRegistrationDbContext dbContext,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public Task<CounterReconciliationResult> ReconcileAsync(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        EnsureIdentifier(groupId, nameof(groupId));
        return ExecuteSerializableAsync(
            token => ReconcileCoreAsync(groupId, token),
            cancellationToken);
    }

    public Task<CounterRepairResult> RepairAsync(
        ReconciliationServiceIdentity identity,
        CounterRepairRequest request,
        CancellationToken cancellationToken = default)
    {
        Authorize(identity);
        Validate(request);
        return ExecuteSerializableAsync(
            token => RepairCoreAsync(identity, request, token),
            cancellationToken);
    }

    public static string ComputeEvidenceHash(IEnumerable<Guid> enrollmentIds)
    {
        ArgumentNullException.ThrowIfNull(enrollmentIds);
        var canonical = string.Join(
            "\n",
            enrollmentIds.OrderBy(id => id).Select(id => id.ToString("N")));
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private async Task<CounterReconciliationResult> ReconcileCoreAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var group = await LockGroupAsync(groupId, cancellationToken);
        var activeEnrollmentIds = await ActiveEnrollmentIdsAsync(
            groupId,
            cancellationToken);
        var activeCount = activeEnrollmentIds.Length;
        var evidenceHash = ComputeEvidenceHash(activeEnrollmentIds);
        var mismatch = group.EnrolledCount != activeCount;

        if (mismatch)
        {
            group.SetRegistrationPaused(true);
            _dbContext.AuditEvents.Add(CreateAuditEvent(
                "registration-reconciliation-worker",
                groupId,
                "RegistrationCounterMismatchAlerted",
                "COUNTER_MISMATCH",
                group.EnrolledCount,
                activeCount,
                $"reconciliation-alert:{Guid.NewGuid():N}"));
            await _dbContext.SaveChangesAsync(cancellationToken);
            CounterMismatchMetric.Add(
                1,
                new KeyValuePair<string, object?>("module", "registration"),
                new KeyValuePair<string, object?>("operation", "reconciliation"),
                new KeyValuePair<string, object?>("outcome", "conflict"),
                new KeyValuePair<string, object?>("code", "COUNTER_MISMATCH"));
        }

        return new CounterReconciliationResult(
            group.Id,
            mismatch,
            group.RegistrationPaused,
            group.EnrolledCount,
            activeCount,
            group.Version.ToArray(),
            evidenceHash);
    }

    private async Task<CounterRepairResult> RepairCoreAsync(
        ReconciliationServiceIdentity identity,
        CounterRepairRequest request,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = RepairIdempotencyKey(request);
        var group = await LockGroupAsync(request.GroupId, cancellationToken);
        var existingRepair = await RepairAuditExistsAsync(
            request.GroupId,
            idempotencyKey,
            cancellationToken);
        if (existingRepair)
        {
            return new CounterRepairResult(
                group.Id,
                group.EnrolledCount,
                group.RegistrationPaused,
                true);
        }

        if (!group.Version.AsSpan().SequenceEqual(request.ObservedGroupVersion))
        {
            throw new InvalidOperationException("RECONCILIATION_VERSION_CHANGED");
        }

        var activeEnrollmentIds = await ActiveEnrollmentIdsAsync(
            request.GroupId,
            cancellationToken);
        var actualEvidenceHash = ComputeEvidenceHash(activeEnrollmentIds);
        if (!string.Equals(
                actualEvidenceHash,
                request.EnrollmentEvidenceHash,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("RECONCILIATION_EVIDENCE_CHANGED");
        }

        var beforeCount = group.EnrolledCount;
        var repairedCount = activeEnrollmentIds.Length;
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [scheduling].[SectionGroups] WITH (UPDLOCK, HOLDLOCK)
            SET [EnrolledCount] = {repairedCount}, [RegistrationPaused] = 0
            WHERE [Id] = {request.GroupId}
              AND [Version] = {request.ObservedGroupVersion}
              AND [RegistrationPaused] = 1
            """,
            cancellationToken);
        if (affected != 1)
        {
            throw new InvalidOperationException("RECONCILIATION_VERSION_CHANGED");
        }

        await _dbContext.Entry(group).ReloadAsync(cancellationToken);
        if (group.EnrolledCount != repairedCount || group.RegistrationPaused)
        {
            throw new InvalidOperationException("RECONCILIATION_INVARIANT_FAILED");
        }

        _dbContext.AuditEvents.Add(CreateAuditEvent(
            identity.Subject,
            request.GroupId,
            "RegistrationCounterRepaired",
            $"AUTHORIZED_REPAIR:{request.CorrelationId}",
            beforeCount,
            repairedCount,
            idempotencyKey));
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CounterRepairResult(
            group.Id,
            group.EnrolledCount,
            group.RegistrationPaused,
            false);
    }

    private Task<bool> RepairAuditExistsAsync(
        Guid groupId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        _dbContext.AuditEvents
            .AsNoTracking()
            .AnyAsync(
                auditEvent =>
                    auditEvent.Action == "RegistrationCounterRepaired" &&
                    auditEvent.EntityType == "SectionGroup" &&
                    auditEvent.EntityId == groupId.ToString("N") &&
                    auditEvent.CorrelationId == idempotencyKey,
                cancellationToken);

    private async Task<SectionGroup> LockGroupAsync(
        Guid groupId,
        CancellationToken cancellationToken) =>
        await _dbContext.Set<SectionGroup>()
            .FromSqlInterpolated(
                $"SELECT * FROM [scheduling].[SectionGroups] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {groupId}")
            .SingleAsync(cancellationToken);

    private async Task<Guid[]> ActiveEnrollmentIdsAsync(
        Guid groupId,
        CancellationToken cancellationToken) =>
        await _dbContext.Set<Enrollment>()
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.GroupId == groupId &&
                enrollment.State == EnrollmentState.Active)
            .OrderBy(enrollment => enrollment.Id)
            .Select(enrollment => enrollment.Id)
            .ToArrayAsync(cancellationToken);

    private async Task<TResult> ExecuteSerializableAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _dbContext.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable,
                        cancellationToken);
                try
                {
                    var result = await operation(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }
            });
    }

    private AuditEvent CreateAuditEvent(
        string actor,
        Guid groupId,
        string action,
        string reason,
        int beforeCount,
        int afterCount,
        string correlationId)
    {
        var before = JsonSerializer.Serialize(new { enrolledCount = beforeCount });
        var after = JsonSerializer.Serialize(new { enrolledCount = afterCount });
        return new AuditEvent(
            Guid.NewGuid(),
            actor,
            $"group:{groupId:N}",
            action,
            "SectionGroup",
            groupId.ToString("N"),
            reason,
            before,
            after,
            correlationId,
            _timeProvider.GetUtcNow().UtcDateTime);
    }

    private static string RepairIdempotencyKey(CounterRepairRequest request)
    {
        var scope = string.Concat(
            request.GroupId.ToString("N"),
            ":",
            Convert.ToHexString(request.ObservedGroupVersion),
            ":",
            request.EnrollmentEvidenceHash);
        return string.Concat(
            "reconcile:",
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scope))));
    }

    private static void Authorize(ReconciliationServiceIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (!identity.IsServiceIdentity || !identity.HasPermission(ReconcilePermission))
        {
            throw new UnauthorizedAccessException("REGISTRATION_RECONCILE_FORBIDDEN");
        }
    }

    private static void Validate(CounterRepairRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureIdentifier(request.GroupId, nameof(request.GroupId));
        if (request.ObservedGroupVersion is not { Length: 8 })
        {
            throw new ArgumentException(
                "An eight-byte observed group rowversion is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.EnrollmentEvidenceHash) ||
            string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            throw new ArgumentException(
                "Evidence hash and correlation ID are required.",
                nameof(request));
        }
    }

    private static void EnsureIdentifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("The identifier is required.", parameterName);
        }
    }
}
