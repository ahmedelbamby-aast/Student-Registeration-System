namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint06ContractTests
{
    [Fact]
    public void Recovery_completion_accepts_only_the_opaque_challenge_and_new_password()
    {
        var request = Spec007ContractAssertions.ContractType("RecoveryCompleteRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "ChallengeToken",
            "NewPassword");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "UniversityId",
            "UserName",
            "UserId",
            "PasswordHash",
            "SecurityStamp");
    }

    [Fact]
    public void Recovery_completion_is_anonymous_but_antiforgery_and_rate_limited()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/recovery/complete");

        Spec007ContractAssertions.IsAnonymous(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Spec007ContractAssertions.RequiresNamedRateLimit(endpoint);
    }

    [Fact]
    public void Recovery_completion_contract_is_single_use_generic_and_rotates_security_state()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "Responses: `204`; `400 VALIDATION_FAILED`, `PASSWORD_REJECTED`, or generic",
            "`CHALLENGE_INVALID`; `429 RATE_LIMITED`",
            "Success conditionally consumes the hashed proof",
            "rotates security state in one transaction");
    }
}
