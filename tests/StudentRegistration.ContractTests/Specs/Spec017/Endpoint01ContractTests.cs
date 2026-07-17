namespace StudentRegistration.ContractTests.Specs.Spec017;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Metrics_contract_declares_scope_freshness_and_degraded_semantics()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/operations/metrics",
            "## GET /api/admin/audit");

        Spec017ContractAssertions.ContainsAll(
            section,
            "AdminOperations.Metrics.Read",
            "AdminOperationsRead",
            "termId",
            "registrationWindowId",
            "observedAtUtc",
            "availabilityState",
            "traffic, success, expected rejection, server failure, fill-rate, lock-wait",
            "support reference",
            "MUST NOT replace missing values with zero");
    }

    [Fact]
    public void Metrics_contract_finalizes_every_http_outcome()
    {
        var section = Spec017ContractAssertions.Section(
            "## GET /api/admin/operations/metrics",
            "## GET /api/admin/audit");

        Spec017ContractAssertions.ContainsAll(
            section,
            "| Current or explicitly stale/degraded observation | 200 |",
            "| Invalid UUID or unsupported query combination | 400 |",
            "| Missing authentication | 401 |",
            "| Missing `AdminOperations.Metrics.Read` | 403 |",
            "| Window absent or outside row scope | 404 |",
            "| Named limit exceeded | 429 |",
            "| Metrics source unavailable and no last observation exists | 503 |",
            "| Unexpected failure | 500 |",
            "No 409 conflict is defined");
    }

    [Fact]
    public void Metrics_response_is_bounded_and_observation_only()
    {
        var response = Spec017ContractAssertions.Interface("AdminOperationsMetricsDto");
        Spec017ContractAssertions.ContainsAll(
            response,
            "observedAtUtc: string",
            "availabilityState: \"live\" | \"stale\" | \"degraded\"",
            "metrics: OperationalMetricDto[]",
            "reconciliationAlerts: RegistrationReconciliationAlertDto[]");

        var alert = Spec017ContractAssertions.Interface("RegistrationReconciliationAlertDto");
        Spec017ContractAssertions.ContainsAll(alert, "metric: string", "threshold: number", "supportReferencePath: string");
        Spec017ContractAssertions.Excludes(alert, "repairUrl", "repairCommand");
    }
}
