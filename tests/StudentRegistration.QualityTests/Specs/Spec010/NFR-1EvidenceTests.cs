using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec010;

public sealed class NFR_1EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-010-NFR-1.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-1EvidenceTests.cs";
    private const int RequestCount = 300;
    private const double MaximumP95Milliseconds = 300d;

    [Fact]
    public async Task Three_hundred_concurrent_offering_reads_meet_the_component_p95_gate()
    {
        var store = new ReadOnlyOfferingStore(CompleteOffering());
        var service = new OfferingService(store);
        _ = await service.GetStudentDetailAsync(store.Offering.Id);

        var batch = Stopwatch.StartNew();
        var latencies = await Task.WhenAll(
            Enumerable.Range(0, RequestCount)
                .Select(_ => TimedReadAsync(service, store.Offering.Id)));
        batch.Stop();

        var ordered = latencies.Order().ToArray();
        var p95Index = (int)Math.Ceiling(ordered.Length * 0.95d) - 1;
        var p95Milliseconds = ordered[p95Index];
        var achievedRequestsPerSecond =
            RequestCount / Math.Max(batch.Elapsed.TotalSeconds, 0.001d);

        Assert.True(
            p95Milliseconds <= MaximumP95Milliseconds,
            $"Measured p95 was {p95Milliseconds:F3} ms.");
        Assert.True(
            achievedRequestsPerSecond >= RequestCount,
            $"Measured component throughput was {achievedRequestsPerSecond:F1} reads/s.");
        Assert.Equal(RequestCount + 1, store.ReadCount);
    }

    [Fact]
    public void Evidence_is_source_bound_and_does_not_claim_the_spec018_mixed_load_run()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-010 NFR-1 Offering Read Evidence",
            "300 concurrent reads",
            "300 ms p95",
            "OfferingService.GetStudentDetailAsync",
            "component-level",
            "SPEC-018",
            "ten-minute",
            "not replaced",
            "2 passed",
            "0 failed",
            $"Quality test normalized-LF SHA-256: `{SourceHash(TestPath)}`",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static async Task<double> TimedReadAsync(
        OfferingService service,
        Guid offeringId)
    {
        var stopwatch = Stopwatch.StartNew();
        var detail = await service.GetStudentDetailAsync(offeringId);
        stopwatch.Stop();

        Assert.NotNull(detail);
        Assert.Equal(2, Assert.Single(detail.Groups).Meetings.Count);
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static OfferingSnapshot CompleteOffering()
    {
        var offeringId = Id(1);
        var groupId = Id(2);
        return new(
            offeringId,
            "published",
            [
                new OfferingGroupSnapshot(
                    groupId,
                    "G01",
                    30,
                    12,
                    false,
                    "published",
                    [8],
                    [
                        new OfferingMeetingSnapshot(
                            Id(3),
                            "Lecture",
                            (int)DayOfWeek.Sunday,
                            new TimeOnly(9, 0),
                            new TimeOnly(10, 0),
                            "C-101",
                            "Main Campus",
                            [new OfferingStaffSnapshot(Id(4), "Lecturer", "Dr Lecturer")]),
                        new OfferingMeetingSnapshot(
                            Id(5),
                            "Tutorial",
                            (int)DayOfWeek.Tuesday,
                            new TimeOnly(11, 0),
                            new TimeOnly(12, 0),
                            "C-201",
                            "Main Campus",
                            [new OfferingStaffSnapshot(
                                Id(6),
                                "TeachingAssistant",
                                "TA Assistant")])
                    ])
            ]);
    }

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private static string SourceHash(string relativePath)
    {
        var source = RepositoryFiles.Read(relativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private sealed class ReadOnlyOfferingStore(OfferingSnapshot offering)
        : IOfferingStore
    {
        private int _readCount;

        public OfferingSnapshot Offering { get; } = offering;

        public int ReadCount => _readCount;

        public Task<OfferingSnapshot?> LoadAsync(
            Guid offeringId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _readCount);
            return Task.FromResult<OfferingSnapshot?>(
                offeringId == Offering.Id ? Offering : null);
        }

        public Task<OfferingGroupSnapshot?> LoadGroupAsync(
            Guid groupId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _readCount);
            return Task.FromResult<OfferingGroupSnapshot?>(
                Offering.Groups.SingleOrDefault(group => group.Id == groupId));
        }

        public Task<OfferingSnapshot> CreateAsync(
            CreateOfferingStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<OfferingGroupSnapshot> UpdateGroupAsync(
            UpdateGroupStoreCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<AdminOfferingPage> ListAsync(
            AdminOfferingQuery query,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
