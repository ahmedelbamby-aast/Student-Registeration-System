using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec003;

public sealed class AC_12Tests
{
    [Fact]
    public void Representative_usability_is_passed_or_explicitly_waived_for_the_non_production_demo()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-003-NFR-8-results.json"));
        var evidence = document.RootElement;

        Assert.Equal("WAIVED-DEMO", evidence.GetProperty("result").GetString());
        Assert.Equal("non-production design-capability demo", evidence.GetProperty("scope").GetString());
        Assert.False(evidence.GetProperty("productionGoLiveApproved").GetBoolean());
        Assert.False(evidence.GetProperty("humanCohortPerformed").GetBoolean());
        Assert.Equal(17, evidence.GetProperty("requiredUniqueParticipants").GetInt32());
        Assert.Equal(0, evidence.GetProperty("recordedUniqueParticipants").GetInt32());
        Assert.False(evidence.GetProperty("completionRateCalculated").GetBoolean());
        Assert.True(evidence.GetProperty("automatedAccessibilityGatePassed").GetBoolean());
        Assert.False(evidence.GetProperty("automatedPersonaEvidenceIsHumanEvidence").GetBoolean());

        var waiver = evidence.GetProperty("waiver");
        Assert.Equal("Ahmed ELbamby", waiver.GetProperty("approvedBy").GetString());
        Assert.Equal("2026-07-21", waiver.GetProperty("approvedOn").GetString());
        Assert.Contains("non-production demo", waiver.GetProperty("reason").GetString(), StringComparison.OrdinalIgnoreCase);
    }
}
