using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec004;

public sealed class ModuleBoundarySchemaTests
{
    private const string SchemaPath =
        "specs/004-architecture-engineering-principles/schemas/module-boundary.schema.json";

    private static readonly string[] CanonicalModules =
    [
        "Client",
        "Api",
        "Contracts",
        "IdentityAccess",
        "Academics",
        "Scheduling",
        "Registration",
        "StaffAdministration",
        "Infrastructure.SqlServer"
    ];

    private static readonly string[] CanonicalProjects =
        CanonicalModules.Select(module => $"StudentRegistration.{module}").ToArray();

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
            "https://student-registration.demo/schemas/module-boundary/1.0",
            root.GetProperty("$id").GetString());
        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal("1.0", properties.GetProperty("schemaVersion").GetProperty("const").GetString());
        Assert.Equal("SPEC-004", properties.GetProperty("ownerSpec").GetProperty("const").GetString());
    }

    [Fact]
    public void Schema_requires_the_complete_boundary_record_and_exact_project_shape()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;
        var properties = root.GetProperty("properties");
        var required = root.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        string[] expectedRequired =
        [
            "schemaVersion",
            "ownerSpec",
            "module",
            "project",
            "ownedConcerns",
            "allowedProjectReferences",
            "exposedContracts",
            "approvalVersion"
        ];

        Assert.Equal(
            expectedRequired.Order(StringComparer.Ordinal),
            required.Order(StringComparer.Ordinal));
        Assert.Equal(
            CanonicalModules.Order(StringComparer.Ordinal),
            EnumValues(properties.GetProperty("module")).Order(StringComparer.Ordinal));
        Assert.Equal(
            CanonicalProjects.Order(StringComparer.Ordinal),
            EnumValues(properties.GetProperty("project")).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Boundary_collections_are_unique_and_references_use_canonical_projects()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var properties = schema.RootElement.GetProperty("properties");

        AssertUniqueStringArray(properties.GetProperty("ownedConcerns"), requiresItem: true);
        AssertUniqueStringArray(properties.GetProperty("exposedContracts"), requiresItem: false);

        var references = properties.GetProperty("allowedProjectReferences");
        Assert.Equal("array", references.GetProperty("type").GetString());
        Assert.True(references.GetProperty("uniqueItems").GetBoolean());
        Assert.Equal(
            CanonicalProjects.Order(StringComparer.Ordinal),
            EnumValues(references.GetProperty("items")).Order(StringComparer.Ordinal));

        var approvalVersion = properties.GetProperty("approvalVersion");
        Assert.Equal("string", approvalVersion.GetProperty("type").GetString());
        Assert.Equal(1, approvalVersion.GetProperty("minLength").GetInt32());
    }

    private static IEnumerable<string> EnumValues(JsonElement element) =>
        element.GetProperty("enum")
            .EnumerateArray()
            .Select(item => item.GetString()!);

    private static void AssertUniqueStringArray(JsonElement property, bool requiresItem)
    {
        Assert.Equal("array", property.GetProperty("type").GetString());
        Assert.True(property.GetProperty("uniqueItems").GetBoolean());
        Assert.Equal("string", property.GetProperty("items").GetProperty("type").GetString());
        Assert.Equal(1, property.GetProperty("items").GetProperty("minLength").GetInt32());

        if (requiresItem)
        {
            Assert.Equal(1, property.GetProperty("minItems").GetInt32());
        }
    }
}
