namespace StudentRegistration.Contracts.Admin;

public sealed record AdminOperationalMetricDto(
    string Name,
    double Value,
    IReadOnlyDictionary<string, string> Dimensions,
    DateTime ObservedAtUtc);

public sealed record RegistrationReconciliationAlertDto(
    string Metric,
    double Threshold,
    double ObservedValue,
    DateTime ObservedAtUtc,
    string SupportReferencePath);

public sealed record AdminOperationsMetricsDto(
    DateTime ObservedAtUtc,
    string AvailabilityState,
    IReadOnlyList<AdminOperationalMetricDto> Metrics,
    IReadOnlyList<RegistrationReconciliationAlertDto> ReconciliationAlerts);

public sealed record AdminRedactedChangeFieldDto(
    string Name,
    string DisplayValue);

public sealed record AdminRedactedChangeSummaryDto(
    IReadOnlyList<AdminRedactedChangeFieldDto> Fields,
    string RedactionVersion);

public sealed record AdminAuditEventDto(
    Guid Id,
    DateTime OccurredAtUtc,
    string ActorId,
    string ActorDisplay,
    string Action,
    string EntityType,
    string EntityId,
    string Reason,
    AdminRedactedChangeSummaryDto BeforeSummary,
    AdminRedactedChangeSummaryDto AfterSummary,
    string CorrelationId,
    string SourceStream);

public sealed record AdminAuditEventPageDto(
    IReadOnlyList<AdminAuditEventDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record AdminAuditExportFilterDto(
    DateTime? OccurredFromUtc = null,
    DateTime? OccurredToUtc = null,
    Guid? ActorId = null,
    string? Action = null,
    string? SourceStream = null);

public sealed record CreateAdminExportRequest(
    Guid ClientRequestId,
    string ExportType,
    AdminAuditExportFilterDto Filters);

public sealed record AdminExportJobDto(
    Guid JobId,
    string State,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? ExpiresAtUtc,
    int? RetryAfterSeconds,
    string? DownloadUrl,
    string? FailureCode);
