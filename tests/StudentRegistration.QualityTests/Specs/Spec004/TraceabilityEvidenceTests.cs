using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec004;

public sealed class TraceabilityEvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-004-traceability.md";

    private static readonly string[] ExpectedIds =
    [
        "FR-1", "FR-2", "FR-3", "FR-4", "FR-5", "FR-6", "FR-7", "FR-8", "FR-9",
        "NFR-1", "NFR-2", "NFR-3", "NFR-4",
        "AC-1", "AC-2", "AC-3", "AC-4", "AC-5", "AC-6", "AC-7",
        "EC-1", "EC-2", "EC-3",
        "SC-1", "SC-2", "SC-3",
        "ROUTE-NONE"
    ];

    [Fact]
    public void Every_requirement_scenario_and_route_has_passing_evidence()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var rows = ParseGovernedRows(evidence);

        Assert.Equal(ExpectedIds.Length, rows.Count);
        Assert.Equal(rows.Count, rows.Keys.Distinct(StringComparer.Ordinal).Count());
        Assert.True(
            ExpectedIds.ToHashSet(StringComparer.Ordinal)
                .SetEquals(rows.Keys),
            $"Traceability IDs differ. Actual: {string.Join(", ", rows.Keys.Order())}");
        Assert.All(rows, row => Assert.Equal("PASS", row.Value.Status));
        Assert.All(
            rows,
            row => Assert.Contains(".cs", row.Value.Evidence, StringComparison.Ordinal));

        RepositoryFiles.ContainsAll(
            evidence,
            "exact project shape",
            "atomic audit",
            "stateless key ring",
            "artifact lifecycle",
            "single-writer DbContext",
            "Missing, duplicate, non-PASS, or evidence-free row rejects release");

        var spec = RepositoryFiles.Read(
            "specs/004-architecture-engineering-principles/spec.md");
        RepositoryFiles.ContainsAll(
            spec,
            "## Frontend Route Ownership",
            "No route is directly owned",
            "SPEC-003 route-manifest amendment");
    }

    [Fact]
    public void Critical_release_evidence_artifacts_exist()
    {
        string[] criticalArtifacts =
        [
            "tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs",
            "tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs",
            "tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs",
            "tests/StudentRegistration.IntegrationTests/Persistence/AuditEventPersistenceTests.cs",
            "tests/StudentRegistration.IntegrationTests/Specs/Spec004/DataProtectionRegistrationTests.cs",
            "tests/StudentRegistration.QualityTests/Specs/Spec004/LocalArtifactRetentionTests.cs",
            "docs/release-evidence/SPEC-004-NFR-1.md",
            "docs/release-evidence/SPEC-004-NFR-2.md",
            "docs/release-evidence/SPEC-004-NFR-3.md",
            "docs/release-evidence/SPEC-004-NFR-4.md",
            "docs/release-evidence/SPEC-004-scope-review.md"
        ];

        Assert.All(
            criticalArtifacts,
            artifact => Assert.True(
                RepositoryFiles.Exists(artifact),
                $"Required release evidence is missing: {artifact}"));
    }

    private static IReadOnlyDictionary<string, EvidenceRow> ParseGovernedRows(
        string markdown)
    {
        var expected = ExpectedIds.ToHashSet(StringComparer.Ordinal);
        var rows = markdown
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| ", StringComparison.Ordinal))
            .Select(line => line.Trim().Trim('|')
                .Split('|', StringSplitOptions.TrimEntries))
            .Where(cells => cells.Length == 4 && expected.Contains(cells[0]))
            .Select(cells => new
            {
                Id = cells[0],
                Row = new EvidenceRow(cells[2], cells[3])
            })
            .ToArray();

        Assert.Equal(rows.Length, rows.Select(row => row.Id).Distinct().Count());
        return rows.ToDictionary(row => row.Id, row => row.Row, StringComparer.Ordinal);
    }

    private sealed record EvidenceRow(string Evidence, string Status);
}
