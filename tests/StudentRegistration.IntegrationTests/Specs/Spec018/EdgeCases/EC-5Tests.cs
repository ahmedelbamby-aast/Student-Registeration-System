using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_5Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-018 T061/T064 must first produce a measured unrealistic target or spike result from the executable load harness; only an Ahmed-approved SPEC-018 revision may then change the target, and correctness invariants remain non-negotiable.";

    [Fact]
    public void Governance_contract_requires_approved_rebaseline_and_never_relaxes_correctness()
    {
        var requirements = Normalize(RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/requirements.md"));
        var research = Normalize(RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/research.md"));

        Assert.Contains(
            "Load targets prove unrealistic -> rebaseline by approved spec change, never silently relax correctness",
            requirements,
            StringComparison.Ordinal);
        Assert.Contains(
            "Correctness has zero tolerance in every profile",
            research,
            StringComparison.Ordinal);
        Assert.Contains(
            "POC changes require an approved spec revision; no institutional value is guessed",
            research,
            StringComparison.Ordinal);
    }

    [Fact(Skip = ActivationGate)]
    public void Measured_unrealistic_profile_enters_rebaseline_without_changing_invariants()
    {
        throw new NotImplementedException(
            "The approved governance response exists, but no load result exists to trigger it.");
    }

    private static string Normalize(string value) =>
        Regex.Replace(value, @"\s+", " ");
}
