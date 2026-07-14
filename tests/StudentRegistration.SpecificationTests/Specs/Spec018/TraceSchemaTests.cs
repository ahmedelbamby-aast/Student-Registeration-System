using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec018;

public sealed class TraceSchemaTests
{
    private const string ContractPath = "docs/operations/telemetry-contract.md";

    [Fact]
    public void Schema_is_closed_versioned_immutable_and_owned_by_spec018()
    {
        using var document = Spec018SchemaTestSupport.ReadMarkedSchema(
            ContractPath,
            "TRACE_SCHEMA");
        var schema = document.RootElement;

        Spec018SchemaTestSupport.AssertClosedVersionedOwned(
            schema,
            "https://student-registration.demo/schemas/trace/1.0");
        Assert.Equal("Trace", schema.GetProperty("title").GetString());

        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            ContractPath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("018:Trace")
                .GetString());
    }

    [Fact]
    public void Schema_requires_correlated_timing_status_and_allow_listed_attributes()
    {
        using var document = Spec018SchemaTestSupport.ReadMarkedSchema(
            ContractPath,
            "TRACE_SCHEMA");
        var schema = document.RootElement;
        var properties = schema.GetProperty("properties");

        Spec018SchemaTestSupport.AssertRequired(
            schema,
            "traceId",
            "spanId",
            "correlationId",
            "operationName",
            "startedAtUtc",
            "durationMs",
            "status",
            "attributes");
        Spec018SchemaTestSupport.AssertUtcTimestamp(properties.GetProperty("startedAtUtc"));
        Assert.Equal(0, properties.GetProperty("durationMs").GetProperty("minimum").GetInt32());
        Assert.Equal(
            ["unset", "ok", "error"],
            properties.GetProperty("status").GetProperty("enum")
                .EnumerateArray().Select(item => item.GetString()!));

        var attributes = properties.GetProperty("attributes");
        Assert.False(attributes.GetProperty("additionalProperties").GetBoolean());
        Assert.InRange(attributes.GetProperty("maxProperties").GetInt32(), 1, 16);
        Spec018SchemaTestSupport.AssertNoSensitivePropertyNames(schema);
    }

    [Fact]
    public void Canonical_example_serializes_without_topology_or_person_identifiers()
    {
        using var document = Spec018SchemaTestSupport.ReadMarkedSchema(
            ContractPath,
            "TRACE_SCHEMA");
        var example = document.RootElement.GetProperty("examples")[0];
        var serialized = JsonSerializer.Serialize(example);
        using var roundTrip = JsonDocument.Parse(serialized);

        Assert.Equal(
            "registration.submit",
            roundTrip.RootElement.GetProperty("operationName").GetString());
        Assert.False(roundTrip.RootElement.TryGetProperty("serverName", out _));
        Assert.False(roundTrip.RootElement.TryGetProperty("databaseName", out _));
        Assert.False(roundTrip.RootElement.TryGetProperty("userId", out _));
    }

    [Fact]
    public void Contract_distinguishes_schema_from_runtime_exporter_configuration()
    {
        var contract = RepositoryFiles.Read(ContractPath);
        RepositoryFiles.ContainsAll(
            contract,
            "governed telemetry schemas, not domain or EF entities",
            "Exporter selection and endpoint configuration are deployment concerns",
            "PII-minimized",
            "query strings");
    }
}
