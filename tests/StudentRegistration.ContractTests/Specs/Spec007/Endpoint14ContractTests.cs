using Microsoft.AspNetCore.Authorization;

namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint14ContractTests
{
    [Fact]
    public void Import_publication_has_one_aggregate_version_and_owner_scoped_key()
    {
        var request = Spec007ContractAssertions.ContractType(
            "IdentityImportPublishRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "ExpectedRowVersion",
            "ClientRequestId");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "Users",
            "ContentHash",
            "Password",
            "RoleAssignmentVersion");
    }

    [Fact]
    public void Import_publication_is_antiforgery_protected_and_transactional_by_contract()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/admin/users/imports/{importId}/publish");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "IdentityManagement");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/Ports/IAdminUserLifecycleStore.cs"),
            "PublishImportAsync",
            "all-or-nothing",
            "Same-key/same-payload");
    }

    [Fact]
    public void Provisioned_credentials_cross_only_the_prepare_complete_abort_server_port()
    {
        var response = Spec007ContractAssertions.ContractType("IdentityImportBatchDto");
        Spec007ContractAssertions.ExcludesProperties(
            response,
            "Credentials",
            "Password",
            "PinCode",
            "HandoffPath",
            "HandoffReference");

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/Ports/IProvisionedCredentialHandoff.cs"),
            "IProvisionedCredentialHandoff",
            "PrepareAsync",
            "CompleteAsync",
            "AbortAsync",
            "DemoCredential",
            "Production");
    }
}
