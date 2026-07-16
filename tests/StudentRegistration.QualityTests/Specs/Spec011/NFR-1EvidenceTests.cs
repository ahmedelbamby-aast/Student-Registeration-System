using System.Diagnostics;
using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec011;
using Xunit.Abstractions;

namespace StudentRegistration.QualityTests.Specs.Spec011;

public sealed class NFR_1EvidenceTests(ITestOutputHelper output)
{
    private const int RequestCount = 300;
    private const double MaximumP95Milliseconds = 300d;
    private const double MinimumReadsPerSecond = 300d;

    [Fact]
    public async Task Three_hundred_discovery_reads_meet_the_component_gate()
    {
        var fixture = new Spec011ScenarioBuilder();
        var query = Spec011QualitySupport.Search(fixture);
        var request = new OfferingSearchRequest(
            Query: null,
            Eligibility: "eligible",
            Credits: null,
            Day: null,
            Availability: "available",
            Sort: "courseCode,id",
            Page: 1,
            PageSize: 20);
        _ = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            request);

        var batch = Stopwatch.StartNew();
        var latencies = await Task.WhenAll(
            Enumerable.Range(0, RequestCount)
                .Select(_ => TimedReadAsync(query, fixture, request)));
        batch.Stop();

        var ordered = latencies.Order().ToArray();
        var p95Index = (int)Math.Ceiling(ordered.Length * 0.95d) - 1;
        var p95Milliseconds = ordered[p95Index];
        var readsPerSecond =
            RequestCount / Math.Max(batch.Elapsed.TotalSeconds, 0.001d);

        output.WriteLine(
            "SPEC011_NFR001 requests={0} elapsed_ms={1:F3} p95_ms={2:F3} reads_per_second={3:F1}",
            RequestCount,
            batch.Elapsed.TotalMilliseconds,
            p95Milliseconds,
            readsPerSecond);
        Assert.True(
            p95Milliseconds <= MaximumP95Milliseconds,
            $"Measured p95 was {p95Milliseconds:F3} ms.");
        Assert.True(
            readsPerSecond >= MinimumReadsPerSecond,
            $"Measured throughput was {readsPerSecond:F1} reads/s.");
    }

    [Fact]
    public void Evidence_records_the_component_boundary_and_spec018_handoff()
    {
        var evidence = RepositoryFiles.Read(
            $"{Spec011QualitySupport.EvidenceDirectory}/SPEC-011-NFR-1.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-011 NFR-1 Discovery Performance Evidence",
            "300 discovery reads",
            "p95 <= 300 ms",
            ">= 300 reads/s",
            "OfferingSearchQuery.SearchAsync",
            "component-level",
            "SPEC-018",
            "ten-minute mixed-load",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static async Task<double> TimedReadAsync(
        OfferingSearchQuery query,
        Spec011ScenarioBuilder fixture,
        OfferingSearchRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            request);
        stopwatch.Stop();

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        var item = Assert.Single(result.Page!.Items);
        Assert.True(item.Eligible);
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}
