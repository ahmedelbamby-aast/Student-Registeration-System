namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Activation_accepts_only_the_issued_and_replacement_credentials()
    {
        var request = Spec007ContractAssertions.ContractType("ActivateStudentRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "UniversityId",
            "InitialPassword",
            "NewPassword");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "ConfirmPassword",
            "PasswordHash",
            "StudentId",
            "Role",
            "SecurityStamp");
    }

    [Fact]
    public void Activation_is_anonymous_but_antiforgery_and_rate_limited()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/student/activate");

        Spec007ContractAssertions.IsAnonymous(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Spec007ContractAssertions.RequiresNamedRateLimit(endpoint);
    }

    [Fact]
    public void Activation_contract_is_atomic_generic_and_replay_safe()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.Requirements(),
            "verify the initial hash, replace it",
            "activate the existing identity in one atomic",
            "generic safe response",
            "already-used safe result");
    }
}
