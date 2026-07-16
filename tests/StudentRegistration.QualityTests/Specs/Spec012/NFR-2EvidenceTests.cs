using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec012;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public void Fixed_input_produces_the_same_ordered_conflict_signatures()
    {
        var input = FixedInput();
        var detector = new ScheduleConflictDetector();
        var expected = detector.Detect(input).Select(Signature).ToArray();

        Assert.Equal(3, expected.Length);
        for (var repeat = 0; repeat < 50; repeat++)
        {
            IReadOnlyList<SelectedScheduleGroup> reordered = repeat % 2 == 0
                ? input.Reverse().ToArray()
                : [input[1], input[2], input[0]];
            var actual = detector.Detect(reordered).Select(Signature).ToArray();

            Assert.Equal(expected, actual);
            Assert.DoesNotContain(
                detector.Detect(reordered),
                conflict => conflict.Code == "TRAVEL_BUFFER");
        }
    }

    [Fact]
    public void Evidence_records_the_fixed_input_determinism_boundary()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-012-NFR-2.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-012 NFR-2 Deterministic Conflict Evidence",
            "fixed input",
            "50 repeated recalculations",
            "reversed and rotated group order",
            "stable conflict signatures",
            "MEETING_OVERLAP",
            "TRAVEL_BUFFER is not emitted",
            "ScheduleConflictDetector.Detect",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|PENDING|PARTIAL)\b",
            evidence);
    }

    private static IReadOnlyList<SelectedScheduleGroup> FixedInput() =>
        [
            Group(1, "A", "AI401", Meeting(11, DayOfWeek.Monday, 10, 0, 12, 0)),
            Group(2, "B", "CS402", Meeting(22, DayOfWeek.Monday, 10, 30, 11, 30)),
            Group(3, "C", "SE403", Meeting(33, DayOfWeek.Monday, 11, 0, 12, 30))
        ];

    private static string Signature(ScheduleConflict conflict) =>
        string.Join(
            "|",
            conflict.Code,
            conflict.First.GroupId,
            conflict.Second.GroupId,
            conflict.DayOfWeek,
            conflict.First.StartLocal.Ticks,
            conflict.First.EndLocal.Ticks,
            conflict.Second.StartLocal.Ticks,
            conflict.Second.EndLocal.Ticks,
            conflict.OverlapStartLocal.Ticks,
            conflict.OverlapEndLocal.Ticks,
            conflict.Message,
            string.Join(
                ",",
                conflict.Actions.Select(action =>
                    $"{action.Action}:{action.TargetGroupId}:{action.Label}:{action.Route}")));

    private static SelectedScheduleGroup Group(
        int id,
        string groupCode,
        string courseCode,
        params SelectedScheduleMeeting[] meetings) =>
        new(Id(id), groupCode, courseCode, $"{courseCode} subject", meetings);

    private static SelectedScheduleMeeting Meeting(
        int id,
        DayOfWeek day,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute) =>
        new(
            Id(id),
            day,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute));

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");
}
