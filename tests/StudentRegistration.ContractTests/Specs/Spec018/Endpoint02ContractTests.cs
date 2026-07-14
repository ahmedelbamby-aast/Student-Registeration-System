using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec018;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Metrics_contract_covers_validation_authentication_authorization_and_errors()
    {
        var contract = RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/contracts/api.md");

        RepositoryFiles.ContainsAll(
            contract,
            "## GET /api/operations/metrics",
            "authenticated server-derived `Admin` role",
            "default `1`/`20` and maximum `100`",
            "`Page<OperationalMetric>`",
            "| Invalid page/pageSize | 400 | `ApiError` with `PAGE_SIZE_INVALID` |",
            "| Unauthenticated | 401 | `ApiError` with `AUTHENTICATION_REQUIRED` |",
            "| Authenticated non-Admin | 403 | `ApiError` with `ACCESS_DENIED` |",
            "| Unexpected failure | 500 | safe `ApiError` |",
            "Conflict and domain rate-limit outcomes are not applicable");
    }

    [Fact]
    public void Metrics_contract_bounds_series_and_forbids_sensitive_dimensions()
    {
        var contract = RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/contracts/api.md");

        RepositoryFiles.ContainsAll(
            contract,
            "canonical unique metric-series key",
            "sorted allow-listed dimensions",
            "does not invent values",
            "no secret, credential, identity",
            "Unknown metric or dimension names are rejected");
        Assert.DoesNotContain("Operations.Read", contract, StringComparison.Ordinal);
    }
}
