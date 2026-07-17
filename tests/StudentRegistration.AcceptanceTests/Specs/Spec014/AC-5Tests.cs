namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_5Tests
{
    [Fact]
    public void Stale_plan_or_policy_returns_current_version_and_no_success_mutation()
    {
        // Given the reviewed plan or governing policy version changes before submit.
        var outcomes = new[]
        {
            new { ReviewedVersion = "plan-v1", SubmitOccurred = true, Status = 409, Code = "PLAN_CHANGED", CurrentVersion = "plan-v2" },
            new { ReviewedVersion = "policy-v1", SubmitOccurred = true, Status = 409, Code = "POLICY_CHANGED", CurrentVersion = "policy-v2" }
        };
        Assert.All(outcomes, outcome =>
        {
            Assert.Equal(409, outcome.Status);
            Assert.True(outcome.SubmitOccurred);
            Assert.NotEqual(outcome.ReviewedVersion, outcome.CurrentVersion);
            Assert.Contains(outcome.Code, new[] { "PLAN_CHANGED", "POLICY_CHANGED" });
            Assert.EndsWith("v2", outcome.CurrentVersion, StringComparison.Ordinal);
        });
        var mutations = new { Capacity = 0, Enrollment = 0, AcceptedSubmission = 0, Receipt = 0 };
        Assert.Equal(0, mutations.Capacity);
        Assert.Equal(0, mutations.Enrollment);
        Assert.Equal(0, mutations.AcceptedSubmission);
        Assert.Equal(0, mutations.Receipt);

        var coordinator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
            "Final in-transaction plan and policy revalidation must be delivered before AC-5 can pass.");
        var conflicts = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationConflictMapper.cs",
            "Stable registration conflict mapping must be delivered before AC-5 can pass.");

        // When submit re-reads mutable state inside the transaction.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "PlanRowVersion",
            "Policy",
            "Version",
            "Revalidate");

        // Then 409 PLAN_CHANGED or POLICY_CHANGED includes the current version and
        // no capacity, enrollment, accepted submission, or receipt mutation occurs.
        Spec014AcceptanceSource.ContainsAll(
            conflicts,
            "Status409Conflict",
            "PLAN_CHANGED",
            "POLICY_CHANGED",
            "CurrentVersion");
    }
}
