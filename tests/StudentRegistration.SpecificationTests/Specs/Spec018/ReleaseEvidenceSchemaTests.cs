using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec018;

public sealed class ReleaseEvidenceSchemaTests
{
    private const string SchemaPath =
        "docs/release-evidence/schemas/release-evidence.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_immutable_and_owned_by_spec018()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var schema = document.RootElement;

        Spec018SchemaTestSupport.AssertClosedVersionedOwned(
            schema,
            "https://student-registration.demo/schemas/release-evidence/1.0");
        Assert.Equal("SPEC-018 ReleaseEvidence", schema.GetProperty("title").GetString());

        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            SchemaPath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("018:ReleaseEvidence")
                .GetString());
    }

    [Fact]
    public void Schema_requires_source_identity_gate_results_and_approval_perspectives()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var schema = document.RootElement;
        var properties = schema.GetProperty("properties");

        Spec018SchemaTestSupport.AssertRequired(
            schema,
            "schemaVersion",
            "ownerSpec",
            "scope",
            "evidenceId",
            "evidenceVersion",
            "releaseCandidate",
            "sourceCommit",
            "recordedAtUtc",
            "status",
            "gates",
            "approvals",
            "productionAuthorized",
            "officialAastmtApproval");
        Assert.Equal("non-production-demo", properties.GetProperty("scope").GetProperty("const").GetString());
        Assert.False(properties.GetProperty("productionAuthorized").GetProperty("const").GetBoolean());
        Assert.False(properties.GetProperty("officialAastmtApproval").GetProperty("const").GetBoolean());
        Assert.Equal(1, properties.GetProperty("gates").GetProperty("minItems").GetInt32());
        Assert.Equal(1, properties.GetProperty("approvals").GetProperty("minItems").GetInt32());
        Assert.Equal(
            ["pass", "fail", "blocked"],
            properties.GetProperty("status").GetProperty("enum")
                .EnumerateArray().Select(item => item.GetString()!));
        Spec018SchemaTestSupport.AssertNoSensitivePropertyNames(schema);
    }

    [Fact]
    public void Gate_records_are_closed_bounded_and_link_to_repository_evidence()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var gate = document.RootElement.GetProperty("$defs").GetProperty("gate");
        Assert.False(gate.GetProperty("additionalProperties").GetBoolean());
        Spec018SchemaTestSupport.AssertRequired(
            gate,
            "gateId",
            "category",
            "status",
            "evidenceRefs",
            "blockingReasons");
        Assert.Equal(1, gate.GetProperty("properties")
            .GetProperty("evidenceRefs").GetProperty("minItems").GetInt32());
        Assert.Equal(
            "^(docs|tests|ops)/",
            gate.GetProperty("properties").GetProperty("evidenceRefs")
                .GetProperty("items").GetProperty("pattern").GetString());
    }

    [Fact]
    public void Passing_release_requires_every_gate_and_approval_to_pass_without_blockers()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var passRule = document.RootElement.GetProperty("allOf")[0];
        var properties = passRule.GetProperty("then").GetProperty("properties");
        var gate = properties.GetProperty("gates").GetProperty("items")
            .GetProperty("properties");
        var approval = properties.GetProperty("approvals").GetProperty("items")
            .GetProperty("properties");

        Assert.Equal("pass", gate.GetProperty("status").GetProperty("const").GetString());
        Assert.Equal(0, gate.GetProperty("blockingReasons").GetProperty("maxItems").GetInt32());
        Assert.Equal(
            "approved",
            approval.GetProperty("decision").GetProperty("const").GetString());

        Assert.Equal(9, properties.GetProperty("gates").GetProperty("minItems").GetInt32());
        Assert.Equal(
            [
                "code",
                "architecture",
                "data",
                "security",
                "accessibility",
                "load",
                "recovery",
                "operations",
                "product"
            ],
            RequiredContainsConstants(properties.GetProperty("gates"), "category"));
        Assert.Equal(7, properties.GetProperty("approvals").GetProperty("minItems").GetInt32());
        Assert.Equal(
            [
                "product-owner",
                "domain-owner",
                "qa",
                "security",
                "accessibility",
                "data-concurrency",
                "operations"
            ],
            RequiredContainsConstants(properties.GetProperty("approvals"), "perspective"));
    }

    [Fact]
    public void Canonical_example_serializes_without_claiming_release_or_institutional_authority()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var example = document.RootElement.GetProperty("examples")[0];
        using var roundTrip = JsonDocument.Parse(JsonSerializer.Serialize(example));

        Assert.Equal("blocked", roundTrip.RootElement.GetProperty("status").GetString());
        Assert.False(roundTrip.RootElement.GetProperty("productionAuthorized").GetBoolean());
        Assert.False(roundTrip.RootElement.GetProperty("officialAastmtApproval").GetBoolean());
        Assert.NotEmpty(roundTrip.RootElement.GetProperty("gates").EnumerateArray());
    }

    private static string[] RequiredContainsConstants(JsonElement collectionRule, string property) =>
        collectionRule.GetProperty("allOf")
            .EnumerateArray()
            .Select(rule => rule.GetProperty("contains")
                .GetProperty("properties")
                .GetProperty(property)
                .GetProperty("const")
                .GetString()!)
            .ToArray();
}
