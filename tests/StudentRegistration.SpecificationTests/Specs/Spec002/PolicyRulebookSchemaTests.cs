using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec002;

public sealed class PolicyRulebookSchemaTests
{
    private const string RulebookPath =
        "specs/002-aastmt-policy-rulebook/policy-rules.md";

    [Fact]
    public void Canonical_rulebook_defines_version_scope_categories_and_approval()
    {
        var rulebook = RepositoryFiles.Read(RulebookPath);

        RepositoryFiles.ContainsAll(
            rulebook,
            "`policy-rulebook/1.0`",
            "`DEMO-POC-2026.1`",
            "Effective period",
            "Academic scope",
            "Priority",
            "Approval state",
            "Approved by");
    }

    [Fact]
    public void Canonical_rulebook_preserves_downstream_runtime_ownership()
    {
        var rulebook = RepositoryFiles.Read(RulebookPath);

        RepositoryFiles.ContainsAll(
            rulebook,
            "Runtime PolicySet and PolicyRule owner: SPEC-009",
            "Historical decision snapshot owner: SPEC-015",
            "owns no API handler");
    }
}
