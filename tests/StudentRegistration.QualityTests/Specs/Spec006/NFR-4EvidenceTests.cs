namespace StudentRegistration.QualityTests.Specs.Spec006;

public sealed class NFR_4EvidenceTests
{
    [Fact]
    public void Generated_schemas_use_the_shared_json_policy() =>
        new Spec006ReleaseEvidenceTests()
            .Nfr4_generated_schemas_use_the_shared_json_policy();
}
