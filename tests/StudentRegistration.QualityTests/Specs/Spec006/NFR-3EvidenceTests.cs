namespace StudentRegistration.QualityTests.Specs.Spec006;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public void Responses_exclude_sensitive_and_unauthorized_details() =>
        new Spec006ReleaseEvidenceTests()
            .Nfr3_baseline_and_runtime_evidence_exclude_sensitive_or_unauthorized_details();
}
