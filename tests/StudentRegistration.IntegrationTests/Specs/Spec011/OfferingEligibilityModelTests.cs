using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Specs.Spec011;

public sealed class OfferingEligibilityModelTests
{
    [Fact]
    public void Projection_is_immutable_versioned_and_fail_closed()
    {
        var reasons = new List<EligibilityReason>
        {
            EligibilityReasonModelTests.Reason("DECISION_DATA_UNAVAILABLE", false, true)
        };
        var groups = new List<GroupSummary>();
        var input = new Dictionary<string, string> { ["standing"] = "unavailable" };

        var model = new OfferingEligibility(
            Guid.NewGuid(),
            "DS413",
            "Project I",
            3m,
            15m,
            18m,
            18m,
            18m,
            false,
            reasons,
            groups,
            input,
            new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
            "AQID",
            "CATALOGUE-2026.1",
            Guid.NewGuid(),
            "DEMO-POC-2026.1",
            "BAUG",
            "plan/15");

        reasons.Clear();
        input["standing"] = "Active";

        Assert.False(model.Eligible);
        Assert.Single(model.Reasons);
        Assert.Equal("unavailable", model.InputSummary["standing"]);
        Assert.Equal("AQID", model.AcademicContextVersion);
        Assert.Equal("CATALOGUE-2026.1", model.CatalogueVersion);
        Assert.Equal("BAUG", model.OfferingRowVersion);
        Assert.Equal("plan/15", model.CurrentPlanVersion);
    }

    [Fact]
    public void Fixed_input_preserves_reason_and_group_order()
    {
        var first = EligibilityReasonModelTests.Reason("WINDOW_OPEN", true, false);
        var second = EligibilityReasonModelTests.Reason("LOAD_ALLOWED", true, false);

        var model = new OfferingEligibility(
            Guid.NewGuid(), "DS413", "Project I", 3m, 15m, 18m, 18m, 18m,
            true, [first, second], [], new Dictionary<string, string>(),
            new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
            "academic", "catalogue", Guid.NewGuid(), "policy", "offering", "plan");

        Assert.Equal(["WINDOW_OPEN", "LOAD_ALLOWED"], model.Reasons.Select(x => x.Code));
    }
}
