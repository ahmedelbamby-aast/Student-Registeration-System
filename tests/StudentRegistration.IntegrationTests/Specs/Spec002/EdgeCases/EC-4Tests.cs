using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.IntegrationTests.Specs.Spec002.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Official_curriculum_row_without_official_provenance_is_rejected()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var validation = harness.ValidateCatalogueRow(
            new PolicyCatalogueRow
            {
                Code = "DS413",
                CodeProvenance = new("OfficialAASTMT", null)
            });

        Assert.False(validation.Accepted);
        Assert.Contains("OFFICIAL_CURRICULUM_PROVENANCE_REQUIRED", validation.RejectionCodes);
    }

    [Fact]
    public void Official_row_with_non_synthetic_credit_provenance_is_rejected()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var validation = harness.ValidateCatalogueRow(
            new PolicyCatalogueRow
            {
                CreditsProvenance = new("OfficialAASTMT", "SRC-DATA-SCIENCE")
            });

        Assert.False(validation.Accepted);
        Assert.Contains("SYNTHETIC_CREDIT_PROVENANCE_REQUIRED", validation.RejectionCodes);
    }

    [Fact]
    public void Synthetic_gap_row_without_an_explicit_label_is_rejected()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var validation = harness.ValidateCatalogueRow(
            new PolicyCatalogueRow
            {
                Code = "DEMO-AI499",
                IsSyntheticGap = true,
                SyntheticGapLabel = null,
                SyntheticGapRationale = "Fills a documented demo curriculum gap.",
                VisibleDisclaimer = "Not AASTMT-published.",
                CodeProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
                TitleProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
                SequenceProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP"),
                PrerequisiteProvenance = new("SyntheticDemo", "DEMO-CURRICULUM-GAP")
            });

        Assert.False(validation.Accepted);
        Assert.Contains("SYNTHETIC_GAP_LABEL_REQUIRED", validation.RejectionCodes);
    }

    [Fact]
    public void Synthetic_gap_without_visible_not_published_disclaimer_is_rejected()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var validation = harness.ValidateCatalogueRow(
            PolicyCatalogueRow.ValidSyntheticGap() with { VisibleDisclaimer = null });

        Assert.False(validation.Accepted);
        Assert.Contains("SYNTHETIC_GAP_DISCLAIMER_REQUIRED", validation.RejectionCodes);
    }

    [Fact]
    public void Official_row_with_field_level_source_split_is_accepted()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var validation = harness.ValidateCatalogueRow(new PolicyCatalogueRow());

        Assert.True(validation.Accepted);
        Assert.Empty(validation.RejectionCodes);
    }
}
