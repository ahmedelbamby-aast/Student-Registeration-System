namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint05ContractTests
{
    [Fact]
    public void Recovery_request_has_one_minimal_subject_lookup_field()
    {
        var request = Spec007ContractAssertions.ContractType("RecoveryRequest");

        Spec007ContractAssertions.HasExactProperties(request, "UniversityIdOrUserName");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "ChallengeToken",
            "RecoveryProof",
            "Proof",
            "UserId",
            "Exists");
    }

    [Fact]
    public void Recovery_request_is_anonymous_antiforgery_rate_limited_and_always_accepted()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/recovery/request");

        Spec007ContractAssertions.IsAnonymous(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Spec007ContractAssertions.RequiresNamedRateLimit(endpoint);
        Spec007ContractAssertions.DeclaresResponseStatus(endpoint, 202);
    }

    [Fact]
    public void Recovery_request_contract_is_indistinguishable_and_never_returns_the_proof()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "Response: always the same empty `202`",
            "No token, delivery reference, account fact, or retry classification appears",
            "delivery port accepts the proof");
    }

    [Fact]
    public void Coarse_throttling_preserves_the_generic_accepted_recovery_boundary()
    {
        var composition = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/IdentitySecurityRegistration.cs");

        RepositoryFiles.ContainsAll(
            composition,
            "options.OnRejected",
            "/api/auth/recovery/request",
            "StatusCodes.Status202Accepted");
    }
}
