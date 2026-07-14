using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec018;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Health_contract_covers_every_response_and_public_authorization_outcome()
    {
        var contract = RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/contracts/api.md");

        RepositoryFiles.ContainsAll(
            contract,
            "## GET /api/health",
            "accepts no request body or query",
            "parameters and returns no component",
            "| Healthy or degraded | 200 | `HealthSummary` |",
            "| Unhealthy dependency | 503 | `HealthSummary` with `status = \"unhealthy\"` |",
            "| Unexpected failure | 500 | safe `ApiError` |",
            "Validation, authentication, authorization, conflict, and rate-limit outcomes",
            "are not applicable",
            "server `TimeProvider`",
            "host, replica, SQL, certificate, key-ring,");
    }

    [Fact]
    public void Health_contract_is_a_bounded_safe_summary_not_a_topology_api()
    {
        var contract = RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/contracts/api.md");
        var section = contract.Split("## GET /api/operations/metrics", StringSplitOptions.None)[0];

        Assert.DoesNotContain("componentStatuses", section, StringComparison.Ordinal);
        Assert.DoesNotContain("connectionString", section, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hostName", section, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replicaName", section, StringComparison.OrdinalIgnoreCase);
    }
}
