using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Policy;

public sealed class PolicyApprovalGateTests
{
    [Fact]
    public void Only_published_approved_effective_policy_can_govern_submission()
    {
        var gate = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-approval-gate.md");

        RepositoryFiles.ContainsAll(
            gate,
            "Draft",
            "Approved",
            "Published",
            "Superseded",
            "server time",
            "exactly one",
            "fail closed");
    }

    [Fact]
    public void Demo_and_institutional_approval_are_never_conflated()
    {
        var gate = RepositoryFiles.Read(
            "specs/002-aastmt-policy-rulebook/contracts/policy-approval-gate.md");

        RepositoryFiles.ContainsAll(
            gate,
            "AhmedApprovedDemo",
            "OfficialAASTMT",
            "not official AASTMT production approval",
            "UnresolvedInstitutional");
    }
}
