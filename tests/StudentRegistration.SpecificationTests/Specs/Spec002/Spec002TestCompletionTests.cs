using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec002;

public sealed class Spec002TestCompletionTests
{
    private static readonly string[] RequiredTestFiles =
    [
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-1Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-2Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-3Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-4Tests.cs",
        "tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-5Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-1Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-2Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-3Tests.cs",
        "tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-4Tests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-1EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-2EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-3EvidenceTests.cs",
        "tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-4EvidenceTests.cs"
    ];

    private static readonly IReadOnlyDictionary<string, string[]> RequiredEvidence =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["docs/release-evidence/SPEC-002-NFR-1.md"] =
            [
                "NFR-1EvidenceTests.cs", "PB-01 through PB-19", "475",
                "Full-decision differences", "Governance-reference scope", "Result: PASS"
            ],
            ["docs/release-evidence/SPEC-002-NFR-2.md"] =
            [
                "NFR-2EvidenceTests.cs", "PB-01", "PB-19", "Relevant input",
                "Executed fixture inputs", "Result: PASS"
            ],
            ["docs/release-evidence/SPEC-002-NFR-3.md"] =
            [
                "NFR-3EvidenceTests.cs", "10,000", "p95",
                "INFORMATIONAL", "Runtime release result: PENDING",
                "SPEC-002-CONTRACT-TEST-BOUNDARY.md"
            ],
            ["docs/release-evidence/SPEC-002-NFR-4.md"] =
            [
                "NFR-4EvidenceTests.cs", "policy-sources.md", "96 (8 x 12)",
                "189", "NOT_SELECTED", "Result: PASS"
            ]
        };

    [Fact]
    public void Every_approved_SPEC_002_test_task_has_executable_unskipped_coverage()
    {
        Assert.Equal(13, RequiredTestFiles.Length);

        foreach (var path in RequiredTestFiles)
        {
            var source = RepositoryFiles.Read(path);
            Assert.Contains("[Fact", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Skip =", source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Every_SPEC_002_non_functional_test_has_a_measured_evidence_record()
    {
        foreach (var (path, expectedValues) in RequiredEvidence)
        {
            var evidence = RepositoryFiles.Read(path);
            RepositoryFiles.ContainsAll(evidence, ["SPEC-002", "Measured", .. expectedValues]);
        }
    }

    [Fact]
    public void Contract_test_boundary_forbids_reference_oracle_results_from_claiming_runtime_release()
    {
        var boundary = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-002-CONTRACT-TEST-BOUNDARY.md");

        RepositoryFiles.ContainsAll(
            boundary,
            "not runtime or release evidence",
            "SPEC-009",
            "SPEC-011",
            "SPEC-014",
            "SPEC-015",
            "SPEC-018",
            "must not call it as the implementation under test");
    }
}
