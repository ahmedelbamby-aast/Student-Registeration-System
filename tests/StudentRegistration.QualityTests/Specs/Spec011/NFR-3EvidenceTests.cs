using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.QualityTests.Specs.Spec011;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public async Task Fixed_inputs_time_and_versions_produce_identical_output_one_hundred_times()
    {
        var fixture = new Spec011ScenarioBuilder();
        var service = Spec011QualitySupport.Service(fixture);
        var baseline = Spec011QualitySupport.CanonicalJson(
            await service.EvaluateTermAsync(
                fixture.ApplicationUserId,
                fixture.TermId));

        for (var iteration = 1; iteration < 100; iteration++)
        {
            var current = Spec011QualitySupport.CanonicalJson(
                await service.EvaluateTermAsync(
                    fixture.ApplicationUserId,
                    fixture.TermId));
            Assert.Equal(baseline, current);
        }

        var result = await service.EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId);
        Assert.Equal(EligibilityEvaluationOutcome.Found, result.Outcome);
        var offering = Assert.Single(result.Items);
        Assert.Equal(fixture.EvaluatedAtUtc, offering.EvaluatedAtUtc);
        Assert.Equal("CATALOGUE-2026.1", offering.CatalogueVersion);
        Assert.Equal("DEMO-POC-2026.1", offering.PolicyVersion);
        Assert.Equal("plan/15", offering.CurrentPlanVersion);
        Assert.Equal(
            offering.Reasons.Select(reason => reason.Code),
            result.Items[0].Reasons.Select(reason => reason.Code));
    }

    [Fact]
    public void Evidence_records_fixed_time_input_versions_and_order()
    {
        var evidence = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-NFR-3.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-011 NFR-3 Determinism Evidence",
            "NFR-3",
            "100 evaluations",
            "fixed UTC instant",
            "CATALOGUE-2026.1",
            "DEMO-POC-2026.1",
            "plan/15",
            "reason order",
            "**Result: PASS.**");
    }
}
