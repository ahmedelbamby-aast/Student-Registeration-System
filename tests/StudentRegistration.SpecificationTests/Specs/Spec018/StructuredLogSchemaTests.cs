using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec018;

public sealed class StructuredLogSchemaTests
{
    private const string ContractPath = "docs/operations/telemetry-contract.md";

    [Fact]
    public void Schema_is_closed_versioned_immutable_and_owned_by_spec018()
    {
        using var document = Spec018SchemaTestSupport.ReadMarkedSchema(
            ContractPath,
            "STRUCTURED_LOG_SCHEMA");
        var schema = document.RootElement;

        Spec018SchemaTestSupport.AssertClosedVersionedOwned(
            schema,
            "https://student-registration.demo/schemas/structured-log/1.0");
        Assert.Equal("StructuredLog", schema.GetProperty("title").GetString());

        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            ContractPath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("018:StructuredLog")
                .GetString());
    }

    [Fact]
    public void Schema_requires_safe_correlated_bounded_log_fields()
    {
        using var document = Spec018SchemaTestSupport.ReadMarkedSchema(
            ContractPath,
            "STRUCTURED_LOG_SCHEMA");
        var schema = document.RootElement;
        var properties = schema.GetProperty("properties");

        Spec018SchemaTestSupport.AssertRequired(
            schema,
            "timestampUtc",
            "level",
            "eventCode",
            "messageTemplate",
            "correlationId",
            "attributes");
        Spec018SchemaTestSupport.AssertUtcTimestamp(properties.GetProperty("timestampUtc"));
        Assert.Equal(
            ["trace", "debug", "information", "warning", "error", "critical"],
            properties.GetProperty("level").GetProperty("enum")
                .EnumerateArray().Select(item => item.GetString()!));
        Assert.InRange(
            properties.GetProperty("messageTemplate").GetProperty("maxLength").GetInt32(),
            1,
            512);

        var attributes = properties.GetProperty("attributes");
        Assert.False(attributes.GetProperty("additionalProperties").GetBoolean());
        Assert.InRange(attributes.GetProperty("maxProperties").GetInt32(), 1, 16);
        Spec018SchemaTestSupport.AssertNoSensitivePropertyNames(schema);
    }

    [Fact]
    public void Canonical_example_is_valid_json_and_uses_only_declared_fields()
    {
        using var document = Spec018SchemaTestSupport.ReadMarkedSchema(
            ContractPath,
            "STRUCTURED_LOG_SCHEMA");
        var schema = document.RootElement;
        var example = schema.GetProperty("examples")[0];
        var serialized = JsonSerializer.Serialize(example);
        using var roundTrip = JsonDocument.Parse(serialized);

        var declared = schema.GetProperty("properties").EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            roundTrip.RootElement.EnumerateObject(),
            property => Assert.Contains(property.Name, declared));
        Assert.Equal(
            "SPEC018_HEALTH_DEGRADED",
            roundTrip.RootElement.GetProperty("eventCode").GetString());
    }

    [Fact]
    public void Retention_and_redaction_rules_are_explicit()
    {
        var contract = RepositoryFiles.Read(ContractPath);
        RepositoryFiles.ContainsAll(
            contract,
            "purged within seven days",
            "MUST NOT contain credentials",
            "full student profiles",
            "raw request or response bodies",
            "bounded fallback",
            "server UTC");
    }
}
