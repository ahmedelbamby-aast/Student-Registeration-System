using System.Diagnostics;
using StudentRegistration.TestSupport.Spec002;
using Xunit.Abstractions;

namespace StudentRegistration.QualityTests.Specs.Spec002;

public sealed class NFR_3EvidenceTests(ITestOutputHelper output)
{
    private const int WarmupIterations = 512;
    private const int MeasuredIterations = 10_000;
    private const double MaximumP95Milliseconds = 100;

    [Fact]
    public void Preloaded_policy_evaluation_completes_within_the_100_millisecond_p95_budget()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var inputs = harness.BoundaryCases.Select(fixture => fixture.Input).ToArray();
        Assert.NotEmpty(inputs);

        for (var iteration = 0; iteration < WarmupIterations; iteration++)
        {
            GC.KeepAlive(harness.Evaluate(inputs[iteration % inputs.Length]));
        }

        var samples = new double[MeasuredIterations];
        for (var iteration = 0; iteration < MeasuredIterations; iteration++)
        {
            var started = Stopwatch.GetTimestamp();
            var decision = harness.Evaluate(inputs[iteration % inputs.Length]);
            var elapsed = Stopwatch.GetTimestamp() - started;

            GC.KeepAlive(decision);
            samples[iteration] = elapsed * 1000d / Stopwatch.Frequency;
        }

        Array.Sort(samples);
        var p50 = Percentile(samples, 0.50);
        var p95 = Percentile(samples, 0.95);
        var maximum = samples[^1];

        output.WriteLine(
            "Warmup={0}; measured={1}; p50={2:F6} ms; p95={3:F6} ms; max={4:F6} ms; threshold={5:F0} ms.",
            WarmupIterations,
            MeasuredIterations,
            p50,
            p95,
            maximum,
            MaximumP95Milliseconds);

        Assert.True(
            p95 <= MaximumP95Milliseconds,
            $"Measured p95 {p95:F6} ms exceeded {MaximumP95Milliseconds:F0} ms.");
    }

    private static double Percentile(IReadOnlyList<double> sortedSamples, double percentile)
    {
        var index = (int)Math.Ceiling(percentile * sortedSamples.Count) - 1;
        return sortedSamples[Math.Clamp(index, 0, sortedSamples.Count - 1)];
    }
}
