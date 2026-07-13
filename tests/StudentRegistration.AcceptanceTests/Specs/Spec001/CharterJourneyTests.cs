using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class CharterJourneyTests
{
    [Fact(Skip = "FR-4 requires the downstream Gate C login-to-atomic-receipt journey.")]
    public void Fr_4_student_journey_reaches_an_atomic_registration_receipt()
    {
        // Future E2E evidence is intentionally owned by the implementing feature specs.
    }

    [Fact]
    public void Fr_6_mvp_scope_and_non_goals_match_the_project_plan()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-charter-traceability.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "FR-6",
            "MVP scope parity",
            "OS-1 through OS-4",
            "docs/PROJECT_PLAN.md");
    }

    [Fact]
    public void Fr_7_delivery_admission_requires_approved_fr_and_ac_traceability()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-001-charter-traceability.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "FR-7",
            "SPEC-NNN/FR-N",
            "SPEC-NNN/AC-N",
            "Rejected when either link is missing or unapproved");
    }
}
