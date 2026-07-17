using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.StaffAdministration.Application.Ports;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.StaffAdministration.Application;

public sealed record AdminExportFilter(
    DateTime? OccurredFromUtc = null,
    DateTime? OccurredToUtc = null,
    Guid? ActorId = null,
    string? Action = null,
    string? SourceStream = null);

public sealed record AdminExportRequest(
    Guid OwnerId,
    Guid ClientRequestId,
    string ScopeHash,
    AdminExportFilter Filter,
    string ActorReference,
    string CorrelationId,
    DateTime RequestedAtUtc);

public sealed record AdminExportAccessContext(
    Guid ActorUserId,
    string ActorReference,
    string ScopeHash,
    bool CanReadAll,
    string CorrelationId);

public sealed record AdminAuditExportRow(
    Guid Id,
    DateTime OccurredAtUtc,
    Guid? ActorId,
    string ActorReference,
    string Action,
    string EntityType,
    string EntityId,
    string Reason,
    string RedactedBeforeSummary,
    string RedactedAfterSummary,
    string CorrelationId,
    string SourceStream,
    string ScopeHash);

public enum AdminExportOutcome
{
    Created = 1,
    Replay = 2,
    Succeeded = 3,
    Invalid = 4,
    IdempotencyKeyReused = 5,
    NotFound = 6,
    NotReady = 7,
    Expired = 8,
    LeaseUnavailable = 9,
    StorageUnavailable = 10,
}

public sealed record AdminExportResult(
    AdminExportOutcome Outcome,
    ExportJob? Job = null,
    ReadOnlyMemory<byte>? Content = null,
    string? ErrorCode = null);

public sealed class AuditExportService
{
    public const int MaximumPageSize = 100;
    public const int MaximumExportRows = 10_000;
    public const int MaximumArtifactBytes = 10 * 1024 * 1024;
    public static readonly TimeSpan MaximumFilterRange = TimeSpan.FromDays(366);
    public static readonly TimeSpan DefaultArtifactRetention = TimeSpan.FromDays(1);

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);
    private static readonly string[] ProhibitedSummaryFields =
    [
        "password",
        "secret",
        "token",
        "connectionstring",
        "metadatajson"
    ];

    private readonly IAdminExportStore _store;
    private readonly IAdminExportArtifactStore _artifactStore;
    private readonly TimeProvider _timeProvider;

    public AuditExportService(
        IAdminExportStore store,
        IAdminExportArtifactStore artifactStore,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _artifactStore = artifactStore
            ?? throw new ArgumentNullException(nameof(artifactStore));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public static bool IsValidPage(int page, int pageSize) =>
        page >= 1 && pageSize is >= 1 and <= MaximumPageSize;

    public async Task<AdminExportResult> RequestAsync(
        AdminExportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsValidRequest(request))
        {
            return Invalid("EXPORT_REQUEST_INVALID");
        }

        var requestHash = ComputeRequestHash(request.Filter);
        var job = new ExportJob(
            Guid.NewGuid(),
            request.OwnerId,
            request.ClientRequestId,
            request.ScopeHash,
            requestHash,
            request.RequestedAtUtc);
        if (job.State is not ExportJobState.Pending)
        {
            throw new InvalidOperationException("A new export job must start Pending.");
        }
        var audit = new AuditEventDraft(
            request.ActorReference,
            $"admin-export:{job.Id:N}",
            "AdminExportRequested",
            nameof(ExportJob),
            job.Id.ToString("N"),
            "A scoped administrative audit export was requested.",
            null,
            JsonSerializer.Serialize(
                new { state = "pending", requestHash },
                JsonOptions),
            request.CorrelationId,
            request.RequestedAtUtc);

        try
        {
            var result = await _store.CreateOrReplayAsync(
                    new AdminExportCreateCommand(job, audit),
                    cancellationToken)
                .ConfigureAwait(false);
            return result.Outcome switch
            {
                AdminExportCreateOutcome.Created =>
                    new(AdminExportOutcome.Created, result.Job),
                AdminExportCreateOutcome.Replay =>
                    new(AdminExportOutcome.Replay, result.Job),
                // Conflict: no duplicate job, audit event, or artifact is created.
                _ => new(
                    AdminExportOutcome.IdempotencyKeyReused,
                    result.Job,
                    ErrorCode: "IDEMPOTENCY_KEY_REUSED")
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            return new(AdminExportOutcome.StorageUnavailable, ErrorCode: "EXPORT_UNAVAILABLE");
        }
    }

    public async Task<AdminExportResult> GetStatusAsync(
        Guid jobId,
        AdminExportAccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidAccess(jobId, access))
        {
            return new(AdminExportOutcome.NotFound, ErrorCode: "RESOURCE_NOT_FOUND");
        }

        var job = await _store.ReadAuthorizedAsync(
                jobId,
                access.ActorUserId,
                access.ScopeHash,
                access.CanReadAll,
                cancellationToken)
            .ConfigureAwait(false);
        if (job is null)
        {
            return new(AdminExportOutcome.NotFound, ErrorCode: "RESOURCE_NOT_FOUND");
        }

        if (job.State is ExportJobState.Failed)
        {
            return new(AdminExportOutcome.Succeeded, job);
        }

        return await ExpireIfRequiredAsync(job, cancellationToken).ConfigureAwait(false)
            ? new(AdminExportOutcome.Expired, job, ErrorCode: "EXPORT_EXPIRED")
            : new(AdminExportOutcome.Succeeded, job);
    }

    public async Task<AdminExportResult> DownloadAsync(
        Guid jobId,
        AdminExportAccessContext access,
        CancellationToken cancellationToken = default)
    {
        var status = await GetStatusAsync(jobId, access, cancellationToken)
            .ConfigureAwait(false);
        if (status.Outcome is not AdminExportOutcome.Succeeded || status.Job is null)
        {
            return status;
        }

        var job = status.Job;
        if (job.State is not ExportJobState.Complete || job.ArtifactId is null)
        {
            return new(AdminExportOutcome.NotReady, job, ErrorCode: "EXPORT_NOT_READY");
        }

        var content = await _artifactStore.ReadAsync(job.ArtifactId.Value, cancellationToken)
            .ConfigureAwait(false);
        if (content is null)
        {
            return new(AdminExportOutcome.StorageUnavailable, job, ErrorCode: "EXPORT_UNAVAILABLE");
        }

        var observedAtUtc = UtcNow();
        var audited = await _store.RecordDownloadAsync(
                job.Id,
                access.ActorUserId,
                access.ScopeHash,
                access.CanReadAll,
                new AuditEventDraft(
                    access.ActorReference,
                    $"admin-export:{job.Id:N}",
                    "AdminExportDownloaded",
                    nameof(ExportJob),
                    job.Id.ToString("N"),
                    "An authorized administrative export was downloaded.",
                    null,
                    JsonSerializer.Serialize(new { state = "downloaded" }, JsonOptions),
                    access.CorrelationId,
                    observedAtUtc),
                observedAtUtc,
                cancellationToken)
            .ConfigureAwait(false);
        return audited
            ? new(AdminExportOutcome.Succeeded, job, content)
            : new(AdminExportOutcome.NotFound, ErrorCode: "RESOURCE_NOT_FOUND");
    }

    public Task<ExportJob?> TryClaimAsync(
        Guid jobId,
        string leaseOwnerId,
        CancellationToken cancellationToken = default) =>
        _store.TryClaimAsync(jobId, leaseOwnerId, UtcNow(), cancellationToken);

    public Task<bool> RenewLeaseAsync(
        Guid jobId,
        string leaseOwnerId,
        CancellationToken cancellationToken = default) =>
        _store.RenewLeaseAsync(jobId, leaseOwnerId, UtcNow(), cancellationToken);

    public async Task<AdminExportResult> PublishAsync(
        ExportJob claimedJob,
        string leaseOwnerId,
        AdminExportFilter filter,
        IReadOnlyCollection<AdminAuditExportRow> rows,
        TimeSpan? retention = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(claimedJob);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(rows);
        if (claimedJob.State is not ExportJobState.Running
            || !string.Equals(claimedJob.LeaseOwnerId, leaseOwnerId, StringComparison.Ordinal)
            || claimedJob.LeaseExpiresAtUtc is null
            || claimedJob.LeaseExpiresAtUtc <= UtcNow()
            || claimedJob.AttemptCount is < 1 or > 3
            || ExportJob.LeaseDuration != TimeSpan.FromSeconds(60)
            || !IsValidFilter(filter)
            || !string.Equals(
                claimedJob.RequestHash,
                ComputeRequestHash(filter),
                StringComparison.Ordinal)
            || rows.Count > MaximumExportRows
            || rows.Any(row =>
                !IsSafeRow(row)
                || !string.Equals(row.ScopeHash, claimedJob.ScopeHash, StringComparison.Ordinal)
                || !MatchesFilter(row, filter)))
        {
            return Invalid("EXPORT_PAYLOAD_INVALID");
        }

        var content = BuildCsv(rows);
        if (content.Length > MaximumArtifactBytes)
        {
            return Invalid("EXPORT_PAYLOAD_TOO_LARGE");
        }

        var completedAtUtc = UtcNow();
        var effectiveRetention = retention ?? DefaultArtifactRetention;
        if (effectiveRetention <= TimeSpan.Zero || effectiveRetention > TimeSpan.FromDays(7))
        {
            return Invalid("EXPORT_RETENTION_INVALID");
        }

        Guid? artifactId = null;
        try
        {
            artifactId = await _artifactStore.WriteAsync(content, cancellationToken)
                .ConfigureAwait(false);
            var published = await _store.PublishAsync(
                    claimedJob.Id,
                    leaseOwnerId,
                    artifactId.Value,
                    completedAtUtc,
                    completedAtUtc.Add(effectiveRetention),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!published)
            {
                await _artifactStore.DeleteAsync(artifactId.Value, CancellationToken.None)
                    .ConfigureAwait(false);
                return new(AdminExportOutcome.LeaseUnavailable, ErrorCode: "EXPORT_LEASE_LOST");
            }

            return new(AdminExportOutcome.Succeeded);
        }
        catch (OperationCanceledException)
        {
            await DeleteUnpublishedArtifactAsync(artifactId).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            await DeleteUnpublishedArtifactAsync(artifactId).ConfigureAwait(false);
            return new(AdminExportOutcome.StorageUnavailable, ErrorCode: "EXPORT_UNAVAILABLE");
        }
    }

    public Task<bool> RecordFailureAsync(
        Guid jobId,
        string leaseOwnerId,
        bool retryable,
        string failureCode,
        CancellationToken cancellationToken = default) =>
        _store.RecordFailureAsync(
            jobId,
            leaseOwnerId,
            UtcNow(),
            retryable,
            failureCode,
            cancellationToken);

    public static string ComputeRequestHash(AdminExportFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var canonical = JsonSerializer.Serialize(
            new
            {
                occurredFromUtc = filter.OccurredFromUtc?.ToUniversalTime().ToString("O"),
                occurredToUtc = filter.OccurredToUtc?.ToUniversalTime().ToString("O"),
                actorId = filter.ActorId?.ToString("D"),
                action = filter.Action?.Trim(),
                sourceStream = filter.SourceStream?.Trim().ToLowerInvariant()
            },
            JsonOptions);
        return $"SHA256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))}";
    }

    private async Task<bool> ExpireIfRequiredAsync(
        ExportJob job,
        CancellationToken cancellationToken)
    {
        var observedAtUtc = UtcNow();
        if (job.State is ExportJobState.Expired)
        {
            if (job.ArtifactId is not null)
            {
                await _artifactStore.DeleteAsync(job.ArtifactId.Value, cancellationToken)
                    .ConfigureAwait(false);
            }

            return true;
        }

        if (job.State is not ExportJobState.Complete
            || job.ArtifactId is null
            || job.ExpiresAtUtc is null
            || observedAtUtc < job.ExpiresAtUtc.Value)
        {
            return false;
        }

        await _artifactStore.DeleteAsync(job.ArtifactId.Value, cancellationToken)
            .ConfigureAwait(false);
        await _store.ExpireAsync(
                job.Id,
                job.ArtifactId.Value,
                observedAtUtc,
                cancellationToken)
            .ConfigureAwait(false);
        return true;
    }

    private static bool IsValidRequest(AdminExportRequest request) =>
        request.OwnerId != Guid.Empty
        && request.ClientRequestId != Guid.Empty
        && IsBounded(request.ScopeHash, 200)
        && IsBounded(request.ActorReference, 200)
        && IsBounded(request.CorrelationId, 200)
        && request.RequestedAtUtc.Kind is DateTimeKind.Utc
        && IsValidFilter(request.Filter);

    private static bool IsValidAccess(Guid jobId, AdminExportAccessContext access) =>
        access is not null
        && jobId != Guid.Empty
        && access.ActorUserId != Guid.Empty
        && IsBounded(access.ActorReference, 200)
        && IsBounded(access.ScopeHash, 200)
        && IsBounded(access.CorrelationId, 200);

    private static bool IsValidFilter(AdminExportFilter filter)
    {
        if (filter.OccurredFromUtc is { Kind: not DateTimeKind.Utc }
            || filter.OccurredToUtc is { Kind: not DateTimeKind.Utc }
            || filter.OccurredFromUtc > filter.OccurredToUtc
            || filter.OccurredFromUtc is not null
                && filter.OccurredToUtc - filter.OccurredFromUtc > MaximumFilterRange
            || filter.Action is not null && !IsBounded(filter.Action, 100))
        {
            return false;
        }

        return filter.SourceStream is null
            || filter.SourceStream is "audit" or "identity-security";
    }

    private static bool IsSafeRow(AdminAuditExportRow row) =>
        row.Id != Guid.Empty
        && row.OccurredAtUtc.Kind is DateTimeKind.Utc
        && IsBounded(row.ActorReference, 200)
        && IsBounded(row.Action, 100)
        && IsBounded(row.EntityType, 100)
        && IsBounded(row.EntityId, 200)
        && IsBounded(row.Reason, 500)
        && IsSafeSummary(row.RedactedBeforeSummary)
        && IsSafeSummary(row.RedactedAfterSummary)
        && IsBounded(row.CorrelationId, 200)
        && row.SourceStream is "audit" or "identity-security"
        && IsBounded(row.ScopeHash, 200);

    private static bool MatchesFilter(AdminAuditExportRow row, AdminExportFilter filter) =>
        (filter.OccurredFromUtc is null || row.OccurredAtUtc >= filter.OccurredFromUtc)
        && (filter.OccurredToUtc is null || row.OccurredAtUtc <= filter.OccurredToUtc)
        && (filter.ActorId is null || row.ActorId == filter.ActorId)
        && (filter.Action is null
            || string.Equals(row.Action, filter.Action.Trim(), StringComparison.Ordinal))
        && (filter.SourceStream is null
            || string.Equals(
                row.SourceStream,
                filter.SourceStream,
                StringComparison.Ordinal));

    private static bool IsSafeSummary(string value) =>
        IsBounded(value, 4_000)
        && !ProhibitedSummaryFields.Any(field =>
            value.Contains(field, StringComparison.OrdinalIgnoreCase));

    private static bool IsBounded(string value, int maximumLength) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= maximumLength
        && !value.Any(char.IsControl);

    private static byte[] BuildCsv(IReadOnlyCollection<AdminAuditExportRow> rows)
    {
        var builder = new StringBuilder(
            "id,occurredAtUtc,actorId,actor,action,entityType,entityId,reason,before,after,correlationId,sourceStream\r\n");
        foreach (var row in rows.OrderByDescending(row => row.OccurredAtUtc).ThenByDescending(row => row.Id))
        {
            builder.AppendJoin(
                ',',
                Csv(row.Id.ToString("D")),
                Csv(row.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture)),
                Csv(row.ActorId?.ToString("D") ?? string.Empty),
                Csv(row.ActorReference),
                Csv(row.Action),
                Csv(row.EntityType),
                Csv(row.EntityId),
                Csv(row.Reason),
                Csv(row.RedactedBeforeSummary),
                Csv(row.RedactedAfterSummary),
                Csv(row.CorrelationId),
                Csv(row.SourceStream));
            builder.Append("\r\n");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Csv(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static AdminExportResult Invalid(string errorCode) =>
        new(AdminExportOutcome.Invalid, ErrorCode: errorCode);

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private async Task DeleteUnpublishedArtifactAsync(Guid? artifactId)
    {
        if (artifactId is null)
        {
            return;
        }

        try
        {
            await _artifactStore.DeleteAsync(artifactId.Value, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch
        {
            // The bounded retention cleanup owns unreachable demo artifacts.
        }
    }

    private static bool IsStorageFailure(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or TimeoutException
            or InvalidOperationException;
}
