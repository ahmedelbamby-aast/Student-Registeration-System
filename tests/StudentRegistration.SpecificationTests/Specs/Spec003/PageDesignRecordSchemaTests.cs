using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec003;

public sealed class PageDesignRecordSchemaTests
{
    private const string SchemaPath =
        "specs/003-ux-storyboard-accessibility/schemas/page-design-record.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_and_requires_the_complete_design_record()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;

        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            root.GetProperty("$schema").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());

        var required = root.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToHashSet(StringComparer.Ordinal);
        string[] expectedRequired =
        [
            "schemaVersion", "routeId", "routeTemplate", "pageName",
            "designOwnerSpec", "implementationOwnerSpec", "ownerSpecs", "actors",
            "purpose", "informationHierarchy", "responsiveWireframes", "components",
            "dataContracts", "actions", "navigationTransitions", "states",
            "responsiveWidths", "focusOrder", "testIds", "contributorContractVersions",
            "readinessState", "approvalVersion"
        ];

        Assert.All(expectedRequired, field => Assert.Contains(field, required));
        Assert.Equal(
            [320, 375, 768, 1024, 1280, 1920],
            root.GetProperty("properties")
                .GetProperty("responsiveWidths")
                .GetProperty("const")
                .EnumerateArray()
                .Select(item => item.GetInt32()));
    }

    [Fact]
    public void State_cases_require_deterministic_content_focus_action_and_test_evidence()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var stateDefinition = schema.RootElement
            .GetProperty("$defs")
            .GetProperty("uiStateCase");
        var required = stateDefinition.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToHashSet(StringComparer.Ordinal);

        Assert.False(stateDefinition.GetProperty("additionalProperties").GetBoolean());
        string[] expectedRequired =
        [
            "state", "applicability", "fixture", "fixtureVersion", "expectedContent",
            "expectedFocusTarget", "liveRegion", "nextActions", "testIds"
        ];
        Assert.All(expectedRequired, field => Assert.Contains(field, required));
        Assert.Equal(
            9,
            schema.RootElement.GetProperty("properties")
                .GetProperty("states")
                .GetProperty("minItems")
                .GetInt32());
    }
}
