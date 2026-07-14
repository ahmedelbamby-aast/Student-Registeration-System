namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Staff_login_accepts_password_credentials_and_no_claimed_role()
    {
        var request = Spec007ContractAssertions.ContractType("StaffLoginRequest");

        Spec007ContractAssertions.HasExactProperties(request, "UserName", "Password");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "Role",
            "Roles",
            "Claims",
            "SecondFactor",
            "PasswordHash");
    }

    [Fact]
    public void Staff_login_is_anonymous_but_antiforgery_and_rate_limited()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/staff/login");

        Spec007ContractAssertions.IsAnonymous(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Spec007ContractAssertions.RequiresNamedRateLimit(endpoint);
    }

    [Fact]
    public void Staff_login_contract_denies_account_state_generically_and_derives_roles_server_side()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.Requirements(),
            "password and account-state checks",
            "derive effective roles after",
            "no pre-authentication role selector",
            "Authentication errors MUST NOT reveal whether an account exists");
    }
}
