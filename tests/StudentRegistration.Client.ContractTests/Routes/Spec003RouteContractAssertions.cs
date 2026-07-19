using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

internal static class Spec003RouteContractAssertions
{
    public static void AssertRoute(
        string routeId,
        string taskId,
        string pageName,
        string routeTemplate,
        params string[] authorityTokens)
    {
        var page = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{pageName}.razor");
        var design = RepositoryFiles.Read(
            $"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");
        using var readiness = JsonDocument.Parse(RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/implementation-readiness-2026-07-19.json"));

        Assert.Contains($"@page \"{routeTemplate}\"", page, StringComparison.Ordinal);
        Assert.Contains($"data-route-id=\"{routeId}\"", page, StringComparison.Ordinal);
        Assert.Contains($"{routeId}-CONTRACT-{taskId}", design, StringComparison.Ordinal);
        Assert.Contains("\"dataContracts\"", design, StringComparison.Ordinal);
        Assert.Contains("reason", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            readiness.RootElement.GetProperty("routes").EnumerateArray(),
            route => route.GetProperty("routeId").GetString() == routeId);
        RepositoryFiles.ContainsAll(page, authorityTokens);
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", page, StringComparison.Ordinal);
    }
}
