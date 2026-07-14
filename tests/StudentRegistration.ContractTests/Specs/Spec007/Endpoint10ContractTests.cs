namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint10ContractTests
{
    [Fact]
    public void Role_context_request_can_name_one_role_but_cannot_supply_claims()
    {
        var request = Spec007ContractAssertions.ContractType("SelectRoleContextRequest");

        Spec007ContractAssertions.HasExactProperties(request, "Role");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "Roles",
            "Claims",
            "Permissions",
            "UserId",
            "SecurityStamp");
    }

    [Fact]
    public void Role_context_selection_is_a_protected_antiforgery_mutation()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "PUT",
            "/api/auth/session/context");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
    }

    [Fact]
    public void Role_context_contract_requires_claim_subset_and_cookie_rotation()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "The selected role must be a currently effective server-returned assignment",
            "success rotates the cookie",
            "cannot add or union claims");
    }
}
