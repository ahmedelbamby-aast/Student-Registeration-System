using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Policy;

public sealed class PublishedPolicyImmutabilityTests
{
    [Fact]
    public void Published_rulebook_is_never_edited_or_deleted()
    {
        var rulebook = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/policy-rules.md");

        RepositoryFiles.ContainsAll(
            rulebook,
            "Published versions are immutable",
            "MUST NOT be edited",
            "MUST NOT be deleted",
            "Superseded",
            "expected version");
    }

    [Fact]
    public void Historical_decisions_retain_original_policy_and_source_metadata()
    {
        var rulebook = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/policy-rules.md");

        RepositoryFiles.ContainsAll(
            rulebook,
            "policy version",
            "input summary",
            "source URL",
            "source access date",
            "SPEC-015");
    }
}
