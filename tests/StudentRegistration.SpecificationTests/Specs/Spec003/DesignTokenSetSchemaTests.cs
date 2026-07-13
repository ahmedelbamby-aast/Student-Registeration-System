using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec003;

public sealed class DesignTokenSetSchemaTests
{
    private const string SchemaPath =
        "specs/003-ux-storyboard-accessibility/schemas/design-token-set.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_approved_and_neutral_only()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;
        var properties = root.GetProperty("properties");

        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            root.GetProperty("$schema").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal("neutral", properties.GetProperty("scope").GetProperty("const").GetString());
        Assert.False(
            properties.GetProperty("brandValuesDerivedFromLogo")
                .GetProperty("const")
                .GetBoolean());
        Assert.Equal(
            "Ahmed ELbamby",
            properties.GetProperty("approval")
                .GetProperty("properties")
                .GetProperty("approvedBy")
                .GetProperty("const")
                .GetString());
    }

    [Fact]
    public void Token_object_requires_every_governed_category()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var tokens = schema.RootElement
            .GetProperty("properties")
            .GetProperty("tokens");
        var required = tokens.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToHashSet(StringComparer.Ordinal);
        string[] categories =
        [
            "color", "typography", "spacing", "sizing", "border", "focus",
            "elevation", "motion", "breakpoint", "zIndex"
        ];

        Assert.False(tokens.GetProperty("additionalProperties").GetBoolean());
        Assert.All(categories, category => Assert.Contains(category, required));
    }
}
