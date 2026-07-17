namespace StudentRegistration.ContractTests.Specs.Spec017;

public sealed class Endpoint05ContractTests
{
    [Fact]
    public void Download_contract_reauthorizes_audits_and_returns_safe_headers()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/exports/{jobId}/download",
            "## Worker lease contract");

        Spec017ContractAssertions.ContainsAll(
            section,
            "AdminAudit.Export.ReadAll",
            "AdminExportDownload",
            "Complete, unexpired job",
            "audited before",
            "Content-Disposition",
            "Cache-Control: no-store",
            "X-Content-Type-Options: nosniff",
            "no artifact bytes");
    }

    [Fact]
    public void Download_contract_finalizes_not_ready_expired_and_failure_shapes()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/exports/{jobId}/download",
            "## Worker lease contract");

        Spec017ContractAssertions.ContainsAll(
            section,
            "| Authorized Complete artifact | 200 |",
            "| Invalid `jobId` | 400 |",
            "| Missing authentication | 401 |",
            "| Missing resource or current owner/row scope | 404 |",
            "| Pending, Running, Failed, or artifact not published | 409 |",
            "EXPORT_NOT_READY",
            "| Expired job or artifact | 410 |",
            "EXPORT_EXPIRED",
            "| Named limit exceeded | 429 |",
            "| Artifact/audit persistence unavailable | 503 |",
            "| Unexpected failure | 500 |");
    }

    [Fact]
    public void Worker_contract_is_durable_bounded_and_single_publisher()
    {
        var section = Spec017ContractAssertions.Section(
            "## Worker lease contract",
            "## Delegated command boundary");

        Spec017ContractAssertions.ContainsAll(
            section,
            "generation is asynchronous",
            "one conditional SQL update",
            "60-second LeaseExpiresAtUtc",
            "Only the current lease owner",
            "AttemptCount is capped at three",
            "exactly one ArtifactId",
            "publish at most one artifact",
            "reclaim only after lease expiry",
            "status and download return 410");
    }
}
