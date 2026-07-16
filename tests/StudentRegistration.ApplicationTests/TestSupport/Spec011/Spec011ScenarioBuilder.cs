using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.TestSupport.Spec011;

// Shared only by Spec 011 test projects through explicit compile links.
public sealed class Spec011ScenarioBuilder
{
    public Guid ApplicationUserId { get; } =
        Guid.Parse("01100000-0000-0000-0000-000000000001");

    public Guid StudentId { get; } =
        Guid.Parse("01100000-0000-0000-0000-000000000002");

    public Guid TermId { get; } =
        Guid.Parse("01100000-0000-0000-0000-000000000003");

    public Guid CourseId { get; } =
        Guid.Parse("01100000-0000-0000-0000-000000000004");

    public Guid OfferingId { get; } =
        Guid.Parse("01100000-0000-0000-0000-000000000005");

    public Guid GroupId { get; } =
        Guid.Parse("01100000-0000-0000-0000-000000000006");

    public DateTime EvaluatedAtUtc { get; } =
        new(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc);

    public decimal Gpa { get; set; } = 3.2m;

    public decimal EarnedCredits { get; set; } = 96m;

    public string Standing { get; set; } = "Active";

    public bool WindowOpen { get; set; } = true;

    public bool BlockingHold { get; set; }

    public bool PrerequisitePassed { get; set; } = true;

    public bool CurrentCoursePassed { get; set; }

    public decimal CurrentPlanCredits { get; set; } = 15m;

    public string CurrentPlanVersion { get; set; } = "plan/15";

    public int Capacity { get; set; } = 30;

    public int EnrolledCount { get; set; } = 12;

    public bool RegistrationPaused { get; set; }

    public string GroupState { get; set; } = "published";

    public bool RoomAvailable { get; set; } = true;

    public bool CompleteStaffing { get; set; } = true;

    public bool Conflict { get; set; }

    public bool CandidateOfferingAlreadySelected { get; set; }

    public byte OfferingRowVersionValue { get; set; } = 5;

    public byte GroupRowVersionValue { get; set; } = 6;

    public bool IncludeAcademicSnapshot { get; set; } = true;

    public bool IncludePolicy { get; set; } = true;

    public bool IncludeCatalogue { get; set; } = true;

    public string CourseCode { get; set; } = "DS413";

    public string CourseTitle { get; set; } = "Project I";

    public decimal CourseCredits { get; set; } = 3m;

    public decimal? MinimumGpa { get; set; } = 2m;

    public decimal? MinimumEarnedCredits { get; set; } = 96m;

    public PassiveAcademicReader AcademicReader() =>
        new(IncludeAcademicSnapshot ? AcademicSnapshot() : null);

    public PassiveOfferingReader OfferingReader(params EligibilityOfferingSnapshot[]? offerings) =>
        new(offerings is { Length: > 0 } ? offerings : [OfferingSnapshot()]);

    public PassiveCurrentPlanReader CurrentPlanReader() =>
        new(CurrentPlanSnapshot());

    public EligibilityAcademicSnapshot AcademicSnapshot()
    {
        var transcript = new List<EligibilityTranscriptSnapshot>();
        if (PrerequisitePassed)
        {
            transcript.Add(new("DS312", "passed", true));
        }

        if (CurrentCoursePassed)
        {
            transcript.Add(new(CourseCode, "passed", true));
        }

        return new EligibilityAcademicSnapshot(
            StudentId,
            TermId,
            "DATA-SCIENCE",
            "2023",
            Gpa,
            EarnedCredits,
            Standing,
            Bytes(1),
            new EligibilityRegistrationWindowSnapshot(WindowOpen, Bytes(2)),
            BlockingHold
                ? [new EligibilityHoldSnapshot(
                    "REG-HOLD",
                    "Registration is blocked.",
                    true,
                    "DEMO-APPROVAL-2026.1")]
                : [],
            transcript,
            IncludeCatalogue ? CatalogueSnapshot() : null,
            IncludePolicy ? PolicySnapshot() : null);
    }

    public EligibilityCatalogueSnapshot CatalogueSnapshot() =>
        new(
            Guid.Parse("01100000-0000-0000-0000-000000000010"),
            "CATALOGUE-2026.1",
            Bytes(3),
            [
                new EligibilityCourseSnapshot(
                    CourseId,
                    CourseCode,
                    CourseTitle,
                    CourseCredits,
                    ["DS312"],
                    MinimumGpa,
                    MinimumEarnedCredits,
                    "SRC-DATA-SCIENCE",
                    new DateOnly(2026, 7, 13))
            ]);

    public EligibilityPolicySnapshot PolicySnapshot() =>
        new(
            Guid.Parse("01100000-0000-0000-0000-000000000011"),
            "DEMO-POC-2026.1",
            new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            null,
            "Ahmed ELbamby",
            "DEMO-APPROVAL-2026.1",
            Bytes(4),
            RuleTypes.Select((type, index) =>
                new EligibilityPolicyRuleSnapshot(
                    type,
                    ReasonCode(type),
                    type is "Prerequisite" ? "SRC-DATA-SCIENCE" : "DEMO-APPROVAL-2026.1",
                    new DateOnly(2026, 7, 13),
                    index.ToString(System.Globalization.CultureInfo.InvariantCulture)))
                .ToArray());

    public EligibilityOfferingSnapshot OfferingSnapshot(
        Guid? offeringId = null,
        Guid? groupId = null,
        string? courseCode = null,
        decimal? credits = null,
        DayOfWeek day = DayOfWeek.Monday) =>
        new(
            offeringId ?? OfferingId,
            TermId,
            CourseId,
            "published",
            Bytes(OfferingRowVersionValue),
            [
                GroupSnapshot(groupId, day)
            ]);

    public EligibilityGroupSnapshot GroupSnapshot(
        Guid? groupId = null,
        DayOfWeek day = DayOfWeek.Monday)
    {
        var lectureStaff = CompleteStaffing
            ? [new EligibilityMeetingStaffSnapshot(
                Guid.Parse("01100000-0000-0000-0000-000000000020"),
                "Lecturer",
                "Dr. Ada")]
            : Array.Empty<EligibilityMeetingStaffSnapshot>();
        var tutorialStaff = CompleteStaffing
            ? [new EligibilityMeetingStaffSnapshot(
                Guid.Parse("01100000-0000-0000-0000-000000000021"),
                "TeachingAssistant",
                "Eng. Noor")]
            : Array.Empty<EligibilityMeetingStaffSnapshot>();

        return new EligibilityGroupSnapshot(
            groupId ?? GroupId,
            "G1",
            GroupState,
            RegistrationPaused,
            Capacity,
            EnrolledCount,
            Bytes(GroupRowVersionValue),
            [
                new EligibilityMeetingSnapshot(
                    Guid.Parse("01100000-0000-0000-0000-000000000030"),
                    "Lecture",
                    day,
                    new TimeOnly(9, 0),
                    new TimeOnly(10, 0),
                    "R101",
                    "Main",
                    RoomAvailable,
                    lectureStaff),
                new EligibilityMeetingSnapshot(
                    Guid.Parse("01100000-0000-0000-0000-000000000031"),
                    "Tutorial",
                    day,
                    new TimeOnly(10, 0),
                    new TimeOnly(11, 0),
                    "L201",
                    "Labs",
                    RoomAvailable,
                    tutorialStaff)
            ]);
    }

    public CurrentPlanSnapshot CurrentPlanSnapshot()
    {
        if (CandidateOfferingAlreadySelected)
        {
            return new(
                CurrentPlanCredits,
                [new CurrentPlanSelectionSnapshot(
                    OfferingId,
                    Guid.Parse("01100000-0000-0000-0000-000000000007"),
                    [
                        new(
                            DayOfWeek.Monday,
                            new TimeOnly(9, 0),
                            new TimeOnly(10, 0)),
                        new(
                            DayOfWeek.Monday,
                            new TimeOnly(10, 0),
                            new TimeOnly(11, 0))
                    ])],
                CurrentPlanVersion);
        }

        return new(
            CurrentPlanCredits,
            Conflict
                ? [new CurrentPlanSelectionSnapshot(
                    Guid.Parse("01100000-0000-0000-0000-000000000008"),
                    Guid.Parse("01100000-0000-0000-0000-000000000009"),
                    [new CurrentPlanMeetingSnapshot(
                        DayOfWeek.Monday,
                        new TimeOnly(9, 30),
                        new TimeOnly(10, 30))])]
                : [],
            CurrentPlanVersion);
    }

    public static byte[] Bytes(byte value) => [value, value, value, value];

    private static readonly string[] RuleTypes =
    [
        "RegistrationWindow",
        "AcademicStanding",
        "BlockingHold",
        "Prerequisite",
        "CreditLoad",
        "ProbationLoad",
        "RepeatEligibility",
        "Capacity",
        "MeetingConflict"
    ];

    private static string ReasonCode(string type) => type switch
    {
        "RegistrationWindow" => "REGISTRATION_WINDOW_CLOSED",
        "AcademicStanding" => "ACADEMIC_STANDING_UNAVAILABLE",
        "BlockingHold" => "REGISTRATION_HOLD",
        "Prerequisite" => "PREREQUISITE_NOT_COMPLETED",
        "CreditLoad" => "LOAD_ABOVE_NORMAL_MAXIMUM",
        "ProbationLoad" => "PROBATION_LOAD_EXCEEDED",
        "RepeatEligibility" => "REPEAT_POLICY_UNAVAILABLE",
        "Capacity" => "GROUP_FULL",
        "MeetingConflict" => "MEETING_CONFLICT",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}

public sealed class PassiveAcademicReader(EligibilityAcademicSnapshot? snapshot)
    : IEligibilityAcademicReader
{
    public int Calls { get; private set; }

    public Task<EligibilityAcademicSnapshot?> ReadAsync(
        Guid applicationUserId,
        Guid termId,
        IReadOnlyCollection<Guid> courseIds,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(snapshot);
    }
}

public sealed class PassiveOfferingReader(
    IReadOnlyList<EligibilityOfferingSnapshot> offerings)
    : IEligibilityOfferingReader
{
    public int ListCalls { get; private set; }

    public int FindCalls { get; private set; }

    public Task<IReadOnlyList<EligibilityOfferingSnapshot>> ListForTermAsync(
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        ListCalls++;
        return Task.FromResult<IReadOnlyList<EligibilityOfferingSnapshot>>(
            offerings.Where(item => item.TermId == termId).ToArray());
    }

    public Task<EligibilityOfferingSnapshot?> FindAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default)
    {
        FindCalls++;
        return Task.FromResult(
            offerings.SingleOrDefault(item => item.OfferingId == offeringId));
    }
}

public sealed class PassiveCurrentPlanReader(CurrentPlanSnapshot snapshot)
    : ICurrentPlanReader
{
    public int Calls { get; private set; }

    public Task<CurrentPlanSnapshot> ReadAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(snapshot);
    }
}
