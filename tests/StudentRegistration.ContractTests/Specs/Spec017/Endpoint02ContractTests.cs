namespace StudentRegistration.ContractTests.Specs.Spec017;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Audit_contract_is_scoped_paged_parameterized_and_stably_ordered()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/audit",
            "## POST /api/admin/exports");

        Spec017ContractAssertions.ContainsAll(
            section,
            "AdminAudit.Read",
            "AdminOperationsRead",
            "pageSize",
            "occurredFromUtc",
            "occurredToUtc",
            "sourceStream",
            "maximum page size 100",
            "occurredAtUtc` descending then `id` descending",
            "All filters are safely parameterized",
            "omitted before paging/counting");
    }

    [Fact]
    public void Audit_contract_finalizes_response_authorization_and_failure_shapes()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/audit",
            "## POST /api/admin/exports");

        Spec017ContractAssertions.ContainsAll(
            section,
            "| Scoped page | 200 |",
            "| Invalid paging, time range, UUID, action, or source stream | 400 |",
            "| Missing authentication | 401 |",
            "| Missing `AdminAudit.Read` | 403 |",
            "| Named limit exceeded | 429 |",
            "| Audit sources unavailable | 503 |",
            "| Unexpected failure | 500 |",
            "No 404 or 409 outcome is defined");
    }

    [Fact]
    public void Audit_dto_has_redacted_chronological_correlation_fields_only()
    {
        var item = Spec017ContractAssertions.Interface("AuditEventDto");
        Spec017ContractAssertions.ContainsAll(
            item,
            "occurredAtUtc: string",
            "actorId: string",
            "actorDisplay: string",
            "action: string",
            "reason: string",
            "beforeSummary: RedactedChangeSummaryDto",
            "afterSummary: RedactedChangeSummaryDto",
            "correlationId: string",
            "sourceStream: \"audit\" | \"identity-security\"");
        Spec017ContractAssertions.Excludes(item, "password", "connectionString", "metadataJson");
    }
}
