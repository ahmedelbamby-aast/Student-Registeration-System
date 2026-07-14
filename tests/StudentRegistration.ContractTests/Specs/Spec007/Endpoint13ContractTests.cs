using Microsoft.AspNetCore.Authorization;

namespace StudentRegistration.ContractTests.Specs.Spec007;

public sealed class Endpoint13ContractTests
{
    [Fact]
    public void Import_status_returns_only_bounded_safe_state_and_errors()
    {
        var batch = Spec007ContractAssertions.ContractType("IdentityImportBatchDto");
        var error = Spec007ContractAssertions.ContractType("IdentityImportErrorDto");

        Spec007ContractAssertions.HasExactProperties(
            batch,
            "Id",
            "Source",
            "ContentHash",
            "State",
            "RowVersion",
            "Errors");
        Spec007ContractAssertions.HasExactProperties(error, "Row", "Code", "Message");
        Spec007ContractAssertions.ExcludesProperties(
            batch,
            "Users",
            "RawRows",
            "Password",
            "GeneratedCredentials",
            "RequestedByUserId");
    }

    [Fact]
    public void Import_status_is_a_direct_object_authorized_read()
    {
        var endpoint = Spec007ContractAssertions.Endpoint(
            "GET",
            "/api/admin/users/imports/{importId}");

        Spec007ContractAssertions.IsProtected(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "IdentityManagement");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.IdentityAccess/Application/Ports/IAdminUserLifecycleStore.cs"),
            "GetImportAsync",
            "requestedByUserId");
    }
}
