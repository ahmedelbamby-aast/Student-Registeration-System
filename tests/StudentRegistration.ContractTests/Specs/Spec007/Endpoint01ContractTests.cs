namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Student_login_has_the_minimal_shared_request_contract()
    {
        var request = Spec007ContractAssertions.ContractType("StudentLoginRequest");

        Spec007ContractAssertions.HasExactProperties(request, "UniversityId", "Password");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "Role",
            "Roles",
            "UserId",
            "PasswordHash",
            "SecurityStamp");
    }

    [Fact]
    public void Student_login_is_anonymous_but_antiforgery_and_rate_limited()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/auth/student/login");

        Spec007ContractAssertions.IsAnonymous(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Spec007ContractAssertions.RequiresNamedRateLimit(endpoint);
    }

    [Fact]
    public void Student_login_contract_returns_only_server_derived_session_context()
    {
        RepositoryFiles.ContainsAll(
            Spec007ContractAssertions.ApiContract(),
            "Student and staff login return `200 SessionDto`",
            "roles are derived exclusively from server-side assignments",
            "All protected operations require server-validated authentication");
    }
}
