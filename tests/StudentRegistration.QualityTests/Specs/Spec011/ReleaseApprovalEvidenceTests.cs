using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec011;

public sealed class ReleaseApprovalEvidenceTests
{
    [Fact]
    public void Release_record_has_every_perspective_and_keeps_pending_gates_honest()
    {
        var approval = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-release-approval.md");

        RepositoryFiles.ContainsAll(
            approval,
            "Product",
            "Policy SME",
            "UX",
            "Backend",
            "QA",
            "Security",
            "Accessibility",
            "Operations",
            "Ahmed ELbamby",
            "non-production",
            "not institutional approval",
            "NFR-006",
            "SQL parameterization",
            "@termId",
            "@applicationUserId",
            "SPEC-003",
            "product-wide visual governance",
            "**Result: APPROVED — bounded non-production SPEC-011 demo feature.**");
        Assert.DoesNotContain(
            "APPROVED — PRODUCTION",
            approval,
            StringComparison.OrdinalIgnoreCase);
    }
}
