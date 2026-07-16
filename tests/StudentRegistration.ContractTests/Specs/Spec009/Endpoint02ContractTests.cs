using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Version_list_exposes_immutable_summaries_and_stable_filters()
    {
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("CatalogueVersionSummaryDto"),
            "Id", "Scope", "Version", "State", "Source", "PublishedAtUtc");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "GET",
            "/api/admin/catalogue/versions");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "CatalogueVersionSummaryDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, false, false);
        Spec009ContractAssertions.ContractContains(
            "Results are immutable summaries",
            "publishedAtUtc-desc,id",
            "scope/state filters");
    }
}
