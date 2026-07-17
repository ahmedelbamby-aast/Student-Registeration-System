namespace StudentRegistration.ContractTests.Specs.Spec017;

public sealed class Endpoint04ContractTests
{
    [Fact]
    public void Export_status_reauthorizes_owner_and_current_scope()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/exports/{jobId}",
            "## GET /api/admin/exports/{jobId}/download");

        Spec017ContractAssertions.ContainsAll(
            section,
            "original owner with the still-current bound scope",
            "AdminAudit.Export.ReadAll",
            "reauthorizes on every request",
            "privacy-preserving 404",
            "not a 403 existence oracle");
    }

    [Fact]
    public void Export_status_finalizes_pending_complete_failure_and_expiry()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/exports/{jobId}",
            "## GET /api/admin/exports/{jobId}/download");

        Spec017ContractAssertions.ContainsAll(
            section,
            "| Pending or Running | 202 |",
            "| Complete or safely Failed | 200 |",
            "| Invalid `jobId` | 400 |",
            "| Missing authentication | 401 |",
            "| Missing resource or current owner/row scope | 404 |",
            "| Expired job or artifact | 410 |",
            "| Named limit exceeded | 429 |",
            "| Status store unavailable | 503 |",
            "| Unexpected failure | 500 |",
            "No 409 conflict is defined");
    }
}
