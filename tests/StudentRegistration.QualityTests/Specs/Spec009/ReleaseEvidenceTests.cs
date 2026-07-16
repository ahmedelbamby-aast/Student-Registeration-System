using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec009;

public sealed class ReleaseEvidenceTests
{
    [Fact]
    public void Scope_review_verifies_all_four_exclusions_against_delivered_sources()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-009-scope-review.md");
        var catalogueService = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/CataloguePublicationService.cs");
        var policyService = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/PolicyAdministrationService.cs");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            evidence,
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "No live scraping",
            "No complete or official AASTMT curriculum claim",
            "No advisor override",
            "No exception workflow",
            "**Result: PASS.**");
        Assert.DoesNotContain("HttpClient", catalogueService, StringComparison.Ordinal);
        Assert.DoesNotContain("advisor override", policyService, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exception workflow", policyService, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "Complete AASTMT curriculum",
            page,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Traceability_covers_every_approved_inventory_and_delivered_surface()
    {
        var trace = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-009-traceability.md");
        foreach (var prefixAndCount in new[]
                 {
                     ("FR", 10),
                     ("NFR", 4),
                     ("AC", 7),
                     ("EC", 5),
                     ("SC", 3),
                 })
        {
            for (var number = 1; number <= prefixAndCount.Item2; number++)
            {
                Assert.Contains(
                    $"| {prefixAndCount.Item1}-{number} |",
                    trace,
                    StringComparison.Ordinal);
            }
        }

        for (var endpoint = 1; endpoint <= 14; endpoint++)
        {
            Assert.Contains(
                $"| {endpoint:00} |",
                trace,
                StringComparison.Ordinal);
        }

        foreach (var entity in new[]
                 {
                     "Program",
                     "Course",
                     "CurriculumCourse",
                     "CoursePrerequisite",
                     "PolicySet",
                     "PolicyRule",
                     "ImportBatch",
                     "CatalogueDraft",
                     "CatalogueVersion",
                 })
        {
            Assert.Contains($"`{entity}`", trace, StringComparison.Ordinal);
        }

        RepositoryFiles.ContainsAll(
            trace,
            "ADM-05",
            "CatalogueAdministrationPage.razor",
            "CatalogueModelConfiguration.cs",
            "S2CatalogueScheduling remains SPEC-010-owned",
            "**Result: PASS.**");
    }

    [Fact]
    public void Release_record_contains_all_review_perspectives_and_nonproduction_boundary()
    {
        var approval = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-009-release-approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "Registrar / policy SME",
            "Product",
            "Admin user",
            "Data / concurrency",
            "QA",
            "Security",
            "Accessibility",
            "Operations",
            "Ahmed ELbamby",
            "non-production",
            "not institutional approval",
            "S2CatalogueScheduling",
            "SPEC-010",
            "Gate B",
            "production",
            "**Result: APPROVED — NON-PRODUCTION SPEC-009 DEMO ONLY.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            approval);
    }
}
