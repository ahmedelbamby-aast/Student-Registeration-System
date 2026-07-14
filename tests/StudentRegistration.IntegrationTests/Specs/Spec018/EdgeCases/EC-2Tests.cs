namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_2Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-007 must deliver executable cookie/session and security-stamp state, and SPEC-018 T064 must deliver an approved two-replica load-balancer/failover fixture using the shared SQL key repository; registration-only service registration is not failover evidence.";

    [Fact(Skip = ActivationGate)]
    public void Surviving_replica_continues_authenticated_requests_after_one_instance_fails()
    {
        throw new NotImplementedException(
            "A real session routed across a failed and surviving replica is required for EC-2.");
    }
}
