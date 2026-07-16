using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec010;

public sealed class ReleaseApprovalEvidenceTests
{
    [Fact]
    public void Release_record_contains_every_required_perspective_and_boundary()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-010-release-approval.md");

        RepositoryFiles.ContainsAll(
            approval,
            "Admin user",
            "Staff representative",
            "Data / concurrency",
            "QA",
            "Security",
            "Accessibility",
            "Operations",
            "Ahmed ELbamby",
            "Gate A",
            "non-production",
            "not institutional approval",
            "SPEC-011",
            "SPEC-016",
            "SPEC-017",
            "SPEC-018",
            "Gate B",
            "Gate C",
            "Gate D",
            "**Result: APPROVED — NON-PRODUCTION SPEC-010 DEMO ONLY.**");

        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            approval);
    }
}
