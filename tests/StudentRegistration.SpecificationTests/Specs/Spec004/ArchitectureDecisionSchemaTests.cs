using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec004;

public sealed class ArchitectureDecisionSchemaTests
{
    private const string SchemaPath =
        "specs/004-architecture-engineering-principles/schemas/architecture-decision.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_and_owned_by_spec004()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;
        var properties = root.GetProperty("properties");

        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            root.GetProperty("$schema").GetString());
        Assert.Equal(
            "https://student-registration.demo/schemas/architecture-decision/1.0",
            root.GetProperty("$id").GetString());
        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal("1.0", properties.GetProperty("schemaVersion").GetProperty("const").GetString());
        Assert.Equal("SPEC-004", properties.GetProperty("ownerSpec").GetProperty("const").GetString());
    }

    [Fact]
    public void Schema_requires_the_complete_architecture_decision_record()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var required = schema.RootElement.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        string[] expectedRequired =
        [
            "schemaVersion",
            "ownerSpec",
            "decisionId",
            "title",
            "status",
            "decidedAt",
            "approvedBy",
            "context",
            "decision",
            "consequences",
            "simplerAlternativesConsidered",
            "affectedBoundaries",
            "architectureTestUpdates"
        ];

        Assert.Equal(
            expectedRequired.Order(StringComparer.Ordinal),
            required.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Decision_identity_lifecycle_and_approval_are_governed()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var properties = schema.RootElement.GetProperty("properties");

        Assert.Equal(
            "^ADR-[0-9]{3}$",
            properties.GetProperty("decisionId").GetProperty("pattern").GetString());
        Assert.Equal(
            ["accepted", "superseded"],
            properties.GetProperty("status")
                .GetProperty("enum")
                .EnumerateArray()
                .Select(item => item.GetString()!));
        Assert.Equal("date", properties.GetProperty("decidedAt").GetProperty("format").GetString());
        Assert.Equal(
            "Ahmed ELbamby",
            properties.GetProperty("approvedBy").GetProperty("const").GetString());
    }

    [Fact]
    public void Decision_evidence_collections_are_nonempty_and_unique()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var properties = schema.RootElement.GetProperty("properties");

        AssertNonemptyUniqueStringArray(properties.GetProperty("consequences"));
        AssertNonemptyUniqueStringArray(properties.GetProperty("simplerAlternativesConsidered"));
        AssertNonemptyUniqueStringArray(properties.GetProperty("affectedBoundaries"));
        AssertNonemptyUniqueStringArray(properties.GetProperty("architectureTestUpdates"));
    }

    private static void AssertNonemptyUniqueStringArray(JsonElement property)
    {
        Assert.Equal("array", property.GetProperty("type").GetString());
        Assert.Equal(1, property.GetProperty("minItems").GetInt32());
        Assert.True(property.GetProperty("uniqueItems").GetBoolean());
        Assert.Equal("string", property.GetProperty("items").GetProperty("type").GetString());
        Assert.Equal(1, property.GetProperty("items").GetProperty("minLength").GetInt32());
    }
}
