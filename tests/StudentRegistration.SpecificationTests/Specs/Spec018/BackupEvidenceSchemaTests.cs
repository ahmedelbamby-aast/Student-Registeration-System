using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec018;

public sealed class BackupEvidenceSchemaTests
{
    private const string SchemaPath =
        "docs/release-evidence/schemas/backup-evidence.schema.json";

    [Fact]
    public void Schema_is_closed_versioned_immutable_and_owned_by_spec018()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var schema = document.RootElement;

        Spec018SchemaTestSupport.AssertClosedVersionedOwned(
            schema,
            "https://student-registration.demo/schemas/backup-evidence/1.0");
        Assert.Equal("SPEC-018 BackupEvidence", schema.GetProperty("title").GetString());

        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            SchemaPath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("018:BackupEvidence")
                .GetString());
    }

    [Fact]
    public void Schema_requires_restore_timing_targets_integrity_and_reconciliation()
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
            "sourceCommit",
            "recordedAtUtc",
            "backupId",
            "backupCreatedAtUtc",
            "restoreEnvironment",
            "restoreStartedAtUtc",
            "restoreCompletedAtUtc",
            "rpoTargetSeconds",
            "measuredRpoSeconds",
            "rtoTargetSeconds",
            "measuredRtoSeconds",
            "integrityChecks",
            "reconciliationStatus",
            "status",
            "approvedBy",
            "productionAuthorized");
        Assert.Equal(300, properties.GetProperty("rpoTargetSeconds").GetProperty("const").GetInt32());
        Assert.Equal(3600, properties.GetProperty("rtoTargetSeconds").GetProperty("const").GetInt32());
        Assert.Equal(0, properties.GetProperty("measuredRpoSeconds").GetProperty("minimum").GetInt32());
        Assert.Equal(0, properties.GetProperty("measuredRtoSeconds").GetProperty("minimum").GetInt32());
        Assert.Equal(
            ["pass", "fail", "blocked"],
            properties.GetProperty("status").GetProperty("enum")
                .EnumerateArray().Select(item => item.GetString()!));
        Assert.Equal(1, properties.GetProperty("integrityChecks").GetProperty("minItems").GetInt32());
        Spec018SchemaTestSupport.AssertNoSensitivePropertyNames(schema);
    }

    [Fact]
    public void Corrections_require_a_superseded_record_and_rationale()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var allOf = document.RootElement.GetProperty("allOf");
        Assert.Equal(2, allOf.GetArrayLength());
        var correctionRule = allOf[0];

        Assert.True(correctionRule.TryGetProperty("if", out _));
        Assert.True(correctionRule.TryGetProperty("then", out var then));
        Assert.Contains(
            "supersessionRationale",
            then.GetProperty("required").EnumerateArray().Select(item => item.GetString()));
    }

    [Fact]
    public void Passing_evidence_requires_targets_reconciliation_and_every_integrity_check_to_pass()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var passRule = document.RootElement.GetProperty("allOf")[1];
        var properties = passRule.GetProperty("then").GetProperty("properties");

        Assert.Equal(
            300,
            properties.GetProperty("measuredRpoSeconds").GetProperty("maximum").GetInt32());
        Assert.Equal(
            3600,
            properties.GetProperty("measuredRtoSeconds").GetProperty("maximum").GetInt32());
        Assert.Equal(
            "pass",
            properties.GetProperty("reconciliationStatus").GetProperty("const").GetString());
        Assert.Equal(
            "pass",
            properties.GetProperty("integrityChecks")
                .GetProperty("items")
                .GetProperty("properties")
                .GetProperty("status")
                .GetProperty("const")
                .GetString());
    }

    [Fact]
    public void Canonical_example_is_serializable_and_explicitly_non_production()
    {
        using var document = Spec018SchemaTestSupport.ReadSchema(SchemaPath);
        var example = document.RootElement.GetProperty("examples")[0];
        using var roundTrip = JsonDocument.Parse(JsonSerializer.Serialize(example));

        Assert.Equal("non-production-demo", roundTrip.RootElement.GetProperty("scope").GetString());
        Assert.False(roundTrip.RootElement.GetProperty("productionAuthorized").GetBoolean());
        Assert.Equal("pass", roundTrip.RootElement.GetProperty("status").GetString());
    }
}
