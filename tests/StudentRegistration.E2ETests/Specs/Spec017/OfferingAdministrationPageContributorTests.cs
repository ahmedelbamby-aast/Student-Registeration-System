using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec017;

public sealed class OfferingAdministrationPageContributorTests
{
    [Fact]
    public void Adm_06_contribution_freezes_invariants_preview_and_stale_recovery()
    {
        var (contract, page) = ContributorContractAssertions.Load(
            "ADM-06",
            "SPEC-010",
            "OfferingAdministrationPage.razor",
            "/admin/offerings",
            "spec017-adm06/1.0");

        RepositoryFiles.ContainsAll(
            contract,
            "Offerings.Manage",
            "Capacity >= EnrolledCount",
            "offering/group/resource versions",
            "signed preview",
            "stable order",
            "STALE_PREVIEW",
            "STALE_VERSION",
            "GROUP_CHANGED",
            "RESOURCE_CONFLICT",
            "CAPACITY_BELOW_ENROLLED",
            "OFFERING_NOT_VALIDATABLE",
            "IDEMPOTENCY_KEY_REUSED");
        RepositoryFiles.ContainsAll(
            page,
            "SchedulingApi.ListOfferingsAsync",
            "SchedulingApi.UpdateGroupAsync",
            "SchedulingApi.ValidateOfferingAsync",
            "SchedulingApi.PublishOfferingAsync",
            "Capacity cannot be below enrollment",
            "STALE_PREVIEW");
    }

    [Fact]
    public void Adm_06_excludes_capacity_conflict_and_enrollment_overrides()
    {
        var contract = ContributorContractAssertions.Contract("ADM-06");
        RepositoryFiles.ContainsAll(
            contract,
            "No capacity bypass",
            "seat decrement",
            "conflict override",
            "enrollment correction",
            "Admin availability edit",
            "partial/unaudited publication");
    }
}
