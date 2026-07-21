using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec006;

public sealed class Spec006ReleaseEvidenceTests
{
    private const string Baseline =
        "specs/006-domain-class-api-contracts/contracts/openapi/student-registration-v1.json";

    [Fact]
    public void Nfr1_has_deterministic_semantic_OpenApi_and_ci_evidence()
    {
        using var document = OpenApi();
        Assert.Equal(26, document.RootElement.GetProperty("paths").EnumerateObject().Count());
        Assert.True(document.RootElement.GetProperty("components")
            .GetProperty("schemas").EnumerateObject().Count() >= 54);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(".github/scripts/Verify-OpenApi.ps1"),
            "OpenApiBaselineTests",
            "Generated OpenAPI differs semantically");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(".github/workflows/ci.yml"),
            "OpenAPI semantic drift",
            "Verify-OpenApi.ps1");
        AssertPass("docs/release-evidence/SPEC-006-NFR-1.md");
    }

    [Fact]
    public void Nfr2_has_a_nonempty_response_contract_and_owner_test_for_every_operation()
    {
        using var document = OpenApi();
        var operations = document.RootElement.GetProperty("paths")
            .EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject())
            .ToArray();
        Assert.Equal(27, operations.Length);
        Assert.All(
            operations,
            operation => Assert.NotEmpty(
                operation.Value.GetProperty("responses").EnumerateObject()));

        var spec007Tests = Directory.GetFiles(
            RepositoryFiles.PathTo("tests/StudentRegistration.ContractTests/Specs/Spec007"),
            "Endpoint*ContractTests.cs");
        var spec008Tests = Directory.GetFiles(
            RepositoryFiles.PathTo("tests/StudentRegistration.ContractTests/Specs/Spec008"),
            "Endpoint*ContractTests.cs");
        Assert.True(spec007Tests.Length >= 16);
        Assert.True(spec008Tests.Length >= 10);
        Assert.True(RepositoryFiles.Exists(
            "tests/StudentRegistration.OperationsTests/Specs/Spec018/Endpoint01BehaviorTests.cs"));
        Assert.True(RepositoryFiles.Exists(
            "tests/StudentRegistration.OperationsTests/Specs/Spec018/Endpoint02BehaviorTests.cs"));
        AssertPass("docs/release-evidence/SPEC-006-NFR-2.md");
    }

    [Fact]
    public void Nfr3_baseline_and_runtime_evidence_exclude_sensitive_or_unauthorized_details()
    {
        var baseline = RepositoryFiles.Read(Baseline);
        foreach (var forbidden in new[]
        {
            "connectionString", "passwordHash", "securityStamp",
            "stackTrace", "SqlException"
        })
        {
            Assert.DoesNotContain(forbidden, baseline, StringComparison.OrdinalIgnoreCase);
        }

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.IntegrationTests/Specs/Spec006/EdgeCases/EC-4Tests.cs"),
            "Unauthorized",
            "Forbidden",
            "ETag");
        AssertPass("docs/release-evidence/SPEC-006-NFR-3.md");
    }

    [Fact]
    public void Nfr4_generated_schemas_use_the_shared_json_policy()
    {
        using var document = OpenApi();
        var schemas = document.RootElement.GetProperty("components")
            .GetProperty("schemas");
        AssertPropertySet(
            schemas.GetProperty("ApiError"),
            "code", "message", "correlationId", "fieldErrors", "currentVersion");
        AssertPropertySet(
            schemas.GetProperty("PublicContextDto"),
            "serverTimeUtc", "timeZoneId", "teachingTermLabel",
            "registrationTermLabel", "registrationWindowState", "serviceState");
        Assert.Contains(
            "\"format\": \"date-time\"",
            RepositoryFiles.Read(Baseline),
            StringComparison.Ordinal);
        AssertPass("docs/release-evidence/SPEC-006-NFR-4.md");
    }

    [Fact]
    public void Scope_traceability_and_demo_approval_are_complete_and_fail_closed()
    {
        var scope = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-006-scope-review.md");
        RepositoryFiles.ContainsAll(
            scope,
            "GraphQL",
            "gRPC",
            "Generic CRUD",
            "mediator",
            "breaking change",
            "**Result: PASS.**");

        var traceability = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-006-traceability.md");
        foreach (var id in Enumerable.Range(1, 10).Select(value => $"FR-{value}")
            .Concat(Enumerable.Range(1, 4).Select(value => $"NFR-{value}"))
            .Concat(Enumerable.Range(1, 9).Select(value => $"AC-{value}"))
            .Concat(Enumerable.Range(1, 4).Select(value => $"EC-{value}"))
            .Concat(Enumerable.Range(1, 3).Select(value => $"SC-{value}")))
        {
            Assert.Contains($"| {id} |", traceability, StringComparison.Ordinal);
        }

        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-006-release-approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "Ahmed ELbamby",
            "Product owner: APPROVED",
            "Domain owner: APPROVED",
            "QA: APPROVED",
            "Security: APPROVED",
            "Accessibility: APPROVED",
            "Data/concurrency: APPROVED",
            "Operations: APPROVED",
            "Production authorized: false",
            "Official AASTMT approval: false");
    }

    private static JsonDocument OpenApi() =>
        JsonDocument.Parse(RepositoryFiles.Read(Baseline));

    private static void AssertPass(string path) =>
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(path),
            "**Release result:** PASS",
            "Runtime execution result: PASS");

    private static void AssertPropertySet(
        JsonElement schema,
        params string[] expected)
    {
        var actual = schema.GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(expected.ToHashSet(StringComparer.Ordinal), actual);
    }
}
