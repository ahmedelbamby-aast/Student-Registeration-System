using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.IntegrationTests.Specs.Spec002.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public void Unavailable_source_preserves_immutable_history_and_requires_review_and_admin_alert()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            Gpa = 1.99m,
            PlannedCredits = 13
        });
        var snapshot = harness.Capture(decision);

        var outage = harness.MarkSourceUnavailable(snapshot);

        Assert.Equal("SRC-GENERAL-2016", snapshot.Source.Id);
        Assert.Equal("Unavailable", outage.CurrentSource.Availability);
        Assert.True(outage.CurrentSource.ReviewRequired);
        Assert.True(outage.ReviewRequired);
        Assert.True(outage.AdminAlertRequired);
        Assert.False(outage.HistoricalDecisionChanged);

        Assert.Equal(snapshot, outage.HistoricalDecision);
        Assert.Equal("Recorded", outage.HistoricalDecision.Source.Availability);
        Assert.False(outage.HistoricalDecision.Source.ReviewRequired);
        Assert.Equal(snapshot.Source.Id, outage.CurrentSource.Id);
        Assert.Equal(snapshot.Source.ProvenanceKind, outage.CurrentSource.ProvenanceKind);
        Assert.Equal(snapshot.Source.Url, outage.CurrentSource.Url);
        Assert.Equal(snapshot.Source.AccessedOn, outage.CurrentSource.AccessedOn);
        Assert.Equal(snapshot.Source.ApprovalActor, outage.CurrentSource.ApprovalActor);
        Assert.Equal(snapshot.Source.EffectiveFromUtc, outage.CurrentSource.EffectiveFromUtc);
        Assert.Equal(snapshot.Source.EffectiveToUtc, outage.CurrentSource.EffectiveToUtc);
        Assert.Equal(snapshot.Source.Authority, outage.CurrentSource.Authority);
        Assert.Equal(snapshot.Source.AffectedFacts, outage.CurrentSource.AffectedFacts);
        Assert.Equal(snapshot.Source.ContentReference, outage.CurrentSource.ContentReference);
        Assert.Equal(snapshot.Source.ApprovalStatus, outage.CurrentSource.ApprovalStatus);
        Assert.Equal(
            outage.CurrentSource,
            Assert.Single(
                outage.CurrentSourceRegister,
                source => source.Id == outage.CurrentSource.Id));
        Assert.Equal(decision.DeterministicFingerprint, outage.HistoricalDecision.DeterministicFingerprint);
    }
}
