using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec002;

public sealed class PolicyEvidenceSchemaTests
{
    private const string SchemaPath =
        "specs/002-aastmt-policy-rulebook/schemas/policy-rule-definition.schema.json";
    private const string ExamplesPath =
        "specs/002-aastmt-policy-rulebook/policy-boundary-examples.md";
    private const string SourcesPath =
        "specs/002-aastmt-policy-rulebook/policy-sources.md";

    [Fact]
    public void Typed_rule_schema_is_closed_non_executable_and_owned_downstream_at_runtime()
    {
        var schemaText = RepositoryFiles.Read(SchemaPath);
        using var schema = JsonDocument.Parse(schemaText);
        var root = schema.RootElement;

        Assert.Equal("https://json-schema.org/draft/2020-12/schema", root.GetProperty("$schema").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
        Assert.Contains("typeKey", root.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        Assert.DoesNotContain("\"expression\"", schemaText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"script\"", schemaText, StringComparison.OrdinalIgnoreCase);
        RepositoryFiles.ContainsAll(schemaText, "SPEC-009", "reasonCode", "sourceRef");
    }

    [Fact]
    public void Boundary_fixture_covers_approved_demo_limits_and_fail_closed_cases()
    {
        var examples = RepositoryFiles.Read(ExamplesPath);

        RepositoryFiles.ContainsAll(
            examples,
            "9-credit minimum",
            "12-credit probation maximum",
            "18-credit normal maximum",
            "Missing prerequisite",
            "No remaining capacity",
            "Exact meeting overlap",
            "Zero travel buffer",
            "Repeat policy unavailable");
    }

    [Fact]
    public void Source_register_separates_source_demo_synthetic_and_unresolved_authority()
    {
        var sources = RepositoryFiles.Read(SourcesPath);

        RepositoryFiles.ContainsAll(
            sources,
            "OfficialAASTMT",
            "AhmedApprovedDemo",
            "SyntheticDemo",
            "UnresolvedInstitutional",
            "2026-07-13",
            "Synthetic gap rows: none",
            "SPEC-015");
    }
}
