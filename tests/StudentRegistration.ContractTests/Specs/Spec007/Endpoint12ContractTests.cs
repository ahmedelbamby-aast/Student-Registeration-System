using Microsoft.AspNetCore.Authorization;

namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint12ContractTests
{
    [Fact]
    public void Import_request_binds_provenance_hash_key_and_bounded_rows_without_credentials()
    {
        var request = Spec007ContractAssertions.ContractType("IdentityImportRequest");
        var row = Spec007ContractAssertions.ContractType("IdentityImportUserRequest");

        Spec007ContractAssertions.HasExactProperties(
            request,
            "Source",
            "ContentHash",
            "ClientRequestId",
            "Users");
        Spec007ContractAssertions.HasExactProperties(
            row,
            "ExternalReference",
            "Kind",
            "UniversityId",
            "UserName",
            "StaffNumber",
            "DisplayName",
            "Roles");
        Spec007ContractAssertions.ExcludesProperties(
            request,
            "Password",
            "PasswordHash",
            "InitialPassword",
            "PinCode");
        Spec007ContractAssertions.ExcludesProperties(
            row,
            "Password",
            "PasswordHash",
            "InitialPassword",
            "PinCode");
    }

    [Fact]
    public void Import_creation_is_an_antiforgery_protected_identity_admin_command()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "POST",
            "/api/admin/users/imports");

        Spec007ContractAssertions.IsProtected(endpoint);
        Spec007ContractAssertions.RequiresAntiforgery(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "IdentityManagement");
        Spec007ContractAssertions.DeclaresResponseStatus(endpoint, 202);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/AdminUserLifecycleService.cs"),
            "MaximumImportRows = 500",
            "ComputeCanonicalContentHash",
            "IdempotencyKeyReused",
            "ImportContentExists");
    }
}
