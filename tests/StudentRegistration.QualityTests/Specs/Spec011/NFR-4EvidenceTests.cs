using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec011;

public sealed class NFR_4EvidenceTests
{
    [Fact]
    public void Stu_02_and_stu_03_statuses_have_text_icons_and_textual_reasons()
    {
        var presentation = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Registration/EligibilityPresentation.razor");
        var discovery = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SubjectDiscoveryPage.razor");
        var details = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/SubjectDetailsPage.razor");

        RepositoryFiles.ContainsAll(
            presentation,
            "Text=\"@(reason.Passed ? \"Passed\" : \"Unavailable\")\"",
            "Icon=\"@(reason.Passed ? \"✓\" : \"✕\")\"",
            "<strong>@reason.Code</strong>",
            "<p>@reason.Message</p>",
            "Text=\"@(group.Selectable ? \"Selectable\" : \"Unavailable\")\"",
            "Icon=\"@(group.Selectable ? \"✓\" : \"✕\")\"",
            "<span>@reason.Message</span>");
        RepositoryFiles.ContainsAll(
            discovery,
            "Text=\"@(offering.Eligible ? \"Eligible\" : \"Unavailable\")\"",
            "Icon=\"@(offering.Eligible ? \"✓\" : \"✕\")\"");
        RepositoryFiles.ContainsAll(
            details,
            "Text=\"@(_offering.Eligible ? \"Eligible\" : \"Unavailable\")\"",
            "Icon=\"@(_offering.Eligible ? \"✓\" : \"✕\")\"");
    }

    [Fact]
    public void Browser_evidence_checks_visible_text_icon_and_stale_reason_code()
    {
        var discoveryTests = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDiscoveryPageFeatureTests.cs");
        var detailTests = RepositoryFiles.Read(
            "tests/StudentRegistration.E2ETests/Specs/Spec011/SubjectDetailsPageFeatureTests.cs");

        RepositoryFiles.ContainsAll(
            discoveryTests,
            "GetByText(\"Eligible\"",
            "GetByText(\"✓\"",
            "GetByText(\"NORMAL_MAX_CREDITS\"");
        RepositoryFiles.ContainsAll(
            detailTests,
            "GetByText(\"Eligible\"",
            "GetByText(\"✓\"",
            "[data-reason-code='GROUP_FULL']",
            "GetByText(\"GROUP_FULL\"");
    }

    [Fact]
    public void Evidence_limits_the_claim_to_the_spec011_feature()
    {
        var evidence = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-NFR-4.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-011 NFR-4 Non-Color Status Evidence",
            "NFR-4",
            "Eligible",
            "Unavailable",
            "✓",
            "✕",
            "reason code",
            "reason message",
            "feature-level",
            "SPEC-003",
            "not a product-wide WCAG certification",
            "**Result: PASS.**");
    }
}
