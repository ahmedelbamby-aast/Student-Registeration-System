using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec007;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public void Authentication_and_recovery_evidence_is_enumeration_resistant()
    {
        var evidence = RepositoryFiles.Read("docs/release-evidence/SPEC-007-NFR-2.md");
        var normalizedEvidence = System.Text.RegularExpressions.Regex.Replace(
            evidence,
            @"\s+",
            " ");
        var studentAuthentication = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StudentAuthenticationService.cs");
        var staffAuthentication = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StaffAuthenticationService.cs");
        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");

        RepositoryFiles.ContainsAll(
            normalizedEvidence,
            "SPEC-007 NFR-2",
            "unknown account",
            "wrong credential",
            "disabled account",
            "locked account",
            "same public status",
            "same public error code",
            "generic 202",
            "no recovery proof",
            "**Result: PASS.**");
        RepositoryFiles.ContainsAll(
            studentAuthentication,
            "_dummyHash",
            "AuthenticationFailed");
        RepositoryFiles.ContainsAll(
            staffAuthentication,
            "_dummyHash",
            "AuthenticationFailed");
        RepositoryFiles.ContainsAll(
            endpoints,
            "AUTHENTICATION_FAILED",
            "Results.Accepted()");
        Assert.DoesNotContain("proof =", evidence, StringComparison.OrdinalIgnoreCase);
    }
}
