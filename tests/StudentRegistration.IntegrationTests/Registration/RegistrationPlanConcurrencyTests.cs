using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Registration;

public sealed class RegistrationPlanConcurrencyTests
{
    private static readonly Guid ApplicationUserId = Id(0);
    private static readonly Guid StudentId = Id(1);
    private static readonly Guid OtherStudentId = Id(2);
    private static readonly Guid TermId = Id(3);

    [Fact]
    public async Task Complete_replacement_is_owner_scoped_conflict_safe_and_fixed_at_18_credits()
    {
        var context = new FakePlanContextReader(
            Candidate(10, 20, "CS101", 3m, 9, 0, 10, 30),
            Candidate(11, 21, "DS201", 3m, 10, 0, 11, 0));
        var store = new FakePlanStore();
        var service = Service(store, context);

        var result = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20), Id(21)]));

        Assert.Equal(RegistrationPlanOperationOutcome.Updated, result.Outcome);
        Assert.NotNull(result.Plan);
        Assert.Equal(6m, result.Plan.TotalCredits);
        Assert.Equal(18m, result.Plan.DefaultTargetCredits);
        Assert.Equal(18m, result.Plan.MaximumAllowedCredits);
        Assert.True(result.Plan.ReviewBlocked);
        Assert.Single(result.Plan.Conflicts);
        var reason = Assert.Single(result.Plan.LoadReasons);
        Assert.Equal("LOAD_WITHIN_MAXIMUM", reason.Code);
        Assert.False(reason.Blocking);
        Assert.Equal(FakePlanContextReader.PolicySetId, reason.PolicySetId);
        Assert.Equal("DEMO-POC-2026.1", reason.PolicyVersion);
        Assert.Equal("DEMO-APPROVAL-2026.1", reason.SourceReference);
        Assert.Equal(1, store.ReplaceCalls);
        Assert.Equal(0, context.SeatMutationCalls);
        var meeting = Assert.Single(result.Plan.SelectedGroups[0].Meetings);
        Assert.Equal("Lecture", meeting.ActivityKind);
        Assert.Equal("R101", meeting.RoomCode);
        Assert.Equal("Main campus", meeting.Location);
        Assert.Equal("Dr. Ada", meeting.LecturerName);
        Assert.Equal("Africa/Cairo", meeting.Timezone);

        Assert.Null(await store.ReadAsync(OtherStudentId, TermId));
        Assert.Null(await store.ReadAsync(StudentId, Id(99)));
    }

    [Fact]
    public async Task Duplicate_offering_and_above_18_load_are_rejected_without_mutation()
    {
        var duplicateStore = new FakePlanStore();
        var duplicateService = Service(
            duplicateStore,
            new FakePlanContextReader(
                Candidate(10, 20, "CS101", 3m, 9, 0, 10, 0),
                Candidate(10, 21, "CS101", 3m, 11, 0, 12, 0)));

        var duplicate = await duplicateService.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20), Id(21)]));

        Assert.Equal(RegistrationPlanOperationOutcome.InvalidSelection, duplicate.Outcome);
        Assert.Equal("DUPLICATE_OFFERING_SELECTION", duplicate.ErrorCode);
        Assert.Equal(0, duplicateStore.ReplaceCalls);

        var overloadStore = new FakePlanStore();
        var overloadGroups = Enumerable.Range(0, 7)
            .Select(index => Candidate(
                30 + index,
                40 + index,
                $"C{index}",
                3m,
                8 + index,
                0,
                8 + index,
                30))
            .ToArray();
        var overloadService = Service(
            overloadStore,
            new FakePlanContextReader(overloadGroups));

        var overload = await overloadService.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(
                RegistrationPlanService.InitialEmptyVersion,
                overloadGroups.Select(group => group.GroupId).ToArray()));

        Assert.Equal(RegistrationPlanOperationOutcome.InvalidSelection, overload.Outcome);
        Assert.Equal("LOAD_ABOVE_MAXIMUM", overload.ErrorCode);
        Assert.Equal(0, overloadStore.ReplaceCalls);
        Assert.NotNull(overload.Plan);
        Assert.Equal(18m, overload.Plan.MaximumAllowedCredits);
        Assert.Contains(
            overload.Plan.LoadReasons,
            reason => reason.Code == "LOAD_ABOVE_MAXIMUM" && reason.Blocking);

        var missing = await overloadService.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(99)]));
        Assert.Equal(RegistrationPlanOperationOutcome.InvalidSelection, missing.Outcome);
        Assert.Equal("GROUP_UNAVAILABLE", missing.ErrorCode);
        Assert.Null(missing.Plan);
        Assert.Equal(0, overloadStore.ReplaceCalls);
    }

    [Fact]
    public async Task Two_editors_get_one_winner_and_stale_current_plan_without_lost_update()
    {
        var store = new FakePlanStore();
        var context = new FakePlanContextReader(
            Candidate(10, 20, "CS101", 3m, 9, 0, 10, 0),
            Candidate(11, 21, "DS201", 3m, 11, 0, 12, 0));
        var service = Service(store, context);

        var first = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20)]));
        var stale = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(21)]));

        Assert.Equal(RegistrationPlanOperationOutcome.Updated, first.Outcome);
        Assert.Equal(RegistrationPlanOperationOutcome.StaleVersion, stale.Outcome);
        Assert.NotNull(first.Plan);
        Assert.NotNull(stale.Plan);
        Assert.Equal(first.Plan.RowVersion, stale.Plan.RowVersion);
        Assert.Equal([Id(20)], stale.Plan.SelectedGroups.Select(group => group.GroupId));
        Assert.Equal(1, store.SuccessfulReplaceCalls);
    }

    [Fact]
    public async Task Validation_is_non_mutating_and_changed_or_full_groups_block_review()
    {
        var store = new FakePlanStore();
        var context = new FakePlanContextReader(
            Candidate(
                10,
                20,
                "CS101",
                3m,
                9,
                0,
                10,
                0,
                capacity: 30,
                enrolled: 30));
        var service = Service(store, context);

        var saved = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20)]));
        Assert.Equal(RegistrationPlanOperationOutcome.Updated, saved.Outcome);
        var replaceCallsBeforeValidation = store.ReplaceCalls;

        var validation = await service.ValidateAsync(ApplicationUserId, TermId);

        Assert.Equal(RegistrationPlanOperationOutcome.Validated, validation.Outcome);
        Assert.NotNull(validation.Plan);
        Assert.True(validation.Plan.ReviewBlocked);
        Assert.Contains(
            validation.Plan.SelectionIssues,
            issue => issue.Code == "GROUP_FULL");
        var issue = Assert.Single(validation.Plan.SelectionIssues);
        Assert.Equal(Id(10), issue.OfferingId);
        Assert.Equal(Id(20), issue.GroupId);
        Assert.Equal("G20", issue.GroupCode);
        Assert.Contains(
            issue.Actions,
            action => action.Action == "change-group" &&
                action.Route == $"/student/subjects/{Id(10):D}");
        Assert.Equal(replaceCallsBeforeValidation, store.ReplaceCalls);
        Assert.Equal(0, context.SeatMutationCalls);
    }

    [Fact]
    public async Task Empty_get_and_validate_return_the_same_normal_empty_projection()
    {
        var service = Service(new FakePlanStore(), new FakePlanContextReader());

        var read = await service.GetAsync(ApplicationUserId, TermId);
        var validation = await service.ValidateAsync(ApplicationUserId, TermId);

        Assert.Equal(RegistrationPlanOperationOutcome.Found, read.Outcome);
        Assert.Equal(RegistrationPlanOperationOutcome.Validated, validation.Outcome);
        Assert.NotNull(read.Plan);
        Assert.NotNull(validation.Plan);
        Assert.Empty(read.Plan.SelectedGroups);
        Assert.Empty(validation.Plan.SelectedGroups);
        Assert.Equal(18m, read.Plan.DefaultTargetCredits);
        Assert.Equal(18m, validation.Plan.MaximumAllowedCredits);
        Assert.False(read.Plan.ReviewBlocked);
        Assert.False(validation.Plan.ReviewBlocked);
    }

    [Fact]
    public async Task Owner_and_replacement_validation_fail_closed_before_plan_mutation()
    {
        var store = new FakePlanStore();
        var service = Service(store, new FakePlanContextReader());

        Assert.Equal(
            RegistrationPlanOperationOutcome.NotFound,
            (await service.GetAsync(Guid.Empty, TermId)).Outcome);
        Assert.Equal(
            RegistrationPlanOperationOutcome.NotFound,
            (await service.ValidateAsync(ApplicationUserId, Guid.Empty)).Outcome);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ReplaceAsync(ApplicationUserId, TermId, null!));
        Assert.Equal(
            "PLAN_REPLACEMENT_INVALID",
            (await service.ReplaceAsync(
                ApplicationUserId,
                TermId,
                new(" ", []))).ErrorCode);
        Assert.Equal(
            "PLAN_REPLACEMENT_INVALID",
            (await service.ReplaceAsync(
                ApplicationUserId,
                TermId,
                new(RegistrationPlanService.InitialEmptyVersion, null!))).ErrorCode);
        Assert.Equal(
            "DUPLICATE_GROUP_SELECTION",
            (await service.ReplaceAsync(
                ApplicationUserId,
                TermId,
                new(RegistrationPlanService.InitialEmptyVersion, [Guid.Empty]))).ErrorCode);
        var duplicate = Id(20);
        Assert.Equal(
            "DUPLICATE_GROUP_SELECTION",
            (await service.ReplaceAsync(
                ApplicationUserId,
                TermId,
                new(RegistrationPlanService.InitialEmptyVersion, [duplicate, duplicate]))).ErrorCode);
        Assert.Equal(0, store.ReplaceCalls);
    }

    [Theory]
    [InlineData("draft", false, "GROUP_UNPUBLISHED")]
    [InlineData("closed", false, "GROUP_CLOSED")]
    [InlineData("cancelled", false, "GROUP_CANCELLED")]
    [InlineData("published", true, "REGISTRATION_PAUSED")]
    public async Task Lifecycle_issues_use_stable_codes_and_offering_scoped_actions(
        string state,
        bool paused,
        string expectedCode)
    {
        var service = Service(
            new FakePlanStore(),
            new FakePlanContextReader(Candidate(
                10, 20, "CS101", 3m, 9, 0, 10, 0,
                state: state,
                registrationPaused: paused)));

        var result = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20)]));

        Assert.Equal(RegistrationPlanOperationOutcome.Updated, result.Outcome);
        Assert.NotNull(result.Plan);
        var issue = Assert.Single(result.Plan.SelectionIssues);
        Assert.Equal(expectedCode, issue.Code);
        Assert.Contains(
            issue.Actions,
            action => action.Action == "change-group" &&
                action.Route == $"/student/subjects/{Id(10):D}");
    }

    [Fact]
    public async Task Validation_reports_group_changed_from_the_persisted_snapshot()
    {
        var context = new FakePlanContextReader(
            Candidate(10, 20, "CS101", 3m, 9, 0, 10, 0));
        var service = Service(new FakePlanStore(), context);
        var saved = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20)]));
        Assert.Equal(RegistrationPlanOperationOutcome.Updated, saved.Outcome);

        context.SetGroups(Candidate(
            10, 20, "CS101", 3m, 9, 0, 10, 0,
            groupVersion: "group/changed"));
        var validation = await service.ValidateAsync(ApplicationUserId, TermId);

        Assert.Equal(RegistrationPlanOperationOutcome.Validated, validation.Outcome);
        Assert.NotNull(validation.Plan);
        Assert.Contains(
            validation.Plan.SelectionIssues,
            issue => issue.Code == "GROUP_CHANGED" && issue.OfferingId == Id(10));
    }

    [Fact]
    public void Plan_and_conflict_guards_reject_ambiguous_or_incomplete_state()
    {
        Assert.Throws<ArgumentException>(() => new RegistrationPlan(
            Guid.Empty, StudentId, TermId, 0m, RegistrationPlanState.Draft));
        Assert.Throws<ArgumentException>(() => new RegistrationPlan(
            Id(700), Guid.Empty, TermId, 0m, RegistrationPlanState.Draft));
        Assert.Throws<ArgumentException>(() => new RegistrationPlan(
            Id(700), StudentId, Guid.Empty, 0m, RegistrationPlanState.Draft));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegistrationPlan(
            Id(700), StudentId, TermId, 0m, (RegistrationPlanState)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegistrationPlan(
            Id(700), StudentId, TermId, -1m, RegistrationPlanState.Draft));
        Assert.Throws<ArgumentException>(() => new RegistrationPlan(
            Id(700), StudentId, TermId, 0m, RegistrationPlanState.Draft, [null!]));

        var plan = new RegistrationPlan(
            Id(700),
            StudentId,
            TermId,
            0m,
            RegistrationPlanState.Draft);
        var selection = new RegistrationPlanSelection(
            Id(701), Id(10), Id(20), "offering/10", "group/20");
        var validation = new ValidationSnapshot(
            new DateTime(2026, 7, 18, 10, 0, 0, DateTimeKind.Utc),
            "academic/1",
            "policy/1",
            "catalogue/1",
            new Dictionary<Guid, string>(),
            new Dictionary<Guid, string>());

        Assert.Throws<ArgumentOutOfRangeException>(() => plan.ReplaceSelections(
            [selection], -1m, RegistrationPlanState.Draft, [], validation));
        Assert.Throws<ArgumentException>(() => plan.ReplaceSelections(
            [selection, selection], 3m, RegistrationPlanState.Draft, [], validation));
        Assert.Throws<ArgumentException>(() => plan.ReplaceSelections(
            [null!], 3m, RegistrationPlanState.Draft, [], validation));
        Assert.Throws<ArgumentException>(() => plan.ReplaceSelections(
            [selection], 3m, RegistrationPlanState.Draft, [null!], validation));

        var first = new ScheduleConflictParticipant(
            Id(20), "G20", "CS101", "Course", new TimeOnly(10, 0), new TimeOnly(12, 0));
        var second = new ScheduleConflictParticipant(
            Id(21), "G21", "DS201", "Course", new TimeOnly(9, 0), new TimeOnly(11, 0));
        Assert.Throws<ArgumentException>(() => new ScheduleConflictParticipant(
            Id(22), "G22", "AI301", "Course", new TimeOnly(10, 0), new TimeOnly(10, 0)));
        Assert.Throws<ArgumentException>(() => new ScheduleConflictParticipant(
            Id(22), null!, "AI301", "Course", new TimeOnly(10, 0), new TimeOnly(11, 0)));
        Assert.Throws<ArgumentException>(() => new ScheduleConflictAction(
            "unknown", Id(20), "Label", "/student/schedule"));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP",
            first,
            first,
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            "Conflict",
            []));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP",
            first,
            second,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(11, 0),
            "Conflict",
            []));

        ScheduleConflictAction Action(string action, Guid groupId) =>
            new(action, groupId, "Resolve", "/student/schedule");
        var completeActions = new[]
        {
            Action("change-group", first.GroupId),
            Action("remove-group", first.GroupId),
            Action("change-group", second.GroupId),
            Action("remove-group", second.GroupId)
        };
        var validConflict = new ScheduleConflict(
            "MEETING_OVERLAP",
            first,
            second,
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            "Conflict",
            completeActions);
        Assert.Equal(4, validConflict.Actions.Count);
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP", first, second, DayOfWeek.Monday,
            new TimeOnly(10, 0), new TimeOnly(11, 0), "Conflict", [null!]));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP", first, second, DayOfWeek.Monday,
            new TimeOnly(10, 0), new TimeOnly(11, 0), "Conflict",
            [completeActions[0], completeActions[0], .. completeActions[1..]]));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP", first, second, DayOfWeek.Monday,
            new TimeOnly(10, 0), new TimeOnly(11, 0), "Conflict",
            completeActions[..3]));
        Assert.Throws<ArgumentException>(() => new ScheduleConflict(
            "MEETING_OVERLAP", first, second, DayOfWeek.Monday,
            new TimeOnly(10, 0), new TimeOnly(11, 0), "Conflict",
            [.. completeActions, Action("change-group", Id(99))]));
    }

    [Fact]
    public void Conflict_detector_covers_null_non_overlap_and_both_intersection_orders()
    {
        var detector = new ScheduleConflictDetector();
        Assert.Throws<ArgumentException>(() => detector.Detect([null!]));
        var first = new SelectedScheduleGroup(
            Id(20), "G20", "CS101", "Course",
            [new(Id(800), DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(12, 0))]);
        var differentDay = new SelectedScheduleGroup(
            Id(21), "G21", "DS201", "Course",
            [new(Id(801), DayOfWeek.Tuesday, new TimeOnly(10, 0), new TimeOnly(12, 0))]);
        var before = new SelectedScheduleGroup(
            Id(22), "G22", "AI301", "Course",
            [new(Id(802), DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(10, 0))]);
        var containing = new SelectedScheduleGroup(
            Id(23), "G23", "SE401", "Course",
            [new(Id(803), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0))]);

        Assert.Empty(detector.Detect([first, differentDay, before]));
        var conflict = Assert.Single(detector.Detect([first, containing]));
        Assert.Equal(new TimeOnly(10, 0), conflict.OverlapStartLocal);
        Assert.Equal(new TimeOnly(12, 0), conflict.OverlapEndLocal);
    }

    private static RegistrationPlanService Service(
        IRegistrationPlanStore store,
        IRegistrationPlanContextReader context) =>
        new(
            new FakeOwnerReader(),
            store,
            context,
            new ScheduleConflictDetector(),
            new FixedTimeProvider());

    private static RegistrationPlanGroupSnapshot Candidate(
        int offering,
        int group,
        string courseCode,
        decimal credits,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute,
        int capacity = 30,
        int enrolled = 10,
        string state = "published",
        bool registrationPaused = false,
        string? groupVersion = null) =>
        new(
            Id(offering),
            Id(group),
            $"G{group}",
            courseCode,
            $"{courseCode} title",
            credits,
            state,
            registrationPaused,
            capacity,
            enrolled,
            $"offering/{offering}",
            groupVersion ?? $"group/{group}",
            [new(Id(100 + group), DayOfWeek.Monday,
                new TimeOnly(startHour, startMinute),
                new TimeOnly(endHour, endMinute),
                "Lecture",
                "R101",
                "Main campus",
                "Dr. Ada",
                [],
                "Africa/Cairo")]);

    private static Guid Id(int value) =>
        Guid.Parse($"01200000-0000-0000-0000-{value:000000000000}");

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeOwnerReader : IRegistrationPlanOwnerReader
    {
        public Task<Guid?> ResolveStudentIdAsync(
            Guid applicationUserId,
            Guid termId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(
                applicationUserId == ApplicationUserId && termId == TermId
                    ? StudentId
                    : null);
    }

    private sealed class FakePlanContextReader(
        params RegistrationPlanGroupSnapshot[] initialGroups)
        : IRegistrationPlanContextReader
    {
        private RegistrationPlanGroupSnapshot[] _groups = initialGroups;

        public static Guid PolicySetId { get; } = Id(500);

        public int SeatMutationCalls { get; private set; }

        public Task<RegistrationPlanContextSnapshot?> ReadAsync(
            Guid studentId,
            Guid termId,
            IReadOnlyCollection<Guid> groupIds,
            DateTime evaluatedAtUtc,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<RegistrationPlanContextSnapshot?>(
                studentId == StudentId && termId == TermId
                    ? new(
                        "academic/1",
                        PolicySetId,
                        "DEMO-POC-2026.1",
                        "DEMO-APPROVAL-2026.1",
                        "catalogue/1",
                        _groups.Where(group => groupIds.Contains(group.GroupId)).ToArray())
                    : null);

        public void SetGroups(params RegistrationPlanGroupSnapshot[] groups) =>
            _groups = groups;
    }

    private sealed class FakePlanStore : IRegistrationPlanStore
    {
        private readonly Lock _gate = new();
        private RegistrationPlan? _plan;
        private int _version;

        public int ReplaceCalls { get; private set; }

        public int SuccessfulReplaceCalls { get; private set; }

        public Task<RegistrationPlan?> ReadAsync(
            Guid studentId,
            Guid termId,
            CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                return Task.FromResult(
                    _plan is not null &&
                    _plan.StudentId == studentId &&
                    _plan.TermId == termId
                        ? _plan
                        : null);
            }
        }

        public Task<RegistrationPlanStoreResult> ReplaceAsync(
            RegistrationPlanStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                ReplaceCalls++;
                var currentVersion = _plan is null
                    ? RegistrationPlanService.InitialEmptyVersion
                    : Convert.ToBase64String(_plan.Version);
                if (!string.Equals(
                    command.ExpectedRowVersion,
                    currentVersion,
                    StringComparison.Ordinal))
                {
                    return Task.FromResult(new RegistrationPlanStoreResult(
                        RegistrationPlanStoreOutcome.StaleVersion,
                        _plan));
                }

                _plan ??= new(
                    Guid.NewGuid(),
                    command.StudentId,
                    command.TermId,
                    0m,
                    RegistrationPlanState.Draft);
                _plan.ReplaceSelections(
                    command.Selections,
                    command.TotalCredits,
                    command.State,
                    command.Conflicts,
                    command.Validation);
                _version++;
                typeof(RegistrationPlan)
                    .GetProperty(nameof(RegistrationPlan.Version))!
                    .SetValue(_plan, BitConverter.GetBytes(_version));
                SuccessfulReplaceCalls++;
                return Task.FromResult(new RegistrationPlanStoreResult(
                    RegistrationPlanStoreOutcome.Updated,
                    _plan));
            }
        }
    }
}
