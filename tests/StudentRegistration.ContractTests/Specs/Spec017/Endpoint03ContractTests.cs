namespace StudentRegistration.ContractTests.Specs.Spec017;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Export_request_is_scoped_idempotent_audited_and_antiforgery_protected()
    {
        var section = Spec017ContractAssertions.Section(
            "## POST /api/admin/exports",
            "## GET /api/admin/exports/{jobId}");

        Spec017ContractAssertions.ContainsAll(
            section,
            "AdminAudit.Export",
            "AdminExportCreate",
            "same-origin antiforgery token",
            "clientRequestId",
            "OwnerId, ScopeHash, RequestHash and Pending state",
            "request audit event is atomic",
            "Location: /api/admin/exports/{jobId}",
            "IDEMPOTENCY_KEY_REUSED",
            "no duplicate job or audit event");
    }

    [Fact]
    public void Export_request_finalizes_all_response_shapes()
    {
        var section = Spec017ContractAssertions.Section(
            "## POST /api/admin/exports",
            "## GET /api/admin/exports/{jobId}");

        Spec017ContractAssertions.ContainsAll(
            section,
            "| New durable Pending job | 202 |",
            "| Same owner/scope/key/canonical payload replay | 202 |",
            "| Malformed body, invalid UUID/filter, or unsupported export type | 400 |",
            "| Invalid or missing antiforgery token | 400 |",
            "| Missing authentication | 401 |",
            "| Missing `AdminAudit.Export` | 403 |",
            "| Same scoped key with a different canonical payload | 409 |",
            "| Named limit exceeded | 429 |",
            "| Durable job/audit persistence unavailable | 503 |",
            "| Unexpected failure | 500 |");
    }

    [Fact]
    public void Export_request_cannot_claim_server_owned_fields()
    {
        var request = Spec017ContractAssertions.Interface("CreateExportRequest");
        Spec017ContractAssertions.ContainsAll(
            request,
            "clientRequestId: string",
            "exportType: \"audit\"",
            "filters: AuditExportFilterDto");
        Spec017ContractAssertions.Excludes(
            request,
            "ownerId",
            "scopeHash",
            "requestHash",
            "leaseOwnerId",
            "artifactId",
            "expiresAtUtc");
    }

    [Fact]
    public void Terminal_export_retry_requires_reauthorization_and_a_new_key()
    {
        var section = Spec017ContractAssertions.Section(
            "## POST /api/admin/exports",
            "## GET /api/admin/exports/{jobId}");

        Spec017ContractAssertions.ContainsAll(
            section,
            "Failed or Expired job is terminal",
            "never reset or re-queued",
            "After reauthorization",
            "new `clientRequestId`",
            "new Pending job",
            "Reusing the old key returns the original job",
            "IDEMPOTENCY_KEY_REUSED",
            "no retry endpoint or mutable reset command");
    }
}
