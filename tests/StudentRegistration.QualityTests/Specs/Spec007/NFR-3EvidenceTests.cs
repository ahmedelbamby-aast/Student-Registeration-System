using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public void Credential_policy_evidence_matches_the_pinned_demo_baseline()
    {
        var evidence = RepositoryFiles.Read("docs/release-evidence/SPEC-007-NFR-3.md");
        var options = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/IdentitySecurityOptions.cs");
        var composition = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/IdentitySecurityRegistration.cs");

        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-007 NFR-3",
            "IdentityV3",
            "100,000",
            "15-128",
            "DEMO-POC-2026.1",
            "no character-class composition",
            "five failures",
            "five minutes",
            "15 minutes",
            "Production fails closed",
            "**Result: PASS.**");
        RepositoryFiles.ContainsAll(
            options,
            "PasswordHasherCompatibilityMode.IdentityV3",
            "100_000",
            "DefaultMinimumPasswordLength = 15",
            "DefaultMaximumPasswordLength = 128",
            "DefaultMaximumFailures = 5",
            "UseDemoCredentialPolicy",
            "IDENTITY_POLICY_NOT_APPROVED");
        RepositoryFiles.ContainsAll(
            composition,
            "RequireDigit = false",
            "RequireLowercase = false",
            "RequireUppercase = false",
            "RequireNonAlphanumeric = false");
    }
}
