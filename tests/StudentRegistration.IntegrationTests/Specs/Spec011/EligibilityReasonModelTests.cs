using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec011;

public sealed class EligibilityReasonModelTests
{
    [Fact]
    public void Reason_preserves_governed_explanation_metadata()
    {
        var reason = Reason("MINIMUM_EARNED_CREDITS_NOT_MET", false, true);

        Assert.Equal("MINIMUM_EARNED_CREDITS_NOT_MET", reason.Code);
        Assert.False(reason.Passed);
        Assert.True(reason.Blocking);
        Assert.Equal("96", reason.RequiredValue);
        Assert.Equal("95", reason.CurrentValue);
        Assert.Equal("DEMO-POC-2026.1", reason.PolicyVersion);
        Assert.Equal(new DateOnly(2026, 7, 13), reason.SourceAccessedOn);
        Assert.False(reason.OverridePossible);
        Assert.Equal("/support/registrar", reason.SupportReferencePath);
    }

    [Fact]
    public void Blocker_cannot_claim_an_override()
    {
        Assert.Throws<ArgumentException>(() => new EligibilityReason(
            "GROUP_FULL", false, true, "No seats.", null, null, Guid.NewGuid(),
            "DEMO-POC-2026.1", "DEMO-CAPACITY-FIRST-COMMIT",
            new DateOnly(2026, 7, 13), "DEMO-APPROVAL-2026.1",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc), null, true,
            "/support/registrar"));
    }

    internal static EligibilityReason Reason(
        string code,
        bool passed,
        bool blocking) =>
        new(
            code,
            passed,
            blocking,
            "A governed explanation.",
            "96",
            "95",
            Guid.NewGuid(),
            "DEMO-POC-2026.1",
            "SRC-DATA-SCIENCE",
            new DateOnly(2026, 7, 13),
            "DEMO-APPROVAL-2026.1",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            null,
            false,
            "/support/registrar");
}
