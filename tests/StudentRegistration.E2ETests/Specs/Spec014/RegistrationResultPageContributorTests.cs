using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec014;

public sealed class RegistrationResultPageContributorTests
{
    [Fact]
    public void Stu_06_consumes_atomic_result_replay_and_private_recovery_contract()
    {
        var contract = RepositoryFiles.Read(
            "specs/014-registration-capacity-concurrency/contracts/routes/STU-06.md");

        RepositoryFiles.ContainsAll(
            contract,
            "spec014-stu06/1.0",
            "Canonical page owner:** SPEC-015",
            "GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
            "GET /api/student/registrations/{submissionId}",
            "Registration.SubmitOwn",
            "RegistrationRecords.ReadOwn",
            "REGISTERED",
            "GROUP_FULL",
            "PLAN_CHANGED",
            "POLICY_CHANGED",
            "WINDOW_CLOSED",
            "SCHEDULE_CONFLICT",
            "IDEMPOTENCY_KEY_REUSED",
            "REQUEST_NOT_FOUND",
            "Retry result lookup",
            "no-partial",
            "immutable receipt snapshot");
    }

    [Fact]
    public void Stu_06_contribution_preserves_canonical_page_ownership_and_privacy()
    {
        var contract = RepositoryFiles.Read(
            "specs/014-registration-capacity-concurrency/contracts/routes/STU-06.md");
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-06.md");

        Assert.Contains("does not own or edit `RegistrationResultPage.razor`", contract);
        Assert.Contains("SPEC-015 remains the sole canonical page", contract);
        Assert.Contains("\"implementationOwnerSpec\": \"SPEC-015\"", design);
        Assert.Contains("another owner", contract);
        Assert.Contains("another term", contract);
        Assert.Contains("submission ID", contract);
        Assert.Contains("current version", contract);
        Assert.DoesNotContain("Canonical page owner:** SPEC-014", contract);
    }
}
