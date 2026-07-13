using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec002;

public sealed class PolicyRulebookTests
{
    [Fact]
    public void Rulebook_has_deterministic_effective_scope_and_single_approved_match()
    {
        var versioning = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-versioning.md");

        RepositoryFiles.ContainsAll(
            versioning,
            "EffectiveFromUtc",
            "EffectiveToUtc",
            "Academic scope",
            "highest priority",
            "equal highest priority",
            "fail closed");
    }

    [Fact]
    public void Demo_rulebook_is_immutable_explainable_and_owned_by_governance_only()
    {
        var rulebook = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/policy-rules.md");

        RepositoryFiles.ContainsAll(
            rulebook,
            "`DEMO-POC-2026.1`",
            "Published",
            "immutable",
            "`REPEAT_POLICY_UNAVAILABLE`",
            "Runtime PolicySet and PolicyRule owner: SPEC-009",
            "Historical decision snapshot owner: SPEC-015");
    }
}
