using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec017;

public sealed class NFR_4EvidenceTests
{
    private const string ResultsPath =
        "docs/release-evidence/SPEC-017-NFR-4-results.json";
    private const string EvidencePath =
        "docs/release-evidence/SPEC-017-NFR-4.md";

    [Fact]
    public void Raw_matrix_has_every_required_control_and_no_failure()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(ResultsPath));
        var root = document.RootElement;
        Assert.Equal("spec017-nfr4-results/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal(
            "e3c567f43e608597a66b9d8e5f1f0bcfc4b04594",
            root.GetProperty("implementationCommit").GetString());
        Assert.Equal("PASS", root.GetProperty("result").GetString());

        var matrix = root.GetProperty("matrix").EnumerateArray().ToArray();
        Assert.Equal(
            ["anti-forgery", "audit", "authorization", "concurrency", "validation"],
            matrix.Select(row => row.GetProperty("control").GetString())
                .Order(StringComparer.Ordinal));
        Assert.All(matrix, row =>
        {
            Assert.True(row.GetProperty("passed").GetInt32() > 0);
            Assert.Equal(0, row.GetProperty("failed").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("evidence").GetString()));
        });
        Assert.Equal(2, root.GetProperty("environment").GetProperty("replicas").GetInt32());
        Assert.Equal(0, root.GetProperty("phase5Regression").GetProperty("failed").GetInt32());
    }

    [Fact]
    public void Matrix_is_bound_to_authorization_audit_concurrency_antiforgery_and_validation_sources()
    {
        var authorization = RepositoryFiles.Read(
            "tests/StudentRegistration.AuthorizationTests/Spec017EndpointAuthorizationTests.cs");
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs");
        var audit = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Audit/AuditAggregationConformanceTests.cs");
        var export = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Admin/AuditExportSqlConcurrencyTests.cs");
        var confirmation = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs");
        var application = RepositoryFiles.Read(
            "tests/StudentRegistration.ApplicationTests/Specs/Spec017/ExportLifecycleBehaviorTests.cs");

        RepositoryFiles.ContainsAll(
            authorization,
            "AdminOperationsMetricsRead",
            "AdminAuditRead",
            "AdminAuditExport",
            "AuthorizeAsync");
        RepositoryFiles.ContainsAll(
            endpoint,
            "RequireAntiforgeryTokenAttribute(true)",
            "METRICS_FILTER_INVALID",
            "AUDIT_FILTER_INVALID",
            "EXPORT_REQUEST_INVALID");
        RepositoryFiles.ContainsAll(
            audit,
            "Canonical_sources_and_atomic_writer_remain_upstream_owned",
            "Real_sql_merge_scopes_before_paging_redacts_and_orders_both_streams");
        RepositoryFiles.ContainsAll(
            export,
            "Two_replicas_share_idempotency_lease_artifact_scope_audit_and_expiry",
            "TryClaimNextAsync",
            "AdminExportRequested",
            "AdminExportDownloaded");
        RepositoryFiles.ContainsAll(
            confirmation,
            "Owner_preview_is_bound",
            "Final_admin_write_skew_is_serialized");
        RepositoryFiles.ContainsAll(
            application,
            "Export_endpoints_reauthorize_status_and_download_and_map_the_full_lifecycle");
    }

    [Fact]
    public void Evidence_records_reproduction_commit_environment_and_raw_result()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-017 NFR-4 Admin Action Coverage Evidence",
            "e3c567f43e608597a66b9d8e5f1f0bcfc4b04594",
            "Windows, .NET 10",
            "SQL Server",
            "2022 containers",
            "SPEC-017-NFR-4-results.json",
            "103 focused checks",
            "0 failed",
            "TreatWarningsAsErrors=true",
            "**Result: PASS.**");
        Assert.DoesNotContain("TODO", evidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", evidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PLACEHOLDER", evidence, StringComparison.OrdinalIgnoreCase);
    }
}
