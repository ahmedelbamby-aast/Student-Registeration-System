using System.Diagnostics;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;
using Xunit.Abstractions;

namespace StudentRegistration.QualityTests.Specs.Spec012;

public sealed class NFR_1EvidenceTests(ITestOutputHelper output)
{
    private const int CourseCount = 8;
    private const int SlotsPerCourse = 10;
    private const int MeasurementCount = 100;
    private const double MaximumP95Milliseconds = 200d;

    [Fact]
    public void Eight_course_ten_slot_recalculation_meets_the_p95_gate()
    {
        var groups = EightCourseFixture();
        var detector = new ScheduleConflictDetector();

        for (var warmup = 0; warmup < 10; warmup++)
        {
            Assert.Equal(280, detector.Detect(groups).Count);
        }

        var latencies = new double[MeasurementCount];
        for (var index = 0; index < latencies.Length; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            var conflicts = detector.Detect(groups);
            stopwatch.Stop();

            Assert.Equal(280, conflicts.Count);
            latencies[index] = stopwatch.Elapsed.TotalMilliseconds;
        }

        Array.Sort(latencies);
        var p95Index = (int)Math.Ceiling(latencies.Length * 0.95d) - 1;
        var p95Milliseconds = latencies[p95Index];

        output.WriteLine(
            "SPEC012_NFR001 groups={0} slots_per_group={1} samples={2} p95_ms={3:F3}",
            CourseCount,
            SlotsPerCourse,
            MeasurementCount,
            p95Milliseconds);
        Assert.True(
            p95Milliseconds <= MaximumP95Milliseconds,
            $"Measured p95 was {p95Milliseconds:F3} ms; the limit is " +
            $"{MaximumP95Milliseconds:F0} ms.");
    }

    [Fact]
    public void Evidence_records_the_measured_component_boundary()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-012-NFR-1.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-012 NFR-1 Conflict Recalculation Evidence",
            "8 selected courses/groups",
            "10 meeting slots per group",
            "100 measured recalculations",
            "p95 <= 200 ms",
            "ScheduleConflictDetector.Detect",
            "component-level",
            "SPEC-018",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }

    private static IReadOnlyList<SelectedScheduleGroup> EightCourseFixture() =>
        Enumerable.Range(1, CourseCount)
            .Select(groupNumber =>
            {
                var groupId = Id(groupNumber);
                var meetings = Enumerable.Range(0, SlotsPerCourse)
                    .Select(slotNumber =>
                    {
                        var start = new TimeOnly(8 + (slotNumber / 5), 0);
                        return new SelectedScheduleMeeting(
                            Id(1000 + (groupNumber * 100) + slotNumber),
                            (DayOfWeek)(slotNumber % 5 + 1),
                            start,
                            start.AddMinutes(45));
                    })
                    .ToArray();
                return new SelectedScheduleGroup(
                    groupId,
                    $"G{groupNumber:00}",
                    $"C{groupNumber:000}",
                    $"Course {groupNumber:000}",
                    meetings);
            })
            .ToArray();

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
