using Microsoft.AspNetCore.Components;
using StudentRegistration.Client.Components.Registration;
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
            "--srs-sizing-content-shell",
            "--srs-color-semantic-surface-raised",
            "@media (prefers-reduced-motion: reduce)",
            "@media (forced-colors: active)");
        RepositoryFiles.ContainsAll(app,
            "overflow-wrap: anywhere",
            "min-inline-size: 0",
            "@media (prefers-reduced-motion: reduce)",
            "@media (forced-colors: active)");
        RepositoryFiles.ContainsAll(shell,
            ".srs-app-shell__container",
            ".srs-app-shell__navigation",
            "position: sticky",
            "@media (max-width:",
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
}
