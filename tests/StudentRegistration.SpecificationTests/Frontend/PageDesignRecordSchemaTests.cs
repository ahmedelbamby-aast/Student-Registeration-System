using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Frontend;

public sealed partial class PageDesignRecordSchemaTests
{
    private const string RouteManifestPath = ".specify/route-manifest.json";
    private const string PageApiManifestPath = ".specify/page-api-manifest.json";
    private const string EndpointManifestPath = ".specify/endpoint-manifest.json";
    private const string RouteInventoryPath =
        "specs/003-ux-storyboard-accessibility/design/route-inventory.md";
    private const string RecordContractPath =
        "specs/003-ux-storyboard-accessibility/design/page-design-record-contract.md";
    private const string ResponsiveContractPath =
        "specs/003-ux-storyboard-accessibility/design/responsive-layout-contract.md";
    private const string PageMatrixPath =
        "specs/003-ux-storyboard-accessibility/page-matrix.md";
    private const string DesignIndexPath =
        "specs/003-ux-storyboard-accessibility/design/README.md";
    private const string ContributorBaselinePath =
        "specs/003-ux-storyboard-accessibility/checklists/route-contributor-baseline.md";
    private const string PageDesignRecordSchemaPath =
        "specs/003-ux-storyboard-accessibility/schemas/page-design-record.schema.json";
    private const string StoryboardPath = "docs/STORYBOARD.md";

    private static readonly IReadOnlyDictionary<string, string> ImplementationTasks =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["AUTH-01"] = "SPEC-008/T076",
            ["AUTH-02"] = "SPEC-007/T088",
            ["AUTH-03"] = "SPEC-007/T090",
            ["AUTH-04"] = "SPEC-007/T092",
            ["AUTH-05"] = "SPEC-007/T094",
            ["STU-01"] = "SPEC-008/T078",
            ["STU-02"] = "SPEC-011/T040",
            ["STU-03"] = "SPEC-011/T042",
            ["STU-04"] = "SPEC-012/T043",
            ["STU-05"] = "SPEC-014/T105",
            ["STU-06"] = "SPEC-015/T051",
            ["STU-07"] = "SPEC-015/T053",
            ["STU-08"] = "SPEC-007/T096",
            ["ADM-01"] = "SPEC-017/T074",
            ["ADM-02"] = "SPEC-008/T080",
            ["ADM-03"] = "SPEC-007/T124",
            ["ADM-04"] = "SPEC-008/T082",
            ["ADM-05"] = "SPEC-009/T091",
            ["ADM-06"] = "SPEC-010/T099",
            ["ADM-07"] = "SPEC-010/T101",
            ["ADM-08"] = "SPEC-017/T088",
            ["ADM-09"] = "SPEC-017/T090",
            ["STF-01"] = "SPEC-016/T067",
            ["STF-02"] = "SPEC-016/T069",
            ["STF-03"] = "SPEC-016/T071",
            ["STF-04"] = "SPEC-016/T073",
            ["SYS-01"] = "SPEC-003/T258"
        };

    private static readonly IReadOnlyDictionary<string, string> DesignTasks =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["AUTH-01"] = "SPEC-003/T122",
            ["AUTH-02"] = "SPEC-003/T127",
            ["AUTH-03"] = "SPEC-003/T132",
            ["AUTH-04"] = "SPEC-003/T137",
            ["AUTH-05"] = "SPEC-003/T142",
            ["STU-01"] = "SPEC-003/T147",
            ["STU-02"] = "SPEC-003/T152",
            ["STU-03"] = "SPEC-003/T157",
            ["STU-04"] = "SPEC-003/T162",
            ["STU-05"] = "SPEC-003/T167",
            ["STU-06"] = "SPEC-003/T172",
            ["STU-07"] = "SPEC-003/T177",
            ["STU-08"] = "SPEC-003/T182",
            ["ADM-01"] = "SPEC-003/T187",
            ["ADM-02"] = "SPEC-003/T192",
            ["ADM-03"] = "SPEC-003/T197",
            ["ADM-04"] = "SPEC-003/T202",
            ["ADM-05"] = "SPEC-003/T207",
            ["ADM-06"] = "SPEC-003/T212",
            ["ADM-07"] = "SPEC-003/T217",
            ["ADM-08"] = "SPEC-003/T222",
            ["ADM-09"] = "SPEC-003/T227",
            ["STF-01"] = "SPEC-003/T232",
            ["STF-02"] = "SPEC-003/T237",
            ["STF-03"] = "SPEC-003/T242",
            ["STF-04"] = "SPEC-003/T247",
            ["SYS-01"] = "SPEC-003/T252"
        };

    [Fact]
    public void Route_inventory_matches_all_and_only_the_30_canonical_routes()
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(RouteManifestPath));
        var inventory = RepositoryFiles.Read(RouteInventoryPath);
        var rows = RouteRowPattern().Matches(inventory);
        var pageMatrixRows = PageContributorRowPattern().Matches(
            RepositoryFiles.Read(PageMatrixPath));
        var routes = manifest.RootElement.GetProperty("routes").EnumerateArray().ToArray();

        Assert.Equal("3.0.0", manifest.RootElement.GetProperty("version").GetString());
        RepositoryFiles.ContainsAll(
            inventory,
            "route-inventory/2.0",
            "route-manifest.json` version `3.0.0",
            "Approved by Ahmed ELbamby on 2026-07-20",
            "exactly 30",
            "implementation ownership.");
        Assert.Equal(30, routes.Length);
        Assert.Equal(30, rows.Count);
        Assert.Equal(30, pageMatrixRows.Count);
        Assert.Equal(30, rows.Select(row => row.Groups["id"].Value).Distinct().Count());

        foreach (var route in routes)
        {
            var id = route.GetProperty("id").GetString();
            var template = route.GetProperty("template").GetString();
            var page = $"{route.GetProperty("page").GetString()}.razor";
            var row = Assert.Single(rows.Cast<Match>(), candidate =>
                candidate.Groups["id"].Value == id);

            Assert.Equal(template, row.Groups["template"].Value);
            Assert.Equal(page, row.Groups["page"].Value);
            Assert.Equal(
                $"SPEC-{route.GetProperty("implementationOwner").GetString()}",
                row.Groups["implementationOwner"].Value);
            var expectedOwners = string.Join(
                ", ",
                route.GetProperty("owners").EnumerateArray()
                    .Select(owner => $"SPEC-{owner.GetString()}"));
            Assert.Equal(expectedOwners, row.Groups["owners"].Value);
            Assert.Equal("design-only", row.Groups["readiness"].Value);

            var pageMatrixRow = Assert.Single(
                pageMatrixRows.Cast<Match>(),
                candidate => candidate.Groups["id"].Value == id);
            var expectedContributors = string.Join(
                "/",
                route.GetProperty("owners").EnumerateArray()
                    .Select(owner => owner.GetString()!)
                    .Where(owner => owner != route.GetProperty("designOwner").GetString())
                    .Select((owner, index) => index == 0 ? $"SPEC-{owner}" : owner));
            Assert.Equal(expectedContributors, pageMatrixRow.Groups["contributors"].Value);
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
            "page-design-record/1.1",
            "Approved by Ahmed ELbamby on 2026-07-13",
            "does not authorize route source",
            "exact immutable version",
            "required and forbidden content/actions",
            "primary or failure E2E ID",
            "exactly one",
            "Trigger: initial navigation",
            "Trigger: user action",
            "Trigger: submitted form failure",
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
            28,
            components.RootElement.GetProperty("components").GetArrayLength());
        RepositoryFiles.ContainsAll(
            index,
            "frontend-design-index/2.0",
            "Approved by Ahmed ELbamby on 2026-07-20",
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
        RepositoryFiles.ContainsAll(
            baseline,
            "route-contributors/1.1",
            "Owner-reconciliation amendment:** Approved by Ahmed ELbamby on 2026-07-13",
            "does not promote a route or",
            "change an implementation owner.");
        var rows = ContributorRowPattern().Matches(baseline);
        var routes = manifest.RootElement.GetProperty("routes").EnumerateArray().ToArray();

        Assert.Equal(30, rows.Count);
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

    [Fact]
    public void Route_owners_and_links_cover_every_consumed_endpoint_authority_and_composite_contributor()
    {
        using var routeManifest = JsonDocument.Parse(RepositoryFiles.Read(RouteManifestPath));
        using var pageApiManifest = JsonDocument.Parse(RepositoryFiles.Read(PageApiManifestPath));
        using var endpointManifest = JsonDocument.Parse(RepositoryFiles.Read(EndpointManifestPath));
        Assert.Equal(
            "2.0.0",
            pageApiManifest.RootElement.GetProperty("version").GetString());
        Assert.Equal(
            "2.1.0",
            endpointManifest.RootElement.GetProperty("version").GetString());
        var endpointAuthorities = endpointManifest.RootElement.GetProperty("endpoints")
            .EnumerateArray()
            .ToDictionary(
                endpoint =>
                    $"{endpoint.GetProperty("method").GetString()} {endpoint.GetProperty("path").GetString()}",
                endpoint =>
                {
                    var owner = endpoint.GetProperty("owner").GetString()!;
                    return endpoint.TryGetProperty("contributors", out var contributors)
                        ? new[] { owner }
                            .Concat(contributors.EnumerateArray()
                                .Select(contributor => contributor.GetString()!))
                            .ToArray()
                        : [owner];
                },
                StringComparer.Ordinal);

        foreach (var route in routeManifest.RootElement.GetProperty("routes").EnumerateArray())
        {
            var routeId = route.GetProperty("id").GetString()!;
            var designOwner = route.GetProperty("designOwner").GetString()!;
            var routeOwners = route.GetProperty("owners").EnumerateArray()
                .Select(owner => owner.GetString()!)
                .ToHashSet(StringComparer.Ordinal);
            var linkedSpecs = route.GetProperty("links").EnumerateArray()
                .Select(link => link.GetProperty("spec").GetString()!)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var endpoint in pageApiManifest.RootElement.GetProperty("pages")
                         .GetProperty(routeId).EnumerateArray())
            {
                var endpointKey = endpoint.GetString()!;
                Assert.True(
                    endpointAuthorities.TryGetValue(endpointKey, out var endpointContributors),
                    $"{routeId} consumes unknown endpoint {endpointKey}.");
                Assert.All(
                    endpointContributors!,
                    contributor => Assert.Contains(contributor, routeOwners));
            }

            Assert.Equal(
                routeOwners.Where(owner => owner != designOwner).Order(StringComparer.Ordinal),
                linkedSpecs.Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void All_approved_route_design_records_match_governed_manifests_and_remain_design_only()
    {
        using var routeManifest = JsonDocument.Parse(RepositoryFiles.Read(RouteManifestPath));
        using var apiManifest = JsonDocument.Parse(RepositoryFiles.Read(PageApiManifestPath));
        using var componentManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/component-manifest.json"));
        using var recordSchema = JsonDocument.Parse(
            RepositoryFiles.Read(PageDesignRecordSchemaPath));
        var requiredRecordProperties = recordSchema.RootElement.GetProperty("required")
            .EnumerateArray()
            .Select(property => property.GetString()!)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var requiredStateProperties = recordSchema.RootElement.GetProperty("$defs")
            .GetProperty("uiStateCase")
            .GetProperty("required")
            .EnumerateArray()
            .Select(property => property.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        var allowedStateProperties = recordSchema.RootElement.GetProperty("$defs")
            .GetProperty("uiStateCase")
            .GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        var minimumJourneys = PageJourneyRowPattern().Matches(
                RepositoryFiles.Read(PageMatrixPath))
            .Cast<Match>()
            .ToDictionary(
                row => row.Groups["id"].Value,
                row => row.Groups["journeys"].Value.Split(
                    ',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                StringComparer.Ordinal);
        var knownComponents = componentManifest.RootElement.GetProperty("components")
            .EnumerateArray()
            .Select(component => component.GetProperty("name").GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        var expectedStates = new HashSet<string>(
            [
                "loading",
                "empty",
                "success",
                "validation-error",
                "service-error",
                "unauthorized",
                "session-expired",
                "stale",
                "offline"
            ],
            StringComparer.Ordinal);
        var expectedLiveRegions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["loading"] = "polite",
            ["success"] = "polite",
            ["service-error"] = "polite",
            ["unauthorized"] = "assertive",
            ["session-expired"] = "assertive",
            ["stale"] = "polite",
            ["offline"] = "polite"
        };
        var triggerLiveRegions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Trigger: initial navigation"] = "none",
            ["Trigger: user action"] = "polite",
            ["Trigger: submitted form failure"] = "assertive"
        };
        int[] expectedWidths = [320, 375, 768, 1024, 1280, 1920];

        foreach (var route in routeManifest.RootElement.GetProperty("routes").EnumerateArray())
        {
            var routeId = route.GetProperty("id").GetString()!;
            var draft = RepositoryFiles.Read(
                $"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");
            if (routeId is "STU-09" or "ADM-10" or "STF-05")
            {
                RepositoryFiles.ContainsAll(
                    draft,
                    $"# {routeId} Page Design Record",
                    "**Record version:** `2.0`",
                    "**Approval status:** Approved by Ahmed ELbamby on 2026-07-20",
                    "**Readiness:** `design-only`",
                    "frontend-fixture/2.0");
                var modernMatch = Assert.Single(JsonRecordPattern().Matches(draft).Cast<Match>());
                using var modernDocument = JsonDocument.Parse(modernMatch.Groups["json"].Value);
                var modernRecord = modernDocument.RootElement;
                Assert.Equal(routeId, modernRecord.GetProperty("routeId").GetString());
                Assert.Equal("2.0", modernRecord.GetProperty("approvalVersion").GetString());
                Assert.Equal("design-only", modernRecord.GetProperty("readinessState").GetString());
                Assert.Equal(expectedWidths, modernRecord.GetProperty("responsiveWidths").EnumerateArray().Select(width => width.GetInt32()));
                Assert.Equal(expectedStates, modernRecord.GetProperty("states").EnumerateArray().Select(state => state.GetProperty("state").GetString()!).ToHashSet(StringComparer.Ordinal));
                Assert.All(ReadStrings(modernRecord.GetProperty("components")), component => Assert.Contains(component, knownComponents));
                Assert.Equal(
                    "frontend-design-index/2.0",
                    modernRecord.GetProperty("contributorContractVersions").GetProperty("SPEC-003").GetString());
                continue;
            }
            RepositoryFiles.ContainsAll(
                draft,
                $"# {routeId} Page Design Record",
                "**Record version:** `1.0`",
                "**Approval status:** Approved by Ahmed ELbamby on 2026-07-13",
                "**Readiness:** `design-only`",
                $"**Design task:** {DesignTasks[routeId]}",
                $"**Implementation task:** {ImplementationTasks[routeId]}",
                "## Annotated responsive layouts",
                "## Minimum journey and test traceability",
                "page-design-record/1.1",
                "Forbidden content or action",
                "Component test",
                "E2E test",
                "does not authorize route source");
            Assert.DoesNotContain("Route purpose content", draft, StringComparison.Ordinal);
            Assert.DoesNotContain("Authoritative current state", draft, StringComparison.Ordinal);
            if (routeId == "STU-04")
            {
                RepositoryFiles.ContainsAll(
                    RepositoryFiles.Read(
                        "specs/012-schedule-builder-conflicts/contracts/api.md"),
                    "defaultTargetCredits: 18",
                    "maximumAllowedCredits: 18",
                    "loadReasons: LoadPolicyReasonDto[]",
                    "policyVersion",
                    "sourceReference");
                Assert.All(
                    new[]
                    {
                        "default target 18",
                        "effective maximum 12",
                        "GPA-below-2.0",
                        "policy/source",
                        "Red X",
                        "Conflict",
                        "subjects/groups",
                        "overlap interval",
                        "ranked alternatives",
                        "manual resolution",
                        "disabled Continue"
                    },
                    requirement => Assert.Contains(
                        requirement,
                        draft,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (routeId is "STU-02" or "STU-03")
            {
                RepositoryFiles.ContainsAll(
                    RepositoryFiles.Read(
                        "specs/011-eligibility-subject-discovery/contracts/api.md"),
                    "currentPlanCredits",
                    "projectedPlanCredits",
                    "defaultTargetCredits: 18",
                    "maximumAllowedCredits: 12 | 18",
                    "groups: GroupSummaryDto[]");
                Assert.All(
                    new[]
                    {
                        "current/projected",
                        "default target 18",
                        "effective maximum 12",
                        "GPA-below-2.0",
                        "policy/source",
                        "capacity",
                        "Lecturer",
                        "TA",
                        "location",
                        "day",
                        "start/end"
                    },
                    requirement => Assert.Contains(
                        requirement,
                        draft,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (routeId == "STU-03")
            {
                Assert.Contains(
                    "incomplete staffing bundle",
                    draft,
                    StringComparison.OrdinalIgnoreCase);
            }
            else if (routeId is "AUTH-02" or "AUTH-04")
            {
                Assert.Contains("no second factor", draft, StringComparison.OrdinalIgnoreCase);
            }
            else if (routeId == "AUTH-03")
            {
                Assert.Contains(
                    "first-use activation credential",
                    draft,
                    StringComparison.OrdinalIgnoreCase);
                Assert.Contains("not a second factor", draft, StringComparison.OrdinalIgnoreCase);
            }
            var journeyTraceRows = JourneyTraceRowPattern().Matches(draft)
                .Cast<Match>()
                .Where(row => row.Groups["id"].Value == routeId)
                .ToArray();
            Assert.NotEmpty(journeyTraceRows);
            Assert.Equal(
                journeyTraceRows.Length,
                journeyTraceRows.Select(row => row.Groups["journey"].Value.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count());
            Assert.Equal(minimumJourneys[routeId].Length, journeyTraceRows.Length);
            Assert.All(
                minimumJourneys[routeId],
                journey => Assert.Single(
                    journeyTraceRows,
                    row => string.Equals(
                        row.Groups["journey"].Value.Trim(),
                        journey,
                        StringComparison.OrdinalIgnoreCase)));
            Assert.All(
                journeyTraceRows,
                row =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(row.Groups["required"].Value));
                    Assert.False(string.IsNullOrWhiteSpace(row.Groups["forbidden"].Value));
                    Assert.False(string.IsNullOrWhiteSpace(row.Groups["next"].Value));
                    Assert.StartsWith($"{routeId}-COMP-", row.Groups["component"].Value);
                    Assert.Matches(
                        $"^{Regex.Escape(routeId)}-E2E-(?:PRIMARY|FAILURE)$",
                        row.Groups["e2e"].Value);
                });

            if (routeId == "STF-02")
            {
                var noAssignments = Assert.Single(
                    journeyTraceRows,
                    row => string.Equals(
                        row.Groups["journey"].Value.Trim(),
                        "no assignments",
                        StringComparison.OrdinalIgnoreCase));
                Assert.Equal("empty", noAssignments.Groups["state"].Value);
                Assert.DoesNotContain("unassigned 403", draft, StringComparison.OrdinalIgnoreCase);
            }

            if (routeId == "ADM-08")
            {
                var collision = Assert.Single(
                    journeyTraceRows,
                    row => string.Equals(
                        row.Groups["journey"].Value.Trim(),
                        "collision",
                        StringComparison.OrdinalIgnoreCase));
                Assert.Equal("success", collision.Groups["state"].Value);
            }

            if (routeId is "ADM-04" or "ADM-08")
            {
                var noResults = Assert.Single(
                    journeyTraceRows,
                    row => string.Equals(
                        row.Groups["journey"].Value.Trim(),
                        "no results",
                        StringComparison.OrdinalIgnoreCase));
                Assert.Equal("empty", noResults.Groups["state"].Value);
            }

            if (routeId == "SYS-01")
            {
                foreach (var (journey, expectedState) in new[]
                         {
                             ("healthy", "success"),
                             ("degraded", "service-error"),
                             ("unhealthy", "service-error")
                         })
                {
                    var healthJourney = Assert.Single(
                        journeyTraceRows,
                        row => string.Equals(
                            row.Groups["journey"].Value.Trim(),
                            journey,
                            StringComparison.OrdinalIgnoreCase));
                    Assert.Equal(expectedState, healthJourney.Groups["state"].Value);
                }

                RepositoryFiles.ContainsAll(
                    draft,
                    "HealthSummary status healthy",
                    "HealthSummary status degraded or unhealthy",
                    "Open public gateway",
                    "Public gateway -> AUTH-01");
                RepositoryFiles.ContainsAll(
                    RepositoryFiles.Read(
                        "specs/018-quality-security-scalability-operations/contracts/api.md"),
                    "\"healthy\" | \"degraded\" | \"unhealthy\"");

                var storyboardRow = Assert.Single(
                    RepositoryFiles.Read(StoryboardPath)
                        .Split('\n', StringSplitOptions.TrimEntries),
                    line => line.StartsWith("| SYS-01 /status/{code}", StringComparison.Ordinal));
                RepositoryFiles.ContainsAll(
                    storyboardRow,
                    "healthy",
                    "degraded",
                    "unhealthy",
                    "never a raw stack trace");
            }

            var jsonMatch = JsonRecordPattern().Match(draft);
            Assert.True(jsonMatch.Success, $"{routeId} must contain one fenced JSON record.");
            using var recordDocument = JsonDocument.Parse(jsonMatch.Groups["json"].Value);
            var record = recordDocument.RootElement;
            Assert.Equal(
                requiredRecordProperties,
                record.EnumerateObject()
                    .Select(property => property.Name)
                    .Order(StringComparer.Ordinal));

            Assert.Equal("1.0", record.GetProperty("schemaVersion").GetString());
            Assert.Equal(routeId, record.GetProperty("routeId").GetString());
            Assert.Equal(
                route.GetProperty("template").GetString(),
                record.GetProperty("routeTemplate").GetString());
            Assert.Equal(
                $"{route.GetProperty("page").GetString()}.razor",
                record.GetProperty("pageName").GetString());
            Assert.Equal("SPEC-003", record.GetProperty("designOwnerSpec").GetString());
            Assert.Equal(
                $"SPEC-{route.GetProperty("implementationOwner").GetString()}",
                record.GetProperty("implementationOwnerSpec").GetString());

            var expectedOwners = route.GetProperty("owners").EnumerateArray()
                .Select(owner => $"SPEC-{owner.GetString()}")
                .ToArray();
            Assert.Equal(
                expectedOwners,
                ReadStrings(record.GetProperty("ownerSpecs")));

            AssertNonemptyStrings(record, "actors");
            Assert.False(string.IsNullOrWhiteSpace(record.GetProperty("purpose").GetString()));
            AssertNonemptyStrings(record, "informationHierarchy");
            AssertNonemptyStrings(record, "components");
            var recordComponents = ReadStrings(record.GetProperty("components"));
            Assert.All(
                recordComponents,
                component => Assert.Contains(
                    component switch
                    {
                        "Button" => "AppButton",
                        "StatePanel" => "RouteStatePanel",
                        "ValidationSummary" => "AccessibleValidationSummary",
                        _ => component
                    },
                    knownComponents));
            AssertNonemptyStrings(record, "actions");
            var actions = ReadStrings(record.GetProperty("actions"));
            AssertNonemptyStrings(record, "navigationTransitions");
            AssertNonemptyStrings(record, "focusOrder");
            Assert.DoesNotContain(
                ReadStrings(record.GetProperty("focusOrder")),
                item => item.Contains("when present", StringComparison.OrdinalIgnoreCase)
                    || item.Contains(
                        "information-hierarchy order",
                        StringComparison.OrdinalIgnoreCase));

            if (routeId == "ADM-07")
            {
                Assert.DoesNotContain(
                    actions,
                    action => action.Contains("Import availability", StringComparison.OrdinalIgnoreCase));
            }

            if (routeId == "ADM-02")
            {
                Assert.DoesNotContain(
                    actions,
                    action => string.Equals(
                        action,
                        "Validate context",
                        StringComparison.OrdinalIgnoreCase));
            }

            if (routeId == "ADM-03")
            {
                Assert.DoesNotContain("Error export", draft, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(
                    "downloadable row-error",
                    draft,
                    StringComparison.OrdinalIgnoreCase);
            }

            if (routeId == "STF-01")
            {
                Assert.Contains(
                    actions,
                    action => action.Contains("Switch", StringComparison.OrdinalIgnoreCase)
                        && action.Contains("context", StringComparison.OrdinalIgnoreCase));
            }

            if (routeId == "STU-03")
            {
                Assert.Contains(
                    ReadStrings(record.GetProperty("navigationTransitions")),
                    transition => transition.Contains("STU-04", StringComparison.Ordinal)
                        && transition.Contains("intent", StringComparison.OrdinalIgnoreCase));
            }

            if (routeId == "STU-06")
            {
                Assert.Contains("Button", recordComponents);
            }

            if (routeId is "STU-07" or "STF-02")
            {
                Assert.Contains("FormField", recordComponents);
            }

            if (routeId == "STU-07")
            {
                Assert.Contains("Button", recordComponents);
            }

            var expectedApis = apiManifest.RootElement.GetProperty("pages")
                .GetProperty(routeId)
                .EnumerateArray()
                .Select(endpoint => endpoint.GetString()!)
                .ToArray();
            Assert.Equal(expectedApis, ReadStrings(record.GetProperty("dataContracts")));
            var hasCommandContract = expectedApis.Any(
                endpoint => !endpoint.StartsWith("GET ", StringComparison.Ordinal));

            Assert.Equal(
                expectedWidths,
                record.GetProperty("responsiveWidths").EnumerateArray()
                    .Select(width => width.GetInt32())
                    .ToArray());
            var wireframes = record.GetProperty("responsiveWireframes");
            var wireframeDescriptions = expectedWidths
                .Select(width => wireframes.GetProperty(width.ToString()).GetString()!)
                .ToArray();
            Assert.All(
                expectedWidths,
                width =>
                {
                    var description = wireframes.GetProperty(width.ToString()).GetString();
                    Assert.False(string.IsNullOrWhiteSpace(description));
                });
            Assert.Equal(
                expectedWidths.Length,
                wireframeDescriptions.Distinct(StringComparer.Ordinal).Count());
            Assert.All(
                wireframeDescriptions,
                description =>
                {
                    Assert.DoesNotContain(
                        "Navigation is collapsed when present",
                        description,
                        StringComparison.Ordinal);
                    Assert.DoesNotContain(
                        "optional context sidebar",
                        description,
                        StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain(
                        "Dense data may use the semantic table",
                        description,
                        StringComparison.Ordinal);
                });

            var states = record.GetProperty("states").EnumerateArray().ToArray();
            Assert.Equal(9, states.Length);
            Assert.Equal(
                states.Length,
                states.Select(state => state.GetProperty("fixture").GetString())
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                expectedStates,
                states.Select(state => state.GetProperty("state").GetString()!)
                    .ToHashSet(StringComparer.Ordinal));
            foreach (var state in states)
            {
                var stateProperties = state.EnumerateObject()
                    .Select(property => property.Name)
                    .ToHashSet(StringComparer.Ordinal);
                Assert.All(requiredStateProperties, property => Assert.Contains(property, stateProperties));
                Assert.All(stateProperties, property => Assert.Contains(property, allowedStateProperties));
                var stateName = state.GetProperty("state").GetString()!;
                var applicability = state.GetProperty("applicability").GetString();
                Assert.Contains(applicability, new[] { "required", "not-applicable" });
                if (applicability == "not-applicable")
                {
                    Assert.False(string.IsNullOrWhiteSpace(state.GetProperty("reason").GetString()));
                    Assert.DoesNotContain(
                        ReadStrings(state.GetProperty("expectedContent")),
                        content => content.StartsWith("Trigger:", StringComparison.Ordinal));
                }

                Assert.False(string.IsNullOrWhiteSpace(state.GetProperty("fixture").GetString()));
                Assert.Equal("frontend-fixture/1.0", state.GetProperty("fixtureVersion").GetString());
                AssertNonemptyStrings(state, "expectedContent");
                Assert.False(
                    string.IsNullOrWhiteSpace(state.GetProperty("expectedFocusTarget").GetString()));
                Assert.Contains(
                    state.GetProperty("liveRegion").GetString(),
                    new[] { "none", "polite", "assertive" });
                Assert.True(state.TryGetProperty("nextActions", out _));
                var nextActions = ReadStrings(state.GetProperty("nextActions"));
                if (stateName == "empty" && routeId == "ADM-02")
                {
                    Assert.Contains("Create term", nextActions);
                    Assert.DoesNotContain("Retry or change criteria", nextActions);
                }

                if (stateName == "empty" && routeId == "STF-04")
                {
                    Assert.Contains("Add availability range", nextActions);
                    Assert.DoesNotContain("Retry or change criteria", nextActions);
                }

                if (routeId == "SYS-01" && stateName == "success")
                {
                    Assert.Equal("required", applicability);
                }

                if (routeId == "ADM-08" && stateName == "validation-error")
                {
                    Assert.Equal("not-applicable", applicability);
                }

                var stateJourneyRows = journeyTraceRows
                    .Where(row => row.Groups["state"].Value == stateName)
                    .ToArray();
                if (applicability == "required" && stateJourneyRows.Length == 1)
                {
                    var journeyNextAction = stateJourneyRows[0].Groups["next"].Value.Trim();
                    Assert.Contains(
                        nextActions,
                        action => string.Equals(
                            action,
                            journeyNextAction,
                            StringComparison.OrdinalIgnoreCase));
                }
                AssertNonemptyStrings(state, "testIds");
                var stateTestIds = ReadStrings(state.GetProperty("testIds"));
                Assert.All(
                    stateTestIds,
                    id => Assert.Matches(
                        $"^{Regex.Escape(routeId)}-(?:COMP|CONTRACT|E2E|A11Y|VIS)-[A-Z0-9-]+$",
                        id));

                if (applicability == "required")
                {
                    var expectedContent = string.Join(
                        " ",
                        ReadStrings(state.GetProperty("expectedContent")));
                    var expectedLiveRegion = expectedLiveRegions.GetValueOrDefault(stateName);
                    if (stateName is "empty" or "validation-error")
                    {
                        var fixtureTriggers = triggerLiveRegions.Keys
                            .Where(trigger => expectedContent.Contains(
                                trigger,
                                StringComparison.Ordinal))
                            .ToArray();
                        var fixtureTrigger = Assert.Single(fixtureTriggers);
                        expectedLiveRegion = triggerLiveRegions[fixtureTrigger];
                        if (fixtureTrigger == "Trigger: user action")
                        {
                            var triggerFocusTarget = state.GetProperty("expectedFocusTarget").GetString()!;
                            Assert.True(
                                triggerFocusTarget.Contains("preserve", StringComparison.OrdinalIgnoreCase)
                                || triggerFocusTarget.Contains(
                                    "keep current focus",
                                    StringComparison.OrdinalIgnoreCase),
                                $"{routeId} user-triggered {stateName} must keep current focus.");
                        }
                        else if (fixtureTrigger == "Trigger: submitted form failure")
                        {
                            Assert.Contains(
                                "Validation summary",
                                state.GetProperty("expectedFocusTarget").GetString(),
                                StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    Assert.Equal(expectedLiveRegion, state.GetProperty("liveRegion").GetString());
                    if (stateName == "success")
                    {
                        if (hasCommandContract)
                        {
                            Assert.Contains("serverAccepted", expectedContent, StringComparison.Ordinal);
                        }
                        else
                        {
                            Assert.DoesNotContain(
                                "serverAccepted",
                                expectedContent,
                                StringComparison.Ordinal);
                        }
                    }
                    else if (stateName == "validation-error"
                        || stateName == "stale" && hasCommandContract)
                    {
                        Assert.Contains("reason code exactly", expectedContent, StringComparison.Ordinal);
                    }
                    else if (stateName == "service-error")
                    {
                        Assert.Contains("unknown-code safe fallback", expectedContent, StringComparison.Ordinal);
                    }
                    else if (stateName == "unauthorized")
                    {
                        Assert.Contains("UNAUTHORIZED or FORBIDDEN", expectedContent, StringComparison.Ordinal);
                    }
                    else if (stateName == "session-expired")
                    {
                        Assert.Contains("SESSION_EXPIRED", expectedContent, StringComparison.Ordinal);
                    }
                    else if (stateName == "offline")
                    {
                        Assert.Contains("never claim queued success", expectedContent, StringComparison.Ordinal);
                    }

                    var focusTarget = state.GetProperty("expectedFocusTarget").GetString()!;
                    if (stateName is "loading" or "stale" or "offline")
                    {
                        Assert.True(
                            focusTarget.Contains("preserve", StringComparison.OrdinalIgnoreCase)
                            || focusTarget.Contains(
                                "keep current focus",
                                StringComparison.OrdinalIgnoreCase),
                            $"{routeId} {stateName} must keep current focus.");
                    }
                    else if (stateName == "service-error")
                    {
                        Assert.Contains("background", focusTarget, StringComparison.OrdinalIgnoreCase);
                        Assert.True(
                            focusTarget.Contains("navigation", StringComparison.OrdinalIgnoreCase)
                            || focusTarget.Contains("route load", StringComparison.OrdinalIgnoreCase)
                            || focusTarget.Contains("initial load", StringComparison.OrdinalIgnoreCase),
                            $"{routeId} service-error must distinguish background failure from route-load failure.");
                    }
                }
            }

            var testIds = ReadStrings(record.GetProperty("testIds"));
            Assert.All(
                testIds,
                id => Assert.Matches(
                    $"^{Regex.Escape(routeId)}-(?:COMP|CONTRACT|E2E|A11Y|VIS)-[A-Z0-9-]+$",
                    id));
            Assert.Contains(testIds, id => id.StartsWith($"{routeId}-CONTRACT-", StringComparison.Ordinal));
            Assert.Contains(testIds, id => id.StartsWith($"{routeId}-COMP-", StringComparison.Ordinal));
            Assert.Contains($"{routeId}-E2E-PRIMARY", testIds);
            Assert.Contains($"{routeId}-E2E-FAILURE", testIds);
            Assert.Contains(testIds, id => id.StartsWith($"{routeId}-A11Y-", StringComparison.Ordinal));
            Assert.Contains(testIds, id => id.StartsWith($"{routeId}-VIS-", StringComparison.Ordinal));

            var contributorVersions = record.GetProperty("contributorContractVersions");
            Assert.Equal(
                expectedOwners.Order(StringComparer.Ordinal),
                contributorVersions.EnumerateObject()
                    .Select(property => property.Name)
                    .Order(StringComparer.Ordinal));
            foreach (var owner in expectedOwners)
            {
                Assert.True(contributorVersions.TryGetProperty(owner, out var version));
                Assert.Equal(
                    owner == "SPEC-003" ? "frontend-design-index/1.1" : "not-pinned",
                    version.GetString());
            }

            Assert.Equal("design-only", record.GetProperty("readinessState").GetString());
            Assert.Equal("1.0", record.GetProperty("approvalVersion").GetString());
        }
    }

    private static string[] ReadStrings(JsonElement element) =>
        element.EnumerateArray().Select(item => item.GetString()!).ToArray();

    private static void AssertNonemptyStrings(JsonElement owner, string propertyName)
    {
        var values = ReadStrings(owner.GetProperty(propertyName));
        Assert.NotEmpty(values);
        Assert.Equal(values.Length, values.Distinct(StringComparer.Ordinal).Count());
        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
    }

    [GeneratedRegex(
        @"(?m)^\|\s*(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\s*\|\s*`(?<template>[^`]+)`\s*\|\s*`(?<page>[^`]+)`\s*\|\s*(?<implementationOwner>SPEC-[0-9]{3})\s*\|\s*(?<owners>SPEC-[0-9]{3}(?:, SPEC-[0-9]{3})*)\s*\|\s*(?<readiness>design-only|implementation-ready)\s*\|")]
    private static partial Regex RouteRowPattern();

    [GeneratedRegex(
        @"(?m)^\|\s*(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\s*\|\s*[^|]+\|\s*(?<contributors>SPEC-[0-9]{3}(?:/[0-9]{3})*)\s*\|")]
    private static partial Regex PageContributorRowPattern();

    [GeneratedRegex(
        @"(?m)^\|\s*(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\s*\|\s*[^|]+\|\s*[^|]+\|\s*[^|]+\|\s*(?<journeys>[^|]+)\|")]
    private static partial Regex PageJourneyRowPattern();

    [GeneratedRegex(
        @"(?m)^\|\s*`(?<fixture>(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})-[^`]+-v[0-9]+)`\s*\((?<journey>[^)]+)\)\s*\|\s*(?<state>loading|empty|success|validation-error|service-error|unauthorized|session-expired|stale|offline)\s*\|\s*(?<required>[^|]+)\|\s*(?<forbidden>[^|]+)\|\s*(?<next>[^|]+)\|\s*`(?<component>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2}-COMP-[^`]+)`\s*\|\s*`(?<e2e>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2}-E2E-(?:PRIMARY|FAILURE))`\s*\|$")]
    private static partial Regex JourneyTraceRowPattern();

    [GeneratedRegex(
        @"(?m)^\|\s*(?<id>(?:AUTH|STU|ADM|STF|SYS)-[0-9]{2})\s*\|\s*(?<implementationOwner>SPEC-[0-9]{3})\s*\|\s*(?<owners>SPEC-[0-9]{3}(?:, SPEC-[0-9]{3})*)\s*\|\s*design-only\s*\|\s*`(?<pins>[^`]+)`\s*\|")]
    private static partial Regex ContributorRowPattern();

    [GeneratedRegex("```json\\s*(?<json>\\{.*?\\})\\s*```", RegexOptions.Singleline)]
    private static partial Regex JsonRecordPattern();
}
