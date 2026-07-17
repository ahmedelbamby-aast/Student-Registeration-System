using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class CatalogueAdministrationPageContributorTests
{
    [Fact]
    public void Adm_05_contribution_freezes_preview_idempotency_and_atomic_publication()
    {
        var (contract, page) = ContributorContractAssertions.Load(
            "ADM-05",
            "SPEC-009",
            "CatalogueAdministrationPage.razor",
            "/admin/catalogue",
            "spec017-adm05/1.0");

        RepositoryFiles.ContainsAll(
            contract,
            "CataloguePolicy.Manage",
            "antiforgery",
            "all-or-nothing",
            "expected rowversion",
            "signed preview",
            "dependency versions",
            "client request ID",
            "STALE_PREVIEW",
            "STALE_VERSION",
            "IDEMPOTENCY_KEY_REUSED",
            "PUBLICATION_CONFLICT",
            "Audit/storage failure rolls back");
        RepositoryFiles.ContainsAll(
            page,
            "CatalogueApi.ListProgramsAsync",
            "CatalogueApi.GetDraftAsync",
            "CatalogueApi.ValidateImportAsync",
            "CatalogueApi.SimulatePolicyAsync",
            "CatalogueApi.PublishImportAsync",
            "STALE_PREVIEW",
            "RefreshAndRevalidateAsync");
    }

    [Fact]
    public void Adm_05_excludes_dynamic_policy_partial_publish_and_generic_facades()
    {
        var contract = ContributorContractAssertions.Contract("ADM-05");
        RepositoryFiles.ContainsAll(
            contract,
            "No executable policy expression",
            "partial import publication",
            "generic Admin command/confirmation facade",
            "enrollment correction",
            "success before owner-server acceptance");
    }
}
