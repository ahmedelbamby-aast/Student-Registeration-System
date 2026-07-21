namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint10ContractTests
{
    [Fact]
    public void Role_context_request_contract_is_removed()
    {
        var contracts = RepositoryFiles.Read("src/StudentRegistration.Contracts/Identity/AuthenticationContracts.cs");
        Assert.DoesNotContain("SelectRoleContextRequest", contracts, StringComparison.Ordinal);
    }

    [Fact]
    public void Role_context_selection_endpoint_is_removed()
    {
        var endpoints = RepositoryFiles.Read("src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");
        Assert.DoesNotContain("/api/auth/session/context", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectRoleContextAsync", endpoints, StringComparison.Ordinal);
    }

    [Fact]
    public void Runtime_requires_one_server_authorized_role()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("src/StudentRegistration.IdentityAccess/Application/StaffAuthenticationService.cs"),
            "effectiveRoles.Length != 1",
            "effectiveRoles[0]");
    }
}
