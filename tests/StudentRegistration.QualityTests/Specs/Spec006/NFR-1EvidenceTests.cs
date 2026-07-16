namespace StudentRegistration.QualityTests.Specs.Spec006;

public sealed class NFR_1EvidenceTests
{
    [Fact]
    public void Deterministic_OpenApi_and_ci_evidence_pass() =>
        new Spec006ReleaseEvidenceTests()
            .Nfr1_has_deterministic_semantic_OpenApi_and_ci_evidence();
}
