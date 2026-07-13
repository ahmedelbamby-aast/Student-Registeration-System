using System.Diagnostics;
using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.AcceptanceTests.Specs.Spec002;

public sealed class AC_5Tests
{
    [Fact]
    public void Approved_boundaries_are_deterministic_fast_and_fully_sourced()
    {
        var harness = Spec002PolicyTestHarness.Load();
        Assert.Equal(19, harness.BoundaryCases.Count);

        foreach (var boundary in harness.BoundaryCases)
        {
            var decision = harness.Evaluate(boundary.Input);
            Assert.Equal(boundary.ExpectedEligible, decision.Eligible);
            Assert.Equal(boundary.ExpectedReasonCode, decision.ReasonCode);
        }

        var fixedInput = harness.BoundaryCases.Single(item => item.Id == "PB-06").Input;
        var fingerprints = new HashSet<string>(StringComparer.Ordinal);
        var durations = new List<double>();
        for (var index = 0; index < 500; index++)
        {
            var started = Stopwatch.GetTimestamp();
            var decision = harness.Evaluate(fixedInput);
            durations.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            fingerprints.Add(decision.DeterministicFingerprint);
        }

        durations.Sort();
        var p95 = durations[(int)Math.Ceiling(durations.Count * 0.95) - 1];
        Assert.Single(fingerprints);
        Assert.True(p95 <= 100, $"Policy contract evaluation p95 was {p95:F3} ms.");
        Assert.All(
            harness.Sources,
            source =>
            {
                Assert.False(string.IsNullOrWhiteSpace(source.Url));
                Assert.NotEqual(default, source.AccessedOn);
                Assert.False(string.IsNullOrWhiteSpace(source.ApprovalActor));
                Assert.NotEqual(default, source.EffectiveFromUtc);
            });
    }
}
