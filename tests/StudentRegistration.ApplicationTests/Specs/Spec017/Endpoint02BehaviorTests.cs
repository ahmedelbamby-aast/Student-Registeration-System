namespace StudentRegistration.ApplicationTests.Specs.Spec017;

public sealed class Endpoint02BehaviorTests
{
    private const string QueryPath =
        "src/StudentRegistration.StaffAdministration/Application/AuditEventQueries.cs";
    private const string EndpointPath =
        "src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs";

    [Fact]
    public void Audit_query_merges_scoped_streams_redacts_before_paging_and_orders_stably()
    {
        var query = Spec017BehaviorFiles.FutureSource(
            QueryPath,
            "Expected red for T028: AuditEventQueries is intentionally absent until T070.")
            + Spec017BehaviorFiles.FutureSource(
                "src/StudentRegistration.Infrastructure.SqlServer/Admin/SqlAdminAuditReader.cs",
                "Expected red for T028: SqlAdminAuditReader is intentionally absent until T070.");

        Spec017BehaviorFiles.ContainsAll(
            query,
            "AuditEvent",
            "SecurityEvent",
            "AsNoTracking",
            "Redacted",
            "CorrelationId",
            "SourceStream",
            "OccurredAtUtc",
            "OrderByDescending",
            "ThenByDescending",
            "PageSize",
            "100");
        Assert.DoesNotContain("FromSqlRaw", query, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteUpdate", query, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteDelete", query, StringComparison.Ordinal);
    }

    [Fact]
    public void Audit_endpoint_validates_filters_and_maps_scope_without_an_existence_oracle()
    {
        var endpoint = Spec017BehaviorFiles.FutureSource(
            EndpointPath,
            "Expected red for T028: SPEC-017 audit handler is intentionally absent until T091.");

        Spec017BehaviorFiles.ContainsAll(
            endpoint,
            "/api/admin/audit",
            "AdminAudit.Read",
            "AdminOperationsRead",
            "AuditEventQueries",
            "AUDIT_FILTER_INVALID",
            "PAGE_SIZE_INVALID",
            "RATE_LIMITED",
            "AUDIT_UNAVAILABLE",
            "StatusCodes.Status200OK",
            "StatusCodes.Status400BadRequest",
            "StatusCodes.Status429TooManyRequests",
            "StatusCodes.Status503ServiceUnavailable",
            "CancellationToken");
        Assert.DoesNotContain("MapPut(\"/api/admin/audit", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("MapDelete(\"/api/admin/audit", endpoint, StringComparison.Ordinal);
    }
}
