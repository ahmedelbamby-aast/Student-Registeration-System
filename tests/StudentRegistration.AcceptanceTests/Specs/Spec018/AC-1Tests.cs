namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_1Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-011 must deliver discovery/eligibility reads, SPEC-012 plan/timetable reads, SPEC-014 atomic submissions, SPEC-015 registration-record reads, and SPEC-018 T060-T065 must deliver the production-like fixture, exact load harness, and measured target evidence.";

    [Fact(Skip = ActivationGate)]
    public void Target_mix_meets_latency_error_and_registration_invariant_budgets()
    {
        // Given a production-like database and the exact NFR-2 traffic mix.
        // When 75 submissions/s plus 300 reads/s run for ten minutes.
        // Then latency/error budgets pass with zero overbooking, duplicates, or partial commits.
        throw new NotImplementedException(
            "A declared profile cannot replace measured downstream runtime and SQL evidence.");
    }
}
