using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ReleaseTests;

public sealed partial class CiGateDefinitionTests
{
    private const string WorkflowPath = ".github/workflows/ci.yml";
    private const string LifecyclePath =
        "tests/StudentRegistration.IntegrationTests/Infrastructure/non-production-lifecycle.json";
    private const string BrowserMatrixPath =
        "tests/StudentRegistration.E2ETests/browser-matrix.json";
    private const string CoverageManifestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec018/critical-rule-coverage-manifest.json";
    private const string ManualAccessibilityEvidencePath =
        "docs/release-evidence/SPEC-018-screen-reader-manual.md";

    private static readonly string[] ExpectedGateOrder =
    [
        "Gate 01 - Restore",
        "Gate 02 - Format",
        "Gate 03 - Warnings-as-errors build",
        "Gate 04 - Unit tests",
        "Gate 05 - Architecture tests",
        "Gate 06 - SQL Server runtime",
        "Gate 07 - Migration readiness",
        "Gate 08 - Versioned seed lifecycle",
        "Gate 09 - SQL integration tests",
        "Gate 10 - E2E browser matrix",
        "Gate 11 - Automated and manual accessibility",
        "Gate 12 - Security and threat model",
        "Gate 13 - Critical-rule coverage",
        "Gate 14 - Correctness invariants",
        "Gate 15 - Release blockers"
    ];

    [Fact]
    public void Required_gates_have_one_exact_fail_fast_dependency_order()
    {
        var workflow = Normalize(RepositoryFiles.Read(WorkflowPath));
        var actual = GateNamePattern().Matches(workflow)
            .Select(match => match.Groups["name"].Value.Trim())
            .ToArray();

        Assert.Equal(ExpectedGateOrder, actual);
        Assert.Contains(
            "  sql-lifecycle:\n    needs: code-quality",
            workflow,
            StringComparison.Ordinal);
        Assert.Contains(
            "  browser-accessibility:\n    needs: sql-lifecycle",
            workflow,
            StringComparison.Ordinal);
        Assert.Contains(
            "  security-release:\n    needs: browser-accessibility",
            workflow,
            StringComparison.Ordinal);
        Assert.DoesNotContain("continue-on-error:", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("if: always()", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("pull_request_target:", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void Sql_and_seed_gates_are_pinned_isolated_and_fail_closed_until_owners_are_executable()
    {
        var workflow = RepositoryFiles.Read(WorkflowPath);
        using var lifecycle = JsonDocument.Parse(RepositoryFiles.Read(LifecyclePath));
        var root = lifecycle.RootElement;
        var sql = root.GetProperty("sqlServer");
        var testing = root.GetProperty("testing");
        var readiness = root.GetProperty("upstreamReadiness");

        Assert.StartsWith(
            "mcr.microsoft.com/mssql/server:2022-",
            sql.GetProperty("image").GetString(),
            StringComparison.Ordinal);
        Assert.Contains("@sha256:", sql.GetProperty("image").GetString());
        Assert.Equal("Developer", sql.GetProperty("edition").GetString());
        Assert.Equal(160, sql.GetProperty("compatibilityLevel").GetInt32());
        Assert.Equal("Testing", testing.GetProperty("environment").GetString());
        Assert.Equal(
            "StudentRegistration_Test_{runId}",
            testing.GetProperty("databasePattern").GetString());
        Assert.True(testing.GetProperty("uniquePerRun").GetBoolean());
        Assert.True(testing.GetProperty("migrationsBeforeSeed").GetBoolean());
        Assert.True(testing.GetProperty("readinessRequired").GetBoolean());
        Assert.True(testing.GetProperty("disposeAfterRun").GetBoolean());

        var missingPaths = readiness.GetProperty("requiredPaths")
            .EnumerateArray()
            .Select(path => path.GetString()!)
            .Where(path => !RepositoryFiles.Exists(path))
            .ToArray();
        Assert.Equal(
            missingPaths.Length is 0 ? "ready" : "blocked",
            readiness.GetProperty("status").GetString());
        RepositoryFiles.ContainsAll(
            workflow,
            "Pinned_testcontainers_runtime_creates_and_disposes_the_unique_database",
            "SqlServerTestDatabaseFixtureTests",
            "Pinned_sql_2022_developer_runs_at_compatibility_160",
            "SqlServerContainerRuntimeTests",
            "NonProductionSeedLifecycleTests",
            "$contract.upstreamReadiness.status -ne 'ready'",
            "throw",
            "migration and seed owners are not executable");
        Assert.DoesNotContain("EnsureCreated", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void Browser_and_accessibility_gates_use_the_governed_matrix_without_a_safari_claim()
    {
        var workflow = RepositoryFiles.Read(WorkflowPath);
        using var matrix = JsonDocument.Parse(RepositoryFiles.Read(BrowserMatrixPath));
        var targets = matrix.RootElement.GetProperty("targets")
            .EnumerateArray()
            .ToArray();

        AssertRequiredTarget(targets, "Google Chrome", "required");
        AssertRequiredTarget(targets, "Microsoft Edge", "required");
        AssertRequiredTarget(targets, "Mozilla Firefox", "required");
        var webkit = AssertRequiredTarget(targets, "Playwright WebKit", "required");
        Assert.Equal(
            "Playwright WebKit (not Safari)",
            webkit.GetProperty("label").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            webkit.GetProperty("playwrightVersion").GetString()));
        var safari = AssertRequiredTarget(targets, "Apple Safari", "deferred");
        Assert.Equal("not-passed", safari.GetProperty("result").GetString());

        RepositoryFiles.ContainsAll(
            workflow,
            BrowserMatrixPath,
            "Google Chrome",
            "Microsoft Edge",
            "Mozilla Firefox",
            "Playwright WebKit (not Safari)",
            "SRS_BROWSER_TARGET",
            "SPEC-018-screen-reader-manual.md",
            "tester",
            "assistiveTechnology",
            "defectLinks",
            "uxQaSignOff",
            "NOT EXECUTED|unsigned");
        Assert.DoesNotContain("Safari passed", workflow, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Manual_accessibility_evidence_keeps_the_machine_readable_block_for_gate_evaluation()
    {
        var template = RepositoryFiles.Read(ManualAccessibilityEvidencePath);
        string[] requiredFields =
        [
            "tester",
            "date",
            "assistiveTechnology",
            "route",
            "scenario",
            "result",
            "defectLinks",
            "uxQaSignOff"
        ];

        Assert.All(requiredFields, field =>
            Assert.Matches($@"(?im)^\s*{Regex.Escape(field)}\s*:\s*\S+", template));
    }

    [Fact]
    public void Security_coverage_invariant_and_release_blockers_cannot_be_bypassed()
    {
        var workflow = RepositoryFiles.Read(WorkflowPath);

        RepositoryFiles.ContainsAll(
            workflow,
            "docs/security/THREAT_MODEL.md",
            "unreviewed or stale threat model",
            "unresolved Critical or High security finding",
            "Critical or Major core-usability defect",
            "capacity, duplicate-enrollment, or atomicity invariant failure",
            "0.90",
            "coverage never replaces behavior tests",
            "unresolvedCriticalHighSecurityFindings",
            "invariantFailures",
            "criticalMajorUsabilityDefects",
            "SPEC-018-NFR-1.md",
            "SPEC-018-NFR-3.md",
            "SPEC-018-NFR-5.md",
            "SPEC-018-NFR-6.md",
            "SPEC-018-NFR-9.md",
            "SPEC-018-traceability.md",
            "Measured NFR evidence is pending or incomplete",
            "Required SPEC-018 tests remain explicitly skipped",
            "productOwnerApproval",
            "domainOwnerApproval",
            "qaApproval",
            "securityApproval",
            "accessibilityApproval",
            "dataConcurrencyApproval",
            "operationsApproval",
            "productionAuthorized",
            "officialAastmtApproval");
        Assert.Contains(
            "SRS_PRODUCTION_AUTHORIZED: \"false\"",
            workflow,
            StringComparison.Ordinal);
        Assert.DoesNotContain("productionAuthorized: true", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("officialAastmtApproval: true", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("permissions: write-all", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void Coverage_gate_uses_exact_owner_paths_instead_of_inventing_generic_layers()
    {
        var workflow = RepositoryFiles.Read(WorkflowPath);
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(CoverageManifestPath));
        var root = manifest.RootElement;
        var owners = root.GetProperty("owners").EnumerateArray().ToArray();

        Assert.Equal("critical-rule-coverage/1.0", root.GetProperty("version").GetString());
        Assert.Equal(0.90, root.GetProperty("minimumBranchRate").GetDouble());
        Assert.True(root.GetProperty("coverageNeverReplacesBehaviorTests").GetBoolean());
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
        Assert.Equal(
            ["SPEC-011", "SPEC-012", "SPEC-014"],
            owners.Select(owner => owner.GetProperty("ownerSpec").GetString()));
        Assert.All(owners, owner =>
        {
            Assert.StartsWith(
                "src/StudentRegistration.",
                owner.GetProperty("sourceProject").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(
                "tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj",
                owner.GetProperty("testProject").GetString());
            Assert.NotEmpty(owner.GetProperty("requiredTestPaths").EnumerateArray());
            Assert.False(string.IsNullOrWhiteSpace(
                owner.GetProperty("sourceAssembly").GetString()));
        });

        Assert.Contains(CoverageManifestPath, workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistration.DomainTests", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistration.ApplicationTests", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void Specialized_test_projects_execute_in_their_owned_gate_instead_of_only_building()
    {
        var workflow = Normalize(RepositoryFiles.Read(WorkflowPath));
        var unitGate = GateBlock(workflow, "Gate 04 - Unit tests");
        var accessibilityGate = GateBlock(
            workflow,
            "Gate 11 - Automated and manual accessibility");
        var securityGate = GateBlock(
            workflow,
            "Gate 12 - Security and threat model");
        var releaseQualityGate = GateBlock(
            workflow,
            "Gate 13 - Critical-rule coverage");

        RepositoryFiles.ContainsAll(
            unitGate,
            "dotnet test",
            "StudentRegistration.OperationsTests.csproj",
            "StudentRegistration.RecoveryTests.csproj");
        RepositoryFiles.ContainsAll(
            accessibilityGate,
            "dotnet test",
            "StudentRegistration.AccessibilityTests.csproj");
        RepositoryFiles.ContainsAll(
            securityGate,
            "dotnet test",
            "StudentRegistration.SecurityTests.csproj");
        RepositoryFiles.ContainsAll(
            releaseQualityGate,
            "dotnet test",
            "StudentRegistration.LoadTests.csproj",
            "StudentRegistration.QualityTests.csproj");
    }

    [Fact]
    public void Local_artifact_gate_enforces_the_git_ignored_seven_day_lifecycle()
    {
        var workflow = RepositoryFiles.Read(WorkflowPath);
        var gitIgnore = RepositoryFiles.Read(".gitignore");
        using var lifecycle = JsonDocument.Parse(RepositoryFiles.Read(LifecyclePath));
        var artifacts = lifecycle.RootElement.GetProperty("localArtifacts");

        Assert.Equal(7, artifacts.GetProperty("maximumAgeDays").GetInt32());
        Assert.True(artifacts.GetProperty("gitIgnored").GetBoolean());
        foreach (var root in artifacts.GetProperty("roots").EnumerateArray())
        {
            Assert.Contains(
                $"{root.GetString()!.TrimEnd('/')}/",
                gitIgnore,
                StringComparison.Ordinal);
        }

        RepositoryFiles.ContainsAll(
            workflow,
            "AddDays(-$contract.localArtifacts.maximumAgeDays)",
            "Remove-Item -LiteralPath",
            "stale local credential/log/export artifact");
    }

    private static JsonElement AssertRequiredTarget(
        JsonElement[] targets,
        string name,
        string status)
    {
        var target = Assert.Single(targets, candidate =>
            candidate.GetProperty("name").GetString() == name);
        Assert.Equal(status, target.GetProperty("status").GetString());
        return target;
    }

    private static string Normalize(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string GateBlock(string workflow, string gateName)
    {
        var match = Regex.Match(
            workflow,
            $@"(?ms)^\s*- name:\s+{Regex.Escape(gateName)}\s*$\n(?<body>.*?)(?=^\s*- name:|\z)");
        Assert.True(match.Success, $"Workflow gate was not found: {gateName}");
        return match.Groups["body"].Value;
    }

    [GeneratedRegex(@"(?m)^\s*- name:\s+(?<name>Gate \d{2} - .+?)\s*$")]
    private static partial Regex GateNamePattern();
}
