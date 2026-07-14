namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint04ContractTests
{
    [Fact]
    public void Logout_is_a_protected_antiforgery_mutation()
    {
        var endpoint = Spec007ContractAssertions.Endpoint("POST", "/api/auth/logout");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
    }

    [Fact]
    public void Logout_handler_expires_the_authentication_cookie()
    {
        var source = Spec007ContractAssertions.EndpointSource();

        Assert.Contains("SignOutAsync", source, StringComparison.Ordinal);
        Assert.Contains("/api/auth/logout", source, StringComparison.Ordinal);
    }
}
