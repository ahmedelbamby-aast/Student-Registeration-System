using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Policy;

public sealed class PolicySourceResolutionTests
{
    [Fact]
    public void Conflicts_are_resolved_explicitly_or_fail_closed()
    {
        var resolution = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/source-resolution.md");

        RepositoryFiles.ContainsAll(
            resolution,
            "9-credit",
            "12-credit guidance",
            "Withdrawal",
            "capacity",
            "meeting conflict",
            "In Review",
            "fail closed");
    }

    [Fact]
    public void Source_unavailability_never_rewrites_published_history()
    {
        var resolution = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/source-resolution.md");

        RepositoryFiles.ContainsAll(
            resolution,
            "source unavailable",
            "review required",
            "retain",
            "immutable",
            "SPEC-015");
    }
}
