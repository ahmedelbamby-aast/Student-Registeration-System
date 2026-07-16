using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Scheduling;

public sealed class OfferingLifecycleTests
{
    [Fact]
    public async Task Create_requires_at_least_one_group_and_persists_draft_only()
    {
        var store = new OfferingStoreFake();
        var service = new OfferingService(store);

        var rejected = await service.CreateAsync(
            new CreateOfferingCommand(Id(1), Id(2), [], "create-empty"));
        var created = await service.CreateAsync(
            new CreateOfferingCommand(
                Id(1),
                Id(2),
                [new CreateOfferingGroup("G01", 30)],
                "create-one"));

        Assert.Equal(OfferingOutcome.ValidationError, rejected.Outcome);
        Assert.Equal("GROUP_REQUIRED", rejected.ErrorCode);
        Assert.Equal(OfferingOutcome.Created, created.Outcome);
        Assert.Equal("draft", created.Offering?.State);
        Assert.All(created.Offering!.Groups, group => Assert.Equal("draft", group.State));
        Assert.Single(store.Creates);
    }

    [Fact]
    public async Task Publication_validation_enforces_activity_specific_complete_bundle()
    {
        var store = new OfferingStoreFake
        {
            Snapshot = Offering(
                Meeting("Lecture", "Lecturer"),
                Meeting("Tutorial", staffRole: null),
                Meeting("Laboratory", "TeachingAssistant"))
        };
        var service = new OfferingService(store);

        var result = await service.ValidateForPublicationAsync(store.Snapshot.Id);

        Assert.False(result.Valid);
        Assert.Equal(["MISSING_TEACHING_ASSISTANT"], result.ReasonCodes);
    }

    [Fact]
    public async Task Student_detail_keeps_Tutorial_canonical_and_exposes_full_context()
    {
        var tutorial = Meeting(
            "Tutorial",
            "TeachingAssistant",
            staffName: "TA One",
            roomCode: "C-201",
            location: "Smart Village",
            day: 2,
            start: new(10, 0),
            end: new(11, 30));
        var store = new OfferingStoreFake { Snapshot = Offering(tutorial) };
        var service = new OfferingService(store);

        var detail = await service.GetStudentDetailAsync(store.Snapshot.Id);
        var meeting = Assert.Single(Assert.Single(detail!.Groups).Meetings);

        Assert.Equal("Tutorial", meeting.ActivityType);
        Assert.Equal("Section", meeting.DisplayLabel);
        Assert.Equal("TA One", Assert.Single(meeting.Staff).Name);
        Assert.Equal("C-201", meeting.RoomCode);
        Assert.Equal("Smart Village", meeting.Location);
        Assert.Equal(2, meeting.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), meeting.StartLocal);
        Assert.Equal(new TimeOnly(11, 30), meeting.EndLocal);
    }

    [Fact]
    public async Task Complete_graph_update_advances_the_parent_group_rowversion()
    {
        var store = new OfferingStoreFake();
        var service = new OfferingService(store);

        var result = await service.UpdateGroupAsync(
            new UpdateGroupCommand(
                store.Snapshot.Groups[0].Id,
                ExpectedOfferingRowVersion: [1],
                ExpectedGroupRowVersion: [1],
                GroupCode: "G01",
                Capacity: 30,
                RegistrationPaused: false,
                Meetings: [],
                StaffAssignments: [],
                ActorId: "admin-1",
                Reason: "replace complete schedule"));

        Assert.Equal(OfferingOutcome.Updated, result.Outcome);
        Assert.Equal([2], result.GroupRowVersion);
        Assert.Single(store.Updates);
    }

    private static OfferingSnapshot Offering(params OfferingMeetingSnapshot[] meetings) =>
        FromDomain(
            new CourseOffering(
                Id(10),
                Id(1),
                Id(2),
                CourseOfferingState.Draft),
            new SectionGroup(
                Id(11),
                Id(10),
                "G01",
                30,
                0,
                SectionGroupState.Draft,
                false),
            meetings);

    private static OfferingSnapshot FromDomain(
        CourseOffering offering,
        SectionGroup group,
        IReadOnlyList<OfferingMeetingSnapshot> meetings) =>
        new(
            offering.Id,
            offering.State.ToString().ToLowerInvariant(),
            [
                new OfferingGroupSnapshot(
                    group.Id,
                    group.GroupCode,
                    group.Capacity,
                    group.EnrolledCount,
                    group.RegistrationPaused,
                    group.State.ToString().ToLowerInvariant(),
                    [1],
                    meetings)
            ]);

    private static OfferingMeetingSnapshot Meeting(
        string activity,
        string? staffRole,
        string staffName = "Staff One",
        string roomCode = "C-101",
        string location = "Main Campus",
        int day = 1,
        TimeOnly? start = null,
        TimeOnly? end = null) =>
        FromDomain(
            new MeetingSlot(
                Id(activity.GetHashCode(StringComparison.Ordinal) & 999),
                Id(11),
                Id(40),
                Enum.Parse<ActivityType>(activity),
                (DayOfWeek)day,
                start ?? new(8, 0),
                end ?? new(9, 30)),
            staffRole,
            staffName,
            roomCode,
            location);

    private static OfferingMeetingSnapshot FromDomain(
        MeetingSlot meeting,
        string? staffRole,
        string staffName,
        string roomCode,
        string location) =>
        new(
            meeting.Id,
            meeting.ActivityType.ToString(),
            (int)meeting.DayOfWeek,
            meeting.StartLocal,
            meeting.EndLocal,
            roomCode,
            location,
            staffRole is null
                ? []
                : [new OfferingStaffSnapshot(Id(20), staffRole, staffName)]);

    private static Guid Id(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{Math.Abs(value):000000000000}");

    private sealed class OfferingStoreFake : IOfferingStore
    {
        public OfferingSnapshot Snapshot { get; set; } = Offering();
        public List<CreateOfferingStoreCommand> Creates { get; } = [];
        public List<UpdateGroupStoreCommand> Updates { get; } = [];

        public Task<OfferingSnapshot?> LoadAsync(
            Guid offeringId,
            CancellationToken cancellationToken) =>
            Task.FromResult<OfferingSnapshot?>(Snapshot.Id == offeringId ? Snapshot : null);

        public Task<OfferingSnapshot> CreateAsync(
            CreateOfferingStoreCommand command,
            CancellationToken cancellationToken)
        {
            Creates.Add(command);
            Snapshot = new(
                Id(30),
                "draft",
                command.Groups.Select(
                    group => new OfferingGroupSnapshot(
                        Id(31 + Creates.Count),
                        group.Code,
                        group.Capacity,
                        0,
                        false,
                        "draft",
                        [1],
                        [])).ToArray());
            return Task.FromResult(Snapshot);
        }

        public Task<byte[]> UpdateGroupAsync(
            UpdateGroupStoreCommand command,
            CancellationToken cancellationToken)
        {
            Updates.Add(command);
            return Task.FromResult<byte[]>([2]);
        }

        public Task<AdminOfferingPage> ListAsync(
            AdminOfferingQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new AdminOfferingPage(
                    [Snapshot],
                    query.Page,
                    query.PageSize,
                    1,
                    query.Sort ?? "courseCode,id"));
    }
}
