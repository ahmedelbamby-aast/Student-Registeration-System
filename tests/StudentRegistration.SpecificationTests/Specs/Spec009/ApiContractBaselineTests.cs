using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Specs.Spec009;

public sealed class ApiContractBaselineTests
{
    [Fact]
    public void All_fourteen_owned_endpoints_have_frozen_request_authorization_and_outcome_contracts()
    {
        var contract = RepositoryFiles.Read(
            "specs/009-catalog-prerequisites-policy-admin/contracts/api.md");
        var endpoints = new[]
        {
            "GET /api/admin/programs",
            "GET /api/admin/catalogue/versions",
            "GET /api/admin/catalogue/drafts/{draftId}",
            "PUT /api/admin/catalogue/drafts/{draftId}",
            "POST /api/admin/catalogue/imports",
            "GET /api/admin/catalogue/imports/{importId}",
            "POST /api/admin/catalogue/imports/{importId}/validate",
            "POST /api/admin/catalogue/imports/{importId}/publish",
            "GET /api/admin/policies",
            "POST /api/admin/policies",
            "PUT /api/admin/policies/{policySetId}",
            "POST /api/admin/policies/{policySetId}/validate",
            "POST /api/admin/policies/{policySetId}/simulate",
            "POST /api/admin/policies/{policySetId}/publish",
        };

        foreach (var endpoint in endpoints)
        {
            Assert.Contains($"`{endpoint}`", contract, StringComparison.Ordinal);
        }

        RepositoryFiles.ContainsAll(
            contract,
            "`CataloguePolicy.Manage`",
            "Authorization occurs before",
            "same-origin antiforgery validation",
            "default page 1/size 20",
            "maximum size 100",
            "PAGE_SIZE_INVALID",
            "STALE_VERSION",
            "STALE_PREVIEW",
            "IDEMPOTENCY_KEY_REUSED",
            "UNKNOWN_RULE_TYPE",
            "RULE_VALUE_TYPE_MISMATCH",
            "PROVENANCE_REQUIRED",
            "one winner",
            "rolls back all effects");
    }

    [Fact]
    public void Import_and_simulation_contracts_keep_access_date_field_classification_and_policy_version()
    {
        var contract = RepositoryFiles.Read(
            "specs/009-catalog-prerequisites-policy-admin/contracts/api.md");

        RepositoryFiles.ContainsAll(
            contract,
            "accessedOn: string",
            "syntheticFields: string[]",
            "sourceKind: \"official-source\" | \"synthetic-demo\"",
            "policyVersion: string",
            "sourceReference: string; sourceKind:",
            "Official-",
            "source records retain the URL/access date",
            "fully local records use `synthetic-demo`");
    }
}
