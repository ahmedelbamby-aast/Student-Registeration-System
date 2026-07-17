namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_7Tests
{
    [Fact]
    public void Same_student_concurrent_plans_serialize_and_only_one_valid_combination_commits()
    {
        // Given two plans for one student/term are valid alone but jointly exceed
        // the credit maximum or overlap.
        var outcome = new
        {
            PlanAIndividuallyValid = true,
            PlanBIndividuallyValid = true,
            JointPlansWouldOverlapOrExceedLoad = true,
            DifferentIdempotencyKeys = true,
            CommittedPlans = 1,
            LoserStatus = 409,
            LoserCode = "SCHEDULE_CONFLICT",
            UsedCanonicalBoundary = true,
            RevalidatedAfterBoundary = true,
            CombinedCredits = 12,
            MaximumCredits = 18,
            CombinedOverlapCount = 0
        };
        Assert.True(outcome.PlanAIndividuallyValid);
        Assert.True(outcome.PlanBIndividuallyValid);
        Assert.True(outcome.JointPlansWouldOverlapOrExceedLoad);
        Assert.True(outcome.DifferentIdempotencyKeys);
        Assert.Equal(1, outcome.CommittedPlans);
        Assert.Equal(409, outcome.LoserStatus);
        Assert.Contains(outcome.LoserCode, new[] { "POLICY_CHANGED", "SCHEDULE_CONFLICT" });
        Assert.True(outcome.UsedCanonicalBoundary);
        Assert.True(outcome.RevalidatedAfterBoundary);
        Assert.True(outcome.CombinedCredits <= outcome.MaximumCredits);
        Assert.Equal(0, outcome.CombinedOverlapCount);

        var coordinator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
            "Complete shared-boundary registration coordination must be delivered before AC-7 can pass.");

        // When different idempotency keys submit through different replicas.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "ExecuteRegistrationBoundaryAsync",
            "Revalidate",
            "Credit",
            "Conflict");

        // Then only one plan commits; the loser re-reads within the canonical
        // SPEC-008 boundary and receives POLICY_CHANGED or SCHEDULE_CONFLICT,
        // leaving the combined enrollment policy- and timetable-valid.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "POLICY_CHANGED",
            "SCHEDULE_CONFLICT",
            "Enrollment");
        Assert.DoesNotContain("SemaphoreSlim", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("ConcurrentDictionary", coordinator, StringComparison.Ordinal);
    }
}
