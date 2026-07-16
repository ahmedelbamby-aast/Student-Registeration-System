namespace StudentRegistration.QualityTests.Specs.Spec006;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public void Every_generated_response_has_owner_test_evidence() =>
        new Spec006ReleaseEvidenceTests()
            .Nfr2_has_a_nonempty_response_contract_and_owner_test_for_every_operation();
}
