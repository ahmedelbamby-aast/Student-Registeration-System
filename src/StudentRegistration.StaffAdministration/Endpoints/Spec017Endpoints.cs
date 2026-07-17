using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Admin;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.StaffAdministration.Endpoints;

public static class Spec017Endpoints
{
    private const string MetricsPermission = "AdminOperations.Metrics.Read";
    private const string AuditReadPermission = "AdminAudit.Read";
    private const string AuditExportPermission = "AdminAudit.Export";
    private const string AuditExportReadAllPermission = "AdminAudit.Export.ReadAll";
    private const string AdminOperationsRead = "AdminOperationsRead";
    private const string AdminExportCreate = "AdminExportCreate";
    private const string AdminExportDownload = "AdminExportDownload";
    private const string RateLimitedCode = "RATE_LIMITED";
    private const int RetryAfterSeconds = 2;

    private static readonly string GlobalAdminScopeHash =
        $"SHA256:{Convert.ToHexString(SHA256.HashData("admin:global:v1"u8))}";

    public static IEndpointRouteBuilder MapSpec017Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/admin/operations/metrics", GetMetricsAsync)
            .RequireAuthorization(MetricsPermission)
            .RequireRateLimiting(AdminOperationsRead)
            .Produces<AdminOperationsMetricsDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/admin/audit", SearchAuditAsync)
            .RequireAuthorization(AuditReadPermission)
            .RequireRateLimiting(AdminOperationsRead)
            .Produces<AdminAuditEventPageDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapPost("/api/admin/exports", CreateExportAsync)
            .RequireAuthorization(AuditExportPermission)
            .RequireRateLimiting(AdminExportCreate)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<AdminExportJobDto>(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/admin/exports/{jobId:guid}", GetExportStatusAsync)
            .RequireAuthorization(AuditExportPermission)
            .RequireRateLimiting(AdminOperationsRead)
            .Produces<AdminExportJobDto>(StatusCodes.Status200OK)
            .Produces<AdminExportJobDto>(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status410Gone)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/admin/exports/{jobId:guid}/download", DownloadExportAsync)
            .RequireAuthorization(AuditExportPermission)
            .RequireRateLimiting(AdminExportDownload)
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status410Gone)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }

    private static async Task<IResult> GetMetricsAsync(
        [FromQuery] Guid? termId,
        [FromQuery] Guid? registrationWindowId,
        HttpContext context,
        [FromServices] AdminMetricsQuery query,
        CancellationToken cancellationToken)
    {
        if (termId is null || termId == Guid.Empty
            || registrationWindowId is null || registrationWindowId == Guid.Empty)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "METRICS_FILTER_INVALID",
                "A term and registration-window scope are required.");
        }

        try
        {
            var result = await query.ExecuteAsync(
                new OperationalMetricScope(termId.Value, registrationWindowId.Value),
                cancellationToken);
            if (result.ObservedAtUtc is null)
            {
                return Error(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    "METRICS_UNAVAILABLE",
                    "Operational metrics are temporarily unavailable.");
            }

            return Results.Ok(new AdminOperationsMetricsDto(
                result.ObservedAtUtc.Value,
                result.AvailabilityState.ToString().ToLowerInvariant(),
                result.Metrics.Select(metric => new AdminOperationalMetricDto(
                    metric.Name,
                    metric.Value,
                    metric.Dimensions,
                    metric.ObservedAtUtc)).ToArray(),
                result.Alerts.Select(alert => new RegistrationReconciliationAlertDto(
                    alert.Metric,
                    alert.Threshold,
                    alert.ObservedValue,
                    alert.ObservedAtUtc,
                    alert.SupportReferencePath)).ToArray()));
        }
        catch (ArgumentException)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "METRICS_FILTER_INVALID",
                "The metrics filter is invalid.");
        }
        catch (Exception exception) when (IsDependencyFailure(exception))
        {
            return Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "METRICS_UNAVAILABLE",
                "Operational metrics are temporarily unavailable.");
        }
    }

    private static async Task<IResult> SearchAuditAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] DateTime? occurredFromUtc,
        [FromQuery] DateTime? occurredToUtc,
        [FromQuery] string? actorId,
        [FromQuery] string? action,
        [FromQuery] string? sourceStream,
        HttpContext context,
        [FromServices] AuditEventQueries queries,
        CancellationToken cancellationToken)
    {
        if (!TrySource(sourceStream, out var parsedSource))
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "AUDIT_FILTER_INVALID",
                "The audit source filter is invalid.");
        }

        try
        {
            var result = await queries.SearchAsync(
                new AdminAuditQuery(
                    AdminAuditScope.All(includeIdentitySecurityEvents: true),
                    page ?? 1,
                    pageSize ?? 20,
                    occurredFromUtc,
                    occurredToUtc,
                    actorId,
                    action,
                    parsedSource),
                cancellationToken);
            return Results.Ok(ToContract(result));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            var code = exception.ParamName == "PageSize"
                || exception.ParamName == "pageSize"
                ? "PAGE_SIZE_INVALID"
                : "AUDIT_FILTER_INVALID";
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                code,
                "The audit query is invalid.");
        }
        catch (ArgumentException)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "AUDIT_FILTER_INVALID",
                "The audit query is invalid.");
        }
        catch (Exception exception) when (IsDependencyFailure(exception))
        {
            return Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "AUDIT_UNAVAILABLE",
                "Audit data is temporarily unavailable.");
        }
    }

    private static async Task<IResult> CreateExportAsync(
        [FromBody] CreateAdminExportRequest request,
        HttpContext context,
        [FromServices] AuditExportService service,
        [FromServices] TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!TryUserId(context.User, out var ownerId)
            || request is null
            || request.ClientRequestId == Guid.Empty
            || !string.Equals(request.ExportType, "audit", StringComparison.Ordinal)
            || request.Filters is null)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "EXPORT_REQUEST_INVALID",
                "The export request is invalid.");
        }

        var result = await service.RequestAsync(
            new AdminExportRequest(
                ownerId,
                request.ClientRequestId,
                GlobalAdminScopeHash,
                ToFilter(request.Filters),
                ActorReference(ownerId),
                CorrelationId(context),
                timeProvider.GetUtcNow().UtcDateTime),
            cancellationToken);
        if (result.Outcome is AdminExportOutcome.Created or AdminExportOutcome.Replay
            && result.Job is not null)
        {
            context.Response.Headers.Location = $"/api/admin/exports/{result.Job.Id:D}";
            context.Response.Headers["Retry-After"] = RetryAfterSeconds.ToString();
            return Results.Json(
                ToContract(result.Job),
                statusCode: StatusCodes.Status202Accepted);
        }

        return ExportError(context, result);
    }

    private static async Task<IResult> GetExportStatusAsync(
        [FromRoute] Guid jobId,
        HttpContext context,
        [FromServices] AuditExportService service,
        CancellationToken cancellationToken)
    {
        if (!TryAccess(context, out var access))
        {
            return Error(
                context,
                StatusCodes.Status404NotFound,
                "RESOURCE_NOT_FOUND",
                "The export was not found.");
        }

        var result = await service.GetStatusAsync(jobId, access!, cancellationToken);
        if (result.Outcome is AdminExportOutcome.Succeeded && result.Job is not null)
        {
            var statusCode = result.Job.State is ExportJobState.Pending or ExportJobState.Running
                ? StatusCodes.Status202Accepted
                : StatusCodes.Status200OK;
            if (statusCode == StatusCodes.Status202Accepted)
            {
                context.Response.Headers["Retry-After"] = RetryAfterSeconds.ToString();
            }

            return Results.Json(ToContract(result.Job), statusCode: statusCode);
        }

        return ExportError(context, result);
    }

    private static async Task<IResult> DownloadExportAsync(
        [FromRoute] Guid jobId,
        HttpContext context,
        [FromServices] AuditExportService service,
        CancellationToken cancellationToken)
    {
        if (!TryAccess(context, out var access))
        {
            return Error(
                context,
                StatusCodes.Status404NotFound,
                "RESOURCE_NOT_FOUND",
                "The export was not found.");
        }

        var result = await service.DownloadAsync(jobId, access!, cancellationToken);
        if (result.Outcome is AdminExportOutcome.Succeeded && result.Content is not null)
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            return Results.File(
                result.Content.Value.ToArray(),
                "text/csv; charset=utf-8",
                $"admin-audit-{jobId:N}.csv");
        }

        return ExportError(context, result);
    }

    private static IResult ExportError(HttpContext context, AdminExportResult result) =>
        result.Outcome switch
        {
            AdminExportOutcome.Invalid => Error(
                context,
                StatusCodes.Status400BadRequest,
                result.ErrorCode ?? "EXPORT_REQUEST_INVALID",
                "The export request is invalid."),
            AdminExportOutcome.IdempotencyKeyReused => Error(
                context,
                StatusCodes.Status409Conflict,
                "IDEMPOTENCY_KEY_REUSED",
                "The idempotency key was already used for different input."),
            AdminExportOutcome.NotReady => Error(
                context,
                StatusCodes.Status409Conflict,
                "EXPORT_NOT_READY",
                "The export is not ready."),
            AdminExportOutcome.Expired => Error(
                context,
                StatusCodes.Status410Gone,
                "EXPORT_EXPIRED",
                "The export has expired."),
            AdminExportOutcome.NotFound => Error(
                context,
                StatusCodes.Status404NotFound,
                "RESOURCE_NOT_FOUND",
                "The export was not found."),
            _ => Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                result.ErrorCode ?? "EXPORT_UNAVAILABLE",
                "The export service is temporarily unavailable.")
        };

    private static AdminAuditEventPageDto ToContract(AuditEventPageDto page) => new(
        page.Items.Select(item => new AdminAuditEventDto(
            item.Id,
            item.OccurredAtUtc,
            item.ActorId,
            item.ActorDisplay,
            item.Action,
            item.EntityType,
            item.EntityId,
            item.Reason,
            ToContract(item.BeforeSummary),
            ToContract(item.AfterSummary),
            item.CorrelationId,
            item.SourceStream)).ToArray(),
        page.Page,
        page.PageSize,
        page.TotalCount);

    private static AdminRedactedChangeSummaryDto ToContract(
        RedactedChangeSummaryDto summary) => new(
        summary.Fields.Select(field => new AdminRedactedChangeFieldDto(
            field.Name,
            field.DisplayValue)).ToArray(),
        summary.RedactionVersion);

    private static AdminExportJobDto ToContract(ExportJob job) => new(
        job.Id,
        job.State.ToString().ToLowerInvariant(),
        job.CreatedAtUtc,
        job.CompletedAtUtc,
        job.ExpiresAtUtc,
        job.State is ExportJobState.Pending or ExportJobState.Running
            ? RetryAfterSeconds
            : null,
        job.State is ExportJobState.Complete
            ? $"/api/admin/exports/{job.Id:D}/download"
            : null,
        job.FailureCode);

    private static AdminExportFilter ToFilter(AdminAuditExportFilterDto filter) => new(
        filter.OccurredFromUtc,
        filter.OccurredToUtc,
        filter.ActorId,
        filter.Action,
        filter.SourceStream);

    private static bool TrySource(string? value, out AuditSourceStream? source)
    {
        source = value?.Trim().ToLowerInvariant() switch
        {
            null or "" => null,
            "audit" => AuditSourceStream.Audit,
            "identity-security" => AuditSourceStream.IdentitySecurity,
            _ => (AuditSourceStream?)(-1)
        };
        return source is not (AuditSourceStream)(-1);
    }

    private static bool TryAccess(
        HttpContext context,
        out AdminExportAccessContext? access)
    {
        access = null;
        if (!TryUserId(context.User, out var userId))
        {
            return false;
        }

        access = new(
            userId,
            ActorReference(userId),
            GlobalAdminScopeHash,
            context.User.HasClaim(
                "permission",
                AuditExportReadAllPermission),
            CorrelationId(context));
        return true;
    }

    private static bool TryUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId)
        && userId != Guid.Empty;

    private static string ActorReference(Guid userId) => $"user:{userId:N}";

    private static string CorrelationId(HttpContext context) =>
        string.IsNullOrWhiteSpace(context.TraceIdentifier)
            ? Guid.NewGuid().ToString("N")
            : context.TraceIdentifier;

    private static IResult Error(
        HttpContext context,
        int statusCode,
        string code,
        string message) => Results.Json(
        new ApiError(code, message, CorrelationId(context)),
        statusCode: statusCode);

    private static bool IsDependencyFailure(Exception exception) =>
        exception is InvalidOperationException
            or TimeoutException
            or IOException;
}
