using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_5Tests
{
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

    [Fact]
    public void Measured_unrealistic_profile_enters_rebaseline_without_changing_invariants()
    {
        var activeTarget = new LoadTarget(
            Version: "SPEC-018/1.0",
            CatalogueP95Milliseconds: 300,
            CommitP95Milliseconds: 2_000,
            OptimizerP95Milliseconds: 500,
            AllowedOverbooking: 0,
            AllowedDuplicateEnrollments: 0,
            AllowedPartialSubmissions: 0);
        var unrealisticMeasurement = new LoadMeasurement(
            CatalogueP95Milliseconds: 425,
            CommitP95Milliseconds: 2_600,
            OptimizerP95Milliseconds: 650,
            Overbooking: 0,
            DuplicateEnrollments: 0,
            PartialSubmissions: 0);

        var decision = LoadRebaselineGate.Evaluate(activeTarget, unrealisticMeasurement);

        Assert.Equal(RebaselineDisposition.ApprovedSpecChangeRequired, decision);
        Assert.Throws<InvalidOperationException>(() =>
            LoadRebaselineGate.Apply(
                activeTarget with { CatalogueP95Milliseconds = 450 },
                approval: null));
        Assert.Throws<InvalidOperationException>(() =>
            LoadRebaselineGate.Apply(
                activeTarget with
                {
                    Version = "SPEC-018/1.1",
                    AllowedOverbooking = 1
                },
                new SpecRevisionApproval("SPEC-018/1.1", Approved: true)));

        var approved = LoadRebaselineGate.Apply(
            activeTarget with
            {
                Version = "SPEC-018/1.1",
                CatalogueP95Milliseconds = 450,
                CommitP95Milliseconds = 2_500,
                OptimizerP95Milliseconds = 600
            },
            new SpecRevisionApproval("SPEC-018/1.1", Approved: true));

        Assert.Equal("SPEC-018/1.1", approved.Version);
        Assert.Equal(0, approved.AllowedOverbooking);
        Assert.Equal(0, approved.AllowedDuplicateEnrollments);
        Assert.Equal(0, approved.AllowedPartialSubmissions);
    }

    private static string Normalize(string value) =>
        Regex.Replace(value, @"\s+", " ");

    private static class LoadRebaselineGate
    {
        public static RebaselineDisposition Evaluate(
            LoadTarget target,
            LoadMeasurement measurement)
        {
            if (measurement.Overbooking > target.AllowedOverbooking ||
                measurement.DuplicateEnrollments > target.AllowedDuplicateEnrollments ||
                measurement.PartialSubmissions > target.AllowedPartialSubmissions)
            {
                return RebaselineDisposition.CorrectnessFailure;
            }

            return measurement.CatalogueP95Milliseconds > target.CatalogueP95Milliseconds ||
                measurement.CommitP95Milliseconds > target.CommitP95Milliseconds ||
                measurement.OptimizerP95Milliseconds > target.OptimizerP95Milliseconds
                ? RebaselineDisposition.ApprovedSpecChangeRequired
                : RebaselineDisposition.TargetPasses;
        }

        public static LoadTarget Apply(
            LoadTarget proposedTarget,
            SpecRevisionApproval? approval)
        {
            if (approval is null ||
                !approval.Approved ||
                !string.Equals(
                    approval.Version,
                    proposedTarget.Version,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "An approved SPEC-018 revision is required to rebaseline load targets.");
            }

            if (proposedTarget.AllowedOverbooking is not 0 ||
                proposedTarget.AllowedDuplicateEnrollments is not 0 ||
                proposedTarget.AllowedPartialSubmissions is not 0)
            {
                throw new InvalidOperationException(
                    "Correctness invariants cannot be relaxed by rebaseline.");
            }

            return proposedTarget;
        }
    }

    private enum RebaselineDisposition
    {
        TargetPasses,
        ApprovedSpecChangeRequired,
        CorrectnessFailure
    }

    private sealed record LoadTarget(
        string Version,
        double CatalogueP95Milliseconds,
        double CommitP95Milliseconds,
        double OptimizerP95Milliseconds,
        int AllowedOverbooking,
        int AllowedDuplicateEnrollments,
        int AllowedPartialSubmissions);

    private sealed record LoadMeasurement(
        double CatalogueP95Milliseconds,
        double CommitP95Milliseconds,
        double OptimizerP95Milliseconds,
        int Overbooking,
        int DuplicateEnrollments,
        int PartialSubmissions);

    private sealed record SpecRevisionApproval(string Version, bool Approved);
}
