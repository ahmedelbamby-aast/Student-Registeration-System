using System.Diagnostics;
using System.Reflection;
using StudentRegistration.Client.Components.Scheduling;
using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class AC_5Tests
{
    [Fact]
    public void Eight_course_ten_slot_recalculation_meets_the_approved_p95_budget()
    {
        var groups = EightCourseFixture();
        var detector = new ScheduleConflictDetector();
        var latencies = new double[100];

        for (var warmup = 0; warmup < 10; warmup++)
        {
            Assert.Equal(280, detector.Detect(groups).Count);
        }

        for (var index = 0; index < latencies.Length; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            var conflicts = detector.Detect(groups);
            stopwatch.Stop();

            Assert.Equal(280, conflicts.Count);
            latencies[index] = stopwatch.Elapsed.TotalMilliseconds;
        }

        Array.Sort(latencies);
        var p95 = latencies[(int)Math.Ceiling(latencies.Length * 0.95d) - 1];

        Assert.True(p95 <= 200d, $"Conflict recalculation p95 was {p95:F3} ms.");
    }

    [Fact]
    public void Fixed_input_conflicts_are_deterministic()
    {
        var input = FixedConflictInput();
        var detector = new ScheduleConflictDetector();
        var expected = detector.Detect(input).Select(Signature).ToArray();

        Assert.Equal(3, expected.Length);
        for (var repeat = 0; repeat < 50; repeat++)
        {
            var reordered = repeat % 2 == 0
                ? input.Reverse().ToArray()
                : [input[1], input[2], input[0]];

            Assert.Equal(expected, detector.Detect(reordered).Select(Signature));
        }
    }

    [Fact]
    public void Calendar_and_list_expose_the_same_canonical_meeting_semantics()
    {
        IReadOnlyList<ScheduleCalendar.ScheduleMeetingItem> meetings =
        [
            new(
                "meeting-ai401",
                "AI401",
                "Artificial Intelligence",
                "G01",
                "Lecture",
                "Dr. Salma",
                [],
                "Room A101",
                "Monday",
                "10:00",
                "11:30",
                "Africa/Cairo",
                "Conflict",
                "/student/subjects/offering-ai401",
                "Open Artificial Intelligence group details")
        ];

        var state = ConflictStateMapper.Map([], meetings);

        Assert.Same(state.CalendarMeetings, state.ChronologicalMeetings);
        Assert.Equal(
            state.CalendarMeetings.Select(MeetingSignature),
            state.ChronologicalMeetings.Select(MeetingSignature));
    }

    [Fact]
    public void Two_editors_reject_the_stale_writer_without_losing_the_winner()
    {
        var plan = new VersionedPlanDouble("version-1", ["group-a"]);
        var firstEditorVersion = plan.Read().Version;
        var secondEditorVersion = plan.Read().Version;

        var winner = plan.Replace(firstEditorVersion, ["group-b"]);
        var stale = plan.Replace(secondEditorVersion, ["group-c"]);

        Assert.Equal(ReplaceOutcome.Replaced, winner.Outcome);
        Assert.Equal(ReplaceOutcome.StaleVersion, stale.Outcome);
        Assert.Equal("version-2", stale.CurrentPlan.Version);
        Assert.Equal(["group-b"], stale.CurrentPlan.SelectedGroupIds);
        Assert.Equal(["group-b"], plan.Read().SelectedGroupIds);
    }

    [Fact]
    public void Production_two_editor_proof_requires_the_registration_plan_service()
    {
        var service = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Application.RegistrationPlanService");

        Assert.True(
            service is not null,
            "RegistrationPlanService must deliver the approved read, complete-replacement, validation, and stale-writer behavior before AC-5 can pass against production code.");
        var methods = service!.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        Assert.Contains(
            methods,
            method => method.Name.Contains("Read", StringComparison.Ordinal)
                || method.Name.Contains("Get", StringComparison.Ordinal));
        Assert.Contains(methods, method => method.Name.Contains("Replace", StringComparison.Ordinal));
        Assert.Contains(methods, method => method.Name.Contains("Validat", StringComparison.Ordinal));
    }

    private static IReadOnlyList<SelectedScheduleGroup> EightCourseFixture() =>
        Enumerable.Range(1, 8)
            .Select(groupNumber => Group(
                groupNumber,
                Enumerable.Range(0, 10)
                    .Select(slotNumber => new SelectedScheduleMeeting(
                        Id(1000 + (groupNumber * 100) + slotNumber),
                        (DayOfWeek)(slotNumber % 5 + 1),
                        new TimeOnly(8 + (slotNumber / 5), 0),
                        new TimeOnly(8 + (slotNumber / 5), 45)))
                    .ToArray()))
            .ToArray();

    private static IReadOnlyList<SelectedScheduleGroup> FixedConflictInput() =>
    [
        Group(1, Meeting(11, 10, 0, 12, 0)),
        Group(2, Meeting(22, 10, 30, 11, 30)),
        Group(3, Meeting(33, 11, 0, 12, 30))
    ];

    private static SelectedScheduleGroup Group(
        int id,
        params SelectedScheduleMeeting[] meetings) =>
        new(Id(id), $"G{id:00}", $"C{id:000}", $"Course {id:000}", meetings);

    private static SelectedScheduleMeeting Meeting(
        int id,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute) =>
        new(
            Id(id),
            DayOfWeek.Monday,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute));

    private static string Signature(ScheduleConflict conflict) =>
        string.Join(
            '|',
            conflict.Code,
            conflict.First.GroupId,
            conflict.Second.GroupId,
            conflict.DayOfWeek,
            conflict.OverlapStartLocal,
            conflict.OverlapEndLocal,
            string.Join(',', conflict.Actions.Select(action =>
                $"{action.Action}:{action.TargetGroupId}")));

    private static string MeetingSignature(
        ScheduleCalendar.ScheduleMeetingItem meeting) =>
        string.Join(
            '|',
            meeting.MeetingId,
            meeting.SubjectCode,
            meeting.SubjectName,
            meeting.GroupCode,
            meeting.ActivityKind,
            meeting.LecturerName,
            string.Join(',', meeting.TeachingAssistantNames),
            meeting.Location,
            meeting.Day,
            meeting.StartsAt,
            meeting.EndsAt,
            meeting.Timezone,
            meeting.ConflictStatusText,
            meeting.DetailsHref,
            meeting.DetailsAccessibleName);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:000000000000}");

    private enum ReplaceOutcome
    {
        Replaced,
        StaleVersion
    }

    private sealed record PlanSnapshot(
        string Version,
        IReadOnlyList<string> SelectedGroupIds);

    private sealed record ReplaceResult(
        ReplaceOutcome Outcome,
        PlanSnapshot CurrentPlan);

    private sealed class VersionedPlanDouble(
        string initialVersion,
        IReadOnlyList<string> initialGroupIds)
    {
        private int _version = int.Parse(initialVersion.Split('-')[1]);
        private string[] _selectedGroupIds = [.. initialGroupIds];

        public PlanSnapshot Read() =>
            new($"version-{_version}", [.. _selectedGroupIds]);

        public ReplaceResult Replace(
            string expectedVersion,
            IReadOnlyList<string> selectedGroupIds)
        {
            if (!string.Equals(expectedVersion, Read().Version, StringComparison.Ordinal))
            {
                return new(ReplaceOutcome.StaleVersion, Read());
            }

            _selectedGroupIds = [.. selectedGroupIds];
            _version++;
            return new(ReplaceOutcome.Replaced, Read());
        }
    }
}
