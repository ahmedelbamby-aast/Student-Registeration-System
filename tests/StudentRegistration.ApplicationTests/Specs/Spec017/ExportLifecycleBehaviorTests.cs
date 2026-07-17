namespace StudentRegistration.ApplicationTests.Specs.Spec017;

public sealed class ExportLifecycleBehaviorTests
{
    private const string ServicePath =
        "src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs";
    private const string EndpointPath =
        "src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs";

    [Fact]
    public void Export_service_binds_idempotency_owner_scope_and_request_before_creating_work()
    {
        var service = Spec017BehaviorFiles.FutureSource(
            ServicePath,
            "Expected red for T031: AuditExportService is intentionally absent until T056.");

        Spec017BehaviorFiles.ContainsAll(
            service,
            "ClientRequestId",
            "OwnerId",
            "ScopeHash",
            "RequestHash",
            "Pending",
            "IDEMPOTENCY_KEY_REUSED",
            "Audit",
            "CancellationToken");
    }

    [Fact]
    public void Export_worker_uses_conditional_renewable_lease_and_one_artifact_publication()
    {
        var service = Spec017BehaviorFiles.FutureSource(
            ServicePath,
            "Expected red for T031: durable export worker behavior is intentionally absent until T056.");

        Spec017BehaviorFiles.ContainsAll(
            service,
            "LeaseOwnerId",
            "LeaseExpiresAtUtc",
            "AttemptCount",
            "TimeSpan.FromSeconds(60)",
            "3",
            "ArtifactId",
            "ExpiresAtUtc",
            "Claim",
            "Renew",
            "Publish");
        Assert.DoesNotContain("static readonly ConcurrentQueue", service, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Task.Run(", service, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_endpoints_reauthorize_status_and_download_and_map_the_full_lifecycle()
    {
        var endpoint = Spec017BehaviorFiles.FutureSource(
            EndpointPath,
            "Expected red for T031: SPEC-017 export handlers are intentionally absent until T091.");

        Spec017BehaviorFiles.ContainsAll(
            endpoint,
            "MapPost(\"/api/admin/exports\"",
            "MapGet(\"/api/admin/exports/{jobId:guid}\"",
            "MapGet(\"/api/admin/exports/{jobId:guid}/download\"",
            "AdminAudit.Export",
            "AdminAudit.Export.ReadAll",
            "AdminExportCreate",
            "AdminExportDownload",
            "RequireAntiforgeryTokenAttribute",
            "IDEMPOTENCY_KEY_REUSED",
            "EXPORT_NOT_READY",
            "EXPORT_EXPIRED",
            "StatusCodes.Status202Accepted",
            "StatusCodes.Status409Conflict",
            "StatusCodes.Status410Gone",
            "StatusCodes.Status429TooManyRequests",
            "StatusCodes.Status503ServiceUnavailable");
    }
}
