namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint08ContractTests
{
    [Fact]
    public void Revoke_all_is_a_protected_antiforgery_mutation_with_no_client_security_token()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/sessions/revoke-all");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
    }

    [Fact]
    public void Revoke_all_contract_rotates_shared_state_and_invalidates_both_replicas()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "Success rotates shared security state",
            "replica becomes invalid",
            "Earlier cookies are rejected by every replica",
            "sticky sessions and in-memory-only security state");
    }
}
