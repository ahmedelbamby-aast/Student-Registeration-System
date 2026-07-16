using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Draft_detail_is_direct_object_authorized_without_denial_version_disclosure()
    {
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("CatalogueDraftDto"),
            "Id", "Scope", "BasedOnVersionId", "State", "CanonicalContentHash",
            "RowVersion", "Programs", "Courses", "Curricula");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "GET",
            "/api/admin/catalogue/drafts/{draftId}");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.CatalogueDraftDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, false);
        Spec009ContractAssertions.ContractContains(
            "Authorization occurs before draft",
            "No rowversion is disclosed on denial",
            "DRAFT_NOT_FOUND");
    }
}
