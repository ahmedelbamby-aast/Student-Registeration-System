using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed partial class PageDesignRecordSchemaTests
{
    private const string RouteManifestPath = ".specify/route-manifest.json";
    private const string RouteInventoryPath =
        "specs/003-ux-storyboard-accessibility/design/route-inventory.md";
    private const string RecordContractPath =
        "specs/003-ux-storyboard-accessibility/design/page-design-record-contract.md";
    private const string ResponsiveContractPath =
        "specs/003-ux-storyboard-accessibility/design/responsive-layout-contract.md";
    private const string DesignIndexPath =
        "specs/003-ux-storyboard-accessibility/design/README.md";
    private const string ContributorBaselinePath =
        "specs/003-ux-storyboard-accessibility/checklists/route-contributor-baseline.md";

    [Fact]
    public void Route_inventory_matches_all_and_only_the_27_canonical_routes()
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(RouteManifestPath));
        var inventory = RepositoryFiles.Read(RouteInventoryPath);
        var rows = RouteRowPattern().Matches(inventory);
        var routes = manifest.RootElement.GetProperty("routes").EnumerateArray().ToArray();

        Assert.Equal(27, routes.Length);
        Assert.Equal(27, rows.Count);
        Assert.Equal(27, rows.Select(row => row.Groups["id"].Value).Distinct().Count());

        foreach (var route in routes)
        {
            var id = route.GetProperty("id").GetString();
            var template = route.GetProperty("template").GetString();
            var page = $"{route.GetProperty("page").GetString()}.razor";
            var row = Assert.Single(rows.Cast<Match>(), candidate =>
                candidate.Groups["id"].Value == id);

            Assert.Equal(template, row.Groups["template"].Value);
            Assert.Equal(page, row.Groups["page"].Value);
        }
    }

    [Fact]
    public void Record_contract_requires_complete_ownership_design_state_and_readiness_evidence()
    {
        var contract = RepositoryFiles.Read(RecordContractPath);
        string[] requiredTerms =
        [
            "schemaVersion", "routeId", "routeTemplate", "pageName",
            "designOwnerSpec", "implementationOwnerSpec", "ownerSpecs",
            "actors", "purpose", "informationHierarchy", "responsiveWireframes",
            "components", "dataContracts", "actions", "navigationTransitions",
            "states", "responsiveWidths", "focusOrder", "testIds",
            "contributorContractVersions", "readinessState", "approvalVersion",
            "design-only", "implementation-ready", "fixtureVersion",
            "expectedContent", "expectedFocusTarget", "liveRegion", "nextActions"
        ];

        Assert.All(requiredTerms, term =>
            Assert.Contains(term, contract, StringComparison.Ordinal));
        RepositoryFiles.ContainsAll(
            contract,
            "does not authorize route source",
            "exact immutable version",
            "Ahmed ELbamby");
    }

    [Fact]
    public void Responsive_contract_covers_all_six_widths_reflow_navigation_tables_actions_and_overflow()
    {
        var contract = RepositoryFiles.Read(ResponsiveContractPath);

        RepositoryFiles.ContainsAll(
            contract,
            "320 CSS px",
            "375 CSS px",
            "768 CSS px",
            "1024 CSS px",
            "1280 CSS px",
            "1920 CSS px",
            "Navigation",
            "Table alternative",
            "Action placement",
            "Content order",
            "Overflow",
            "400% zoom");
        Assert.DoesNotContain("device detection", contract, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Traceability_contract_maps_routes_and_components_to_owners_requirements_tasks_and_test_families()
    {
        var index = RepositoryFiles.Read(DesignIndexPath);
        using var components = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/component-manifest.json"));

        Assert.Equal(
            20,
            components.RootElement.GetProperty("components").GetArrayLength());
        RepositoryFiles.ContainsAll(
            index,
            "Page Design Record",
            "owner SPEC and FR/AC",
            "implementation task",
            "component test ID",
            "contract test ID",
            "E2E test ID",
            "accessibility test ID",
            "visual test ID",
            ".specify/route-manifest.json",
            ".specify/component-manifest.json",
            "design-only");
    }

    [Fact]
    public void Contributor_baseline_keeps_all_routes_design_only_until_exact_contract_pins_exist()
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(RouteManifestPath));
        var baseline = RepositoryFiles.Read(ContributorBaselinePath);
        var rows = ContributorRowPattern().Matches(baseline);
        var routes = manifest.RootElement.GetProperty("routes").EnumerateArray().ToArray();

        Assert.Equal(27, rows.Count);
        foreach (var route in routes)
        {
            var id = route.GetProperty("id").GetString();
            var expectedImplementationOwner =
                $"SPEC-{route.GetProperty("implementationOwner").GetString()}";
            var expectedOwners = string.Join(
                ", ",
                route.GetProperty("owners").EnumerateArray()
                    .Select(owner => $"SPEC-{owner.GetString()}"));
            var row = Assert.Single(rows.Cast<Match>(), candidate =>
                candidate.Groups["id"].Value == id);

            Assert.Equal(expectedImplementationOwner, row.Groups["implementationOwner"].Value);
            Assert.Equal(expectedOwners, row.Groups["owners"].Value);
            Assert.Equal("not-pinned", row.Groups["pins"].Value);
        }
    }

    [GeneratedRegex(
        @"(?m)^\|\s*(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\s*\|\s*`(?<template>[^`]+)`\s*\|\s*`(?<page>[^`]+)`\s*\|")]
    private static partial Regex RouteRowPattern();

    [GeneratedRegex(
        @"(?m)^\|\s*(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\s*\|\s*(?<implementationOwner>SPEC-[0-9]{3})\s*\|\s*(?<owners>SPEC-[0-9]{3}(?:, SPEC-[0-9]{3})*)\s*\|\s*design-only\s*\|\s*`(?<pins>[^`]+)`\s*\|")]
    private static partial Regex ContributorRowPattern();
}
