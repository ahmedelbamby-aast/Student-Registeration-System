using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class ApiErrorSchemaTests
{
    private const string SchemaPath =
        "specs/006-domain-class-api-contracts/schemas/api-error.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_and_owned_by_spec006()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;

        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            root.GetProperty("$schema").GetString());
        Assert.Equal(
            "https://student-registration.demo/schemas/api-error/1.0",
            root.GetProperty("$id").GetString());
        Assert.Equal("SPEC-006", root.GetProperty("x-owner-spec").GetString());
        Assert.Equal("1.0", root.GetProperty("x-schema-version").GetString());
        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
    }

    [Fact]
    public void Schema_requires_only_safe_core_fields_and_keeps_details_optional()
    {
        using var schema = JsonDocument.Parse(RepositoryFiles.Read(SchemaPath));
        var root = schema.RootElement;
        var properties = root.GetProperty("properties");
        var required = root.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            new[] { "code", "correlationId", "message" },
            required.Order(StringComparer.Ordinal));
        Assert.Equal(
            new[] { "code", "correlationId", "currentVersion", "fieldErrors", "message" },
            properties.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.DoesNotContain("fieldErrors", required);
        Assert.DoesNotContain("currentVersion", required);

        AssertNonEmptyString(properties.GetProperty("code"));
        AssertNonEmptyString(properties.GetProperty("message"));
        AssertNonEmptyString(properties.GetProperty("correlationId"));
        AssertNonEmptyString(properties.GetProperty("currentVersion"));

        var fieldErrors = properties.GetProperty("fieldErrors");
        Assert.Equal("object", fieldErrors.GetProperty("type").GetString());
        Assert.Equal(20, fieldErrors.GetProperty("maxProperties").GetInt32());
        var messages = fieldErrors.GetProperty("additionalProperties");
        Assert.Equal("array", messages.GetProperty("type").GetString());
        Assert.Equal(1, messages.GetProperty("minItems").GetInt32());
        Assert.Equal(5, messages.GetProperty("maxItems").GetInt32());
        AssertNonEmptyString(messages.GetProperty("items"));
        Assert.Equal(256, messages.GetProperty("items").GetProperty("maxLength").GetInt32());
    }

    private static void AssertNonEmptyString(JsonElement property)
    {
        Assert.Equal("string", property.GetProperty("type").GetString());
        Assert.Equal(1, property.GetProperty("minLength").GetInt32());
    }
}
