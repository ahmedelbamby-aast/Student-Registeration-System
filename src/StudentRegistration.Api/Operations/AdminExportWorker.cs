using System.Text.Json;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.Api.Operations;

public sealed class AdminExportWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AdminExportWorker> logger) : BackgroundService
{
    private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromSeconds(1);
    private const int PageSize = 100;

    private readonly IServiceScopeFactory _scopeFactory =
        scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ILogger<AdminExportWorker> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _leaseOwnerId =
        $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var exportService = scope.ServiceProvider
                    .GetRequiredService<AuditExportService>();
                var claimed = await exportService.TryClaimNextAsync(
                    _leaseOwnerId,
                    stoppingToken);
                if (claimed is null)
                {
                    await Task.Delay(EmptyQueueDelay, stoppingToken);
                    continue;
                }

                await ProcessAsync(scope.ServiceProvider, exportService, claimed, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "The bounded Admin export worker iteration failed.");
                await Task.Delay(EmptyQueueDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(
        IServiceProvider services,
        AuditExportService exportService,
        ExportJob claimed,
        CancellationToken cancellationToken)
    {
        try
        {
            var filter = AuditExportService.DeserializeFilter(claimed.FilterJson);
            var queries = services.GetRequiredService<AuditEventQueries>();
            var rows = new List<AdminAuditExportRow>();
            for (var pageNumber = 1; ; pageNumber++)
            {
                var page = await queries.SearchAsync(
                    new AdminAuditQuery(
                        AdminAuditScope.All(includeIdentitySecurityEvents: true),
                        pageNumber,
                        PageSize,
                        filter.OccurredFromUtc,
                        filter.OccurredToUtc,
                        ActorId: filter.ActorId is null
                            ? null
                            : $"user:{filter.ActorId.Value:N}",
                        Action: filter.Action,
                        SourceStream: ParseSource(filter.SourceStream)),
                    cancellationToken);
                if (page.TotalCount > AuditExportService.MaximumExportRows)
                {
                    await exportService.RecordFailureAsync(
                        claimed.Id,
                        _leaseOwnerId,
                        retryable: false,
                        "EXPORT_ROW_LIMIT_EXCEEDED",
                        cancellationToken);
                    return;
                }

                rows.AddRange(page.Items.Select(item => ToExportRow(
                    item,
                    claimed.ScopeHash)));
                if (rows.Count >= page.TotalCount || page.Items.Count == 0)
                {
                    break;
                }

                if (!await exportService.RenewLeaseAsync(
                        claimed.Id,
                        _leaseOwnerId,
                        cancellationToken))
                {
                    return;
                }
            }

            var published = await exportService.PublishAsync(
                claimed,
                _leaseOwnerId,
                filter,
                rows,
                cancellationToken: cancellationToken);
            if (published.Outcome is AdminExportOutcome.StorageUnavailable)
            {
                await exportService.RecordFailureAsync(
                    claimed.Id,
                    _leaseOwnerId,
                    retryable: true,
                    "EXPORT_STORAGE_UNAVAILABLE",
                    cancellationToken);
            }
        }
        catch (ArgumentException)
        {
            await exportService.RecordFailureAsync(
                claimed.Id,
                _leaseOwnerId,
                retryable: false,
                "EXPORT_FILTER_INVALID",
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Admin export job {JobId} failed and may be retried.",
                claimed.Id);
            await exportService.RecordFailureAsync(
                claimed.Id,
                _leaseOwnerId,
                retryable: true,
                "EXPORT_SOURCE_UNAVAILABLE",
                CancellationToken.None);
        }
    }

    private static AdminAuditExportRow ToExportRow(
        AuditEventDto item,
        string scopeHash) => new(
        item.Id,
        item.OccurredAtUtc,
        ParseActorId(item.ActorId),
        item.ActorDisplay,
        item.Action,
        item.EntityType,
        item.EntityId,
        item.Reason,
        Summary(item.BeforeSummary),
        Summary(item.AfterSummary),
        item.CorrelationId,
        item.SourceStream,
        scopeHash);

    private static string Summary(RedactedChangeSummaryDto summary) =>
        JsonSerializer.Serialize(summary.Fields.ToDictionary(
            field => field.Name,
            field => field.DisplayValue,
            StringComparer.Ordinal));

    private static Guid? ParseActorId(string value)
    {
        var candidate = value.StartsWith("user:", StringComparison.Ordinal)
            ? value["user:".Length..]
            : value;
        return Guid.TryParse(candidate, out var id) ? id : null;
    }

    private static AuditSourceStream? ParseSource(string? value) => value switch
    {
        null => null,
        "audit" => AuditSourceStream.Audit,
        "identity-security" => AuditSourceStream.IdentitySecurity,
        _ => throw new ArgumentException("The persisted export source is invalid.")
    };
}
