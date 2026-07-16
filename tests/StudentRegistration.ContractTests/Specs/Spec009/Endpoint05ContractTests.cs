using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint05ContractTests
{
    [Fact]
    public void Import_creation_binds_provenance_hash_manifest_actor_scope_and_idempotency()
    {
        var request = Spec009ContractAssertions.ContractType("CreateImportRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "DraftId", "Source", "AccessedOn", "ContentHash",
            "SyntheticFields", "ClientRequestId");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "POST",
            "/api/admin/catalogue/imports");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Academics.ImportBatchDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "server-verified content hash",
            "explicit synthetic-field manifest",
            "authenticated actor plus draft scope",
            "IDEMPOTENCY_KEY_REUSED");
    }
}
