using Microsoft.AspNetCore.Components;
using System.Text.Json;
using StudentRegistration.Client.Components.Registration;
using StudentRegistration.Client.Components.Layout;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests;

public sealed class SharedDesignSystemContractTests
{
    [Fact]
    public void Global_design_system_covers_responsive_accessibility_preferences()
    {
        var tokens = RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/css/design-tokens.css");
        var app = RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/css/app.css");
        var shell = RepositoryFiles.Read("src/StudentRegistration.Client/Components/Layout/AppShell.razor.css");

        RepositoryFiles.ContainsAll(tokens,
            "--srs-sizing-content-wide",
            "--srs-color-semantic-surface-default",
            "@media (prefers-reduced-motion: reduce)",
            "@media (forced-colors: active)");
        RepositoryFiles.ContainsAll(app,
            "overflow-wrap: anywhere",
            "min-inline-size: 0",
            "@media (prefers-reduced-motion: reduce)",
            "@media (forced-colors: active)");
        RepositoryFiles.ContainsAll(shell,
            ".srs-app-shell__sidebar",
            ".srs-app-shell__navigation",
            "position: sticky",
            "@media (min-width:",
            "@media (forced-colors: active)");
    }

    [Fact]
    public void Capacity_component_contract_cannot_accept_person_identifiers()
    {
        var parameterNames = typeof(CapacityBreakdown)
            .GetProperties()
            .Where(property => property.GetCustomAttributes(typeof(ParameterAttribute), true).Length > 0)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(parameterNames, name =>
            name.Contains("Student", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("User", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Person", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Identity", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(
            ["AccessibleName", "Available", "AvailableLabel", "Enrolled", "EnrolledLabel", "Held", "HeldLabel", "Total", "TotalLabel"],
            parameterNames.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Every_authenticated_route_uses_the_shared_composition_and_central_navigation()
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(".specify/route-manifest.json"));
        var routes = manifest.RootElement.GetProperty("routes").EnumerateArray().ToArray();
        var authenticated = routes.Where(route =>
            !route.GetProperty("id").GetString()!.StartsWith("AUTH-", StringComparison.Ordinal) &&
            route.GetProperty("id").GetString() != "SYS-01").ToArray();

        Assert.Equal(24, authenticated.Length);
        foreach (var route in authenticated)
        {
            var pageName = route.GetProperty("page").GetString()!;
            var matches = Directory.EnumerateFiles(
                    RepositoryFiles.PathTo("src/StudentRegistration.Client/Pages"),
                    $"{pageName}.razor",
                    SearchOption.AllDirectories)
                .ToArray();
            var path = Assert.Single(matches);
            var source = File.ReadAllText(path);
            Assert.True(
                source.Contains("<AuthenticatedPage", StringComparison.Ordinal) ||
                source.Contains("<ApprovalWorkspace", StringComparison.Ordinal),
                $"{pageName} must use the shared authenticated composition directly or through ApprovalWorkspace.");
        }

        foreach (var item in Enum.GetValues<WorkspaceKind>().SelectMany(WorkspaceNavigationCatalog.For))
        {
            var route = Assert.Single(authenticated, candidate =>
                candidate.GetProperty("id").GetString() == item.RouteId);
            Assert.Equal(route.GetProperty("template").GetString(), item.Href);
        }
    }
}
