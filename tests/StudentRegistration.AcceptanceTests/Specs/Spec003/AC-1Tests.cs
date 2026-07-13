using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_1Tests
{
    private static readonly string[] RequiredRecordFields =
    [
        "schemaVersion",
        "routeId",
        "routeTemplate",
        "pageName",
        "designOwnerSpec",
        "implementationOwnerSpec",
        "ownerSpecs",
        "actors",
        "purpose",
        "informationHierarchy",
        "responsiveWireframes",
        "components",
        "dataContracts",
        "actions",
        "navigationTransitions",
        "states",
        "responsiveWidths",
        "focusOrder",
        "testIds",
        "contributorContractVersions",
        "readinessState",
        "approvalVersion"
    ];

    [Fact]
    public void Page_design_readiness_contract_covers_every_route_without_claiming_runtime_execution()
    {
        // Given any canonical route is selected for a future readiness review.
        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/route-manifest.json"));
        var routes = manifest.RootElement.GetProperty("routes").EnumerateArray().ToArray();

        Assert.Equal(27, routes.Length);
        Assert.Equal(
            27,
            routes.Select(route => route.GetProperty("id").GetString())
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Equal(
            27,
            routes.Select(route => route.GetProperty("template").GetString())
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.All(
            routes,
            route =>
            {
                Assert.Equal("003", route.GetProperty("designOwner").GetString());
                Assert.False(
                    string.IsNullOrWhiteSpace(
                        route.GetProperty("implementationOwner").GetString()));
                Assert.NotEmpty(route.GetProperty("links").EnumerateArray());
            });

        // When the governed Page Design Record schema and trace contract are inspected.
        using var schema = JsonDocument.Parse(
            RepositoryFiles.Read(
                "specs/003-ux-storyboard-accessibility/schemas/page-design-record.schema.json"));
        var root = schema.RootElement;
        var required = root.GetProperty("required").EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(RequiredRecordFields, field => Assert.Contains(field, required));

        var properties = root.GetProperty("properties");
        Assert.Equal(
            [320, 375, 768, 1024, 1280, 1920],
            properties.GetProperty("responsiveWidths").GetProperty("const")
                .EnumerateArray()
                .Select(item => item.GetInt32())
                .ToArray());
        Assert.Equal(
            ["320", "375", "768", "1024", "1280", "1920"],
            properties.GetProperty("responsiveWireframes").GetProperty("required")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray());

        var stateNames = root.GetProperty("$defs").GetProperty("uiStateCase")
            .GetProperty("properties").GetProperty("state").GetProperty("enum")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Equal(9, stateNames.Length);
        Assert.Equal(9, stateNames.Distinct(StringComparer.Ordinal).Count());

        // Then each future approved record is required to carry complete design and trace links.
        var recordContract = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/page-design-record-contract.md");
        RepositoryFiles.ContainsAll(
            recordContract,
            "Each canonical route has one governed record",
            "the six-width `responsiveWireframes`",
            "nine `states`",
            "keyboard `focusOrder`",
            "`testIds`",
            "`approvalVersion` identifies the reviewed record version",
            "does not authorize route source");

        var traceContract = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/README.md");
        RepositoryFiles.ContainsAll(
            traceContract,
            "owner SPEC and FR/AC references",
            "single implementation task",
            "component test ID and contract test ID",
            "E2E test ID",
            "accessibility test ID and visual test ID",
            "A planned identifier",
            "not pass evidence");
    }
}
