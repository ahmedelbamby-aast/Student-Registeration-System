using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.SpecificationTests.Specs.Spec002;

public sealed class Spec002ReleaseEvidenceTests
{
    [Fact]
    public void Scope_review_records_all_four_exclusions_and_no_product_release_overclaim()
    {
        var review = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-002-scope-review.md");

        RepositoryFiles.ContainsAll(
            review,
            "OS-1", "OS-2", "OS-3", "OS-4",
            "Legal interpretation", "Arbitrary executable", "Waitlist",
            "Automatic dismissal", "PASS",
            "does not authorize a system or production");
    }

    [Fact]
    public void Traceability_covers_every_requirement_scenario_success_criterion_and_route_boundary()
    {
        var traceability = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-002-traceability.md");

        string[] requiredIds =
        [
            "FR-1", "FR-2", "FR-3", "FR-4", "FR-5", "FR-6", "FR-7",
            "NFR-1", "NFR-2", "NFR-3", "NFR-4",
            "AC-1", "AC-2", "AC-3", "AC-4", "AC-5",
            "EC-1", "EC-2", "EC-3", "EC-4",
            "SC-1", "SC-2", "SC-3"
        ];

        Assert.All(requiredIds, id => Assert.Contains($"| {id} |", traceability, StringComparison.Ordinal));
        RepositoryFiles.ContainsAll(
            traceability,
            "No direct frontend route",
            "SPEC-002-CONTRACT-TEST-BOUNDARY.md",
            "Runtime/system release is not claimed",
            "PASS");
    }

    [Fact]
    public void Test_run_manifest_binds_results_to_the_exact_governed_artifact_content()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-002-test-run.json"));
        var root = document.RootElement;

        Assert.Equal("spec002-test-run/1.0", root.GetProperty("version").GetString());
        Assert.Equal(
            Spec002PolicyTestHarness.EvidenceScope,
            root.GetProperty("evidenceScope").GetString());
        Assert.Equal("PASS", root.GetProperty("overallResult").GetString());
        Assert.Equal("Release", root.GetProperty("buildConfiguration").GetString());
        Assert.NotEqual(default, root.GetProperty("measuredAtUtc").GetDateTimeOffset());

        var commands = root.GetProperty("commands").EnumerateArray().ToArray();
        Assert.Equal(4, commands.Length);
        var expectedCounts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["StudentRegistration.AcceptanceTests"] = 6,
            ["StudentRegistration.IntegrationTests"] = 9,
            ["StudentRegistration.QualityTests"] = 5,
            ["StudentRegistration.SpecificationTests"] = 28
        };
        Assert.Equal(
            expectedCounts.Keys.Order(StringComparer.Ordinal),
            commands.Select(command => command.GetProperty("project").GetString()!)
                .Order(StringComparer.Ordinal));
        Assert.All(commands, command =>
        {
            Assert.Equal("PASS", command.GetProperty("result").GetString());
            Assert.Equal(0, command.GetProperty("failed").GetInt32());
            Assert.Equal(0, command.GetProperty("skipped").GetInt32());
            Assert.Equal(
                expectedCounts[command.GetProperty("project").GetString()!],
                command.GetProperty("passed").GetInt32());
            Assert.Contains("Spec002", command.GetProperty("filter").GetString()!, StringComparison.Ordinal);
        });

        var digestRows = root.GetProperty("artifactDigests").EnumerateArray()
            .Select(item => new
            {
                Path = item.GetProperty("path").GetString()!,
                Sha256 = item.GetProperty("sha256").GetString()!
            })
            .OrderBy(item => item.Path, StringComparer.Ordinal)
            .ToArray();
        Assert.True(digestRows.Length >= 30);
        Assert.Equal(
            digestRows.Length,
            digestRows.Select(item => item.Path).Distinct(StringComparer.Ordinal).Count());

        foreach (var row in digestRows)
        {
            var actual = Convert.ToHexString(SHA256.HashData(
                    File.ReadAllBytes(RepositoryFiles.PathTo(row.Path))))
                .ToLowerInvariant();
            Assert.Equal(row.Sha256, actual);
        }

        var canonical = string.Join(
            "\n",
            digestRows.Select(item => $"{item.Path}:{item.Sha256}"));
        var treeDigest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
        Assert.Equal(root.GetProperty("testedContentSha256").GetString(), treeDigest);
    }

    [Fact]
    public void Release_approval_records_every_review_perspective_and_the_non_production_boundary()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-002-release-approval.md");

        RepositoryFiles.ContainsAll(
            approval,
            "**Status:** APPROVED",
            "**Approved by:** Ahmed ELbamby",
            "**Decision date:** 2026-07-13",
            "Product Owner",
            "Registrar / domain owner",
            "QA",
            "Security",
            "Accessibility",
            "Data / concurrency",
            "Operations",
            "48 passed, 0 failed, 0 skipped",
            "not official AASTMT",
            "does not release the Student Registration System",
            "SPEC-009",
            "SPEC-011",
            "SPEC-014",
            "SPEC-015",
            "SPEC-018");
    }
}
