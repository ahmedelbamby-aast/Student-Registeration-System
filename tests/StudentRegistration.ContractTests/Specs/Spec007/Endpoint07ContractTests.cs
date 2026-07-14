namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint07ContractTests
{
    [Fact]
    public void Password_change_requires_current_and_replacement_passwords_only()
    {
        var request = Spec007ContractAssertions.ContractType("ChangePasswordRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "CurrentPassword",
            "NewPassword");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "UserId",
            "UniversityId",
            "PasswordHash",
            "SecurityStamp");
    }

    [Fact]
    public void Password_change_is_a_protected_antiforgery_mutation()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/password/change");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
    }

    [Fact]
    public void Password_change_contract_reauthenticates_and_invalidates_previous_sessions()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "interface ChangePasswordRequest { currentPassword: string; newPassword: string; }",
            "Password change, recovery completion, and revoke-all atomically rotate",
            "Earlier cookies are rejected by every replica");
    }
}
