namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_2Tests
{
    [Fact]
    public void Required_spike_preserves_invariants_across_two_stateless_replicas()
    {
        _ = Spec018AcceptanceEvidence.RequirePassingNfr(3);
        var load = Spec018AcceptanceEvidence.RequirePassingNfr(2);
        var invariants = Spec018AcceptanceEvidence.RequirePassingNfr(4);
        var replicas = Spec018AcceptanceEvidence.RequirePassingNfr(5);
        _ = Spec018AcceptanceEvidence.RequirePassingNfr(6);

        Spec018AcceptanceEvidence.Matches(load, @"60[- ]second");
        Spec018AcceptanceEvidence.Matches(load, @"200\s+(registration )?submissions/s");
        Spec018AcceptanceEvidence.ContainsAll(
            replicas,
            "two independently addressable API replicas");
        Spec018AcceptanceEvidence.ContainsAll(
            invariants,
            "zero overbooking",
            "zero duplicate active offering enrollment",
            "zero partial atomic submissions");
        Spec018AcceptanceEvidence.Matches(
            load,
            @"(?i)(no graceful degradation observed|graceful degradation[^\r\n]*documented)");
    }
}
