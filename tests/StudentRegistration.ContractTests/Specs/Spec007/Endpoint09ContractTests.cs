namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint09ContractTests
{
    [Fact]
    public void Session_response_has_only_display_role_state_and_expiry_fields()
    {
        var response = Spec007ContractAssertions.ContractType("SessionDto");

        Spec007ContractAssertions.HasExactProperties(
            response,
            "DisplayName",
            "Roles",
            "ActiveRole",
            "SessionState",
            "ExpiresAtUtc");
        Spec007ContractAssertions.ExcludesProperties(
            response,
            "UserId",
            "UniversityId",
            "PasswordHash",
            "SecurityStamp",
            "SecurityStampVersion",
            "AccessFailedCount",
            "LockoutEndUtc",
            "RowVersion",
            "Claims");
    }

    [Fact]
    public void Session_query_is_protected_and_does_not_require_antiforgery()
    {
        var endpoint = Spec007ContractAssertions.Endpoint("GET", "/api/auth/session");

        Spec007ContractAssertions.IsProtected(endpoint);
        Assert.Null(endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Antiforgery.IAntiforgeryMetadata>());
    }

    [Fact]
    public void Session_contract_constrains_active_role_to_the_server_role_set()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "Session issuance requires exactly one effective role",
            "`SessionDto.activeRole`",
            "zero or multiple roles fail closed",
            "no public security-stamp");
    }
}
