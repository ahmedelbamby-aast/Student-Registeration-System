using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec003;

public sealed class FrontendTestRecordSchemaTests
{
    private const string SchemaPath =
        "specs/003-ux-storyboard-accessibility/schemas/frontend-test-record.schema.json";

    [Fact]
    public void Schema_is_closed_and_requires_deterministic_traceable_test_metadata()
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
            "testId", "routeId", "requirementIds", "type", "fixture",
            "fixtureVersion", "expectedOutcome"
        ];
        Assert.All(expectedRequired, field => Assert.Contains(field, required));

        var testTypes = root.GetProperty("properties")
            .GetProperty("type")
            .GetProperty("enum")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(
            ["accessibility", "component", "contract", "e2e", "visual"],
            testTypes.Order(StringComparer.Ordinal).ToArray());
    }
}
