using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class AdminDashboardPageContractTests
{
    [Fact]
    public void Adm_01_consumes_only_authoritative_context_and_metrics_contracts()
    {
        const string path = "src/StudentRegistration.Client/Pages/AdminDashboardPage.razor";
        Assert.True(
            RepositoryFiles.Exists(path),
            "Expected-red for T073: ADM-01 canonical page is intentionally absent until T074.");
        var page = RepositoryFiles.Read(path);
        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Operations/AdminOperationsApiClient.cs");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/admin\"",
            "data-route-id=\"ADM-01\"",
            "GetAppContextAsync",
            "GetMetricsAsync",
            "Admin",
            "ObservedAtUtc",
            "AvailabilityState",
            "ReconciliationAlerts");
        RepositoryFiles.ContainsAll(
            client,
            "AdminOperationsApiClient",
            "GetMetricsAsync",
            "\"/api/admin/operations/metrics\"",
            "termId",
            "registrationWindowId");
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", page, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/operations/metrics", page, StringComparison.Ordinal);
    }
}
