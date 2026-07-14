using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

internal static class Spec018LoadEvidenceAssertions
{
    public static string ReadPending(string requirement)
    {
        var evidence = RepositoryFiles.Read(
            $"docs/release-evidence/SPEC-018-{requirement}.md");

        RepositoryFiles.ContainsAll(
            evidence,
            $"# SPEC-018 {requirement} Release Evidence",
            "**Artifact version:** 1.0.0",
            $"**Requirement:** {requirement}",
            "**Recorded:** 2026-07-14",
            "**Owner:** Ahmed ELbamby",
            "**Release result:** PENDING",
            "**Production authority:** Not granted",
            "Runtime execution result: PENDING",
            "Activation condition");

        Assert.DoesNotContain(
            "Runtime execution result: PASS",
            evidence,
            StringComparison.OrdinalIgnoreCase);
        return evidence;
    }
}
