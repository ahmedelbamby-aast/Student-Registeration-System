using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec014;

public sealed class RegistrationAdministrationPageContributorTests
{
    [Fact]
    public void Adm_08_consumes_atomic_monitoring_and_reconciliation_contract()
    {
        var contract = RepositoryFiles.Read(
            "specs/014-registration-capacity-concurrency/contracts/routes/ADM-08.md");

        RepositoryFiles.ContainsAll(
            contract,
            "spec014-adm08/1.0",
            "Canonical page owner:** SPEC-017",
            "GET /api/admin/operations/metrics",
            "RegistrationRecords.Read",
            "Audit.Read",
            "Registration.Reconcile",
            "registrationPaused",
            "availabilityState: stale",
            "zero-overbooking",
            "zero-partial-commit",
            "GROUP_FULL",
            "PLAN_CHANGED",
            "POLICY_CHANGED",
            "SCHEDULE_CONFLICT",
            "safe support reference");
    }

    [Fact]
    public void Adm_08_contribution_is_read_only_and_preserves_spec017_page_ownership()
    {
        var contract = RepositoryFiles.Read(
            "specs/014-registration-capacity-concurrency/contracts/routes/ADM-08.md");
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-08.md");

        Assert.Contains("does not own or edit", contract);
        Assert.Contains("SPEC-017 remains the sole canonical page", contract);
        Assert.Contains("\"implementationOwnerSpec\": \"SPEC-017\"", design);
        Assert.Contains("has no repair, correction, enrollment, resume", contract);
        Assert.Contains("never granted to Admin", contract);
        Assert.DoesNotContain("Canonical page owner:** SPEC-014", contract);
    }
}
