using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class SystemStatusPageContractTests
{
    [Fact]
    public void Sys_01_fixture_and_client_bind_only_safe_public_contracts()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec003/SYS-01/route-contract.json"));
        var root = document.RootElement;
        Assert.Equal("SYS-01", root.GetProperty("routeId").GetString());
        Assert.Equal("/status/{code}", root.GetProperty("routeTemplate").GetString());
        Assert.Equal(9, root.GetProperty("codes").GetArrayLength());

        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Operations/OperationsApiClient.cs");
        RepositoryFiles.ContainsAll(
            client,
            "public const string HealthPath = \"/api/health\";",
            "GetHealthAsync",
            "HealthSummary",
            "ServiceUnavailable",
            "ApiError");
        Assert.DoesNotContain("OperationalHealthRegistry", client, StringComparison.Ordinal);
    }

    [Fact]
    public void Sys_01_page_preserves_safe_codes_actions_and_reference_boundary()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SystemStatusPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/status/{Code}\"",
            "RouteStatePanel",
            "healthy",
            "degraded",
            "unhealthy",
            "403",
            "404",
            "expired",
            "maintenance",
            "offline",
            "unexpected",
            "OperationsApi.GetHealthAsync",
            "ReferenceId");
        Assert.DoesNotContain("Exception.Message", page, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", page, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionString", page, StringComparison.Ordinal);
    }
}
