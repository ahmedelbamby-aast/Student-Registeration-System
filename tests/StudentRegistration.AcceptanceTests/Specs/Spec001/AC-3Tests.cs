using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec001;

public sealed class AC_3Tests
{
    [Fact]
    public void Sprint_admission_contract_requires_approved_requirement_and_acceptance_links()
    {
        // Given a proposed implementation story; when readiness is evaluated.
        var requirements = RepositoryFiles.Read(
            "specs/001-product-charter-rbac/requirements.md");
        var plan = RepositoryFiles.Read("docs/PROJECT_PLAN.md");

        // Then both stable references are mandatory and unapproved work is rejected.
        var criterion = RepositoryFiles.Section(requirements, "Acceptance Criteria");
        RepositoryFiles.ContainsAll(
            criterion,
            "AC-3: Scope traceability (FR-6, FR-7)",
            "approved SPEC-NNN/FR-N",
            "SPEC-NNN/AC-N",
            "work is rejected");
        RepositoryFiles.ContainsAll(plan, "Definition of Ready", "spec/criterion link");
    }
}
