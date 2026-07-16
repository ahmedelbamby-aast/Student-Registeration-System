using System.Reflection;
using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec012;

public sealed class ScopeEvidenceTests
{
    private static readonly Guid ApplicationUserId = Id(1);
    private static readonly Guid StudentId = Id(2);
    private static readonly Guid TermId = Id(3);

    [Fact]
    public async Task Delivered_service_keeps_schedule_and_seat_boundaries_read_only()
    {
        var store = new RecordingPlanStore();
        var context = new PlanContext(
            Group(10, 20, "CS101", 3m, 10, 0, 11, 30),
            Group(11, 21, "DS201", 3m, 11, 0, 12, 0));
        var service = Service(store, context);

        var replaced = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20), Id(21)]));

        Assert.Equal(RegistrationPlanOperationOutcome.Updated, replaced.Outcome);
        var plan = Assert.IsType<RegistrationPlanView>(replaced.Plan);
        var conflict = Assert.Single(plan.Conflicts);
        Assert.Equal("MEETING_OVERLAP", conflict.Code);
        Assert.True(plan.ReviewBlocked);
        Assert.Equal(
            ["change-group", "remove-group"],
            conflict.Actions.Select(action => action.Action)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal));
        Assert.Equal(1, store.ReplaceCalls);

        var mapped = ConflictStateMapper.Map([Presentation(conflict)], []);
        Assert.Empty(Assert.Single(mapped.Conflicts).Panel.Alternatives);
        Assert.DoesNotContain(
            typeof(RegistrationPlanView).GetProperties(),
            property => property.Name.Contains("alternative", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(RegistrationPlanDto).GetProperties(),
            property => property.Name.Contains("alternative", StringComparison.OrdinalIgnoreCase));

        var writesBeforeValidation = store.ReplaceCalls;
        var validated = await service.ValidateAsync(ApplicationUserId, TermId);

        Assert.Equal(RegistrationPlanOperationOutcome.Validated, validated.Outcome);
        Assert.Equal(writesBeforeValidation, store.ReplaceCalls);
        Assert.Equal(3, context.ReadCalls);

        Assert.Equal(
            ["ReadAsync"],
            typeof(IRegistrationPlanContextReader).GetMethods()
                .Select(method => method.Name)
                .Distinct(StringComparer.Ordinal));
        Assert.Equal(
            [
                "Conflicts", "ExpectedRowVersion", "Selections", "State",
                "StudentId", "TermId", "TotalCredits", "Validation"
            ],
            typeof(RegistrationPlanStoreCommand).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Fixed_18_maximum_rejects_excess_instead_of_opening_an_overload_path()
    {
        var groups = Enumerable.Range(0, 7)
            .Select(index => Group(
                100 + index,
                200 + index,
                $"C{index + 1:000}",
                3m,
                8 + index,
                0,
                8 + index,
                50))
            .ToArray();
        var store = new RecordingPlanStore();
        var service = Service(store, new PlanContext(groups));

        var result = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(
                RegistrationPlanService.InitialEmptyVersion,
                groups.Select(group => group.GroupId).ToArray()));

        Assert.Equal(RegistrationPlanOperationOutcome.InvalidSelection, result.Outcome);
        Assert.Equal("LOAD_ABOVE_MAXIMUM", result.ErrorCode);
        var plan = Assert.IsType<RegistrationPlanView>(result.Plan);
        Assert.Equal(18m, plan.DefaultTargetCredits);
        Assert.Equal(18m, plan.MaximumAllowedCredits);
        Assert.True(Assert.Single(plan.LoadReasons).Blocking);
        Assert.Equal(0, store.ReplaceCalls);
        Assert.DoesNotContain(
            typeof(RegistrationPlanService).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            method => method.Name.Contains("overload", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Delivered_api_and_page_expose_only_plan_edit_and_manual_resolution()
    {
        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs");
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor");

        RepositoryFiles.ContainsAll(
            endpoints,
            "MapGet",
            "MapPut",
            "MapPost",
            "/api/student/terms/{termId}/registration-plan",
            "/api/student/terms/{termId}/registration-plan/validate",
            "RegistrationPlanService");
        RepositoryFiles.ContainsAll(
            page,
            "GetRegistrationPlanAsync",
            "ReplaceRegistrationPlanAsync",
            "ValidateRegistrationPlanAsync",
            "change-group",
            "remove-group",
            "DefaultTargetCredits",
            "MaximumAllowedCredits");
        Assert.DoesNotContain("SchedulingApiClient", page, StringComparison.Ordinal);
        Assert.DoesNotContain("GPA", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("overload", page, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evidence_binds_every_exclusion_to_delivered_behavior()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-012-scope-review.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "RegistrationPlanService",
            "Spec012Endpoints",
            "ScheduleBuilderPage",
            "IRegistrationPlanContextReader",
            "RegistrationPlanStoreCommand",
            "non-mutating validation",
            "change-group",
            "remove-group",
            "no seat allocation",
            "ConflictStateMapper",
            "empty alternatives collection",
            "TRAVEL_BUFFER",
            "18-credit maximum",
            "no GPA-derived branch",
            "ScopeEvidenceTests");
    }

    private static RegistrationPlanService Service(
        IRegistrationPlanStore store,
        IRegistrationPlanContextReader context) =>
        new(
            new OwnerReader(),
            store,
            context,
            new ScheduleConflictDetector(),
            new FixedTimeProvider());

    private static RegistrationPlanGroupSnapshot Group(
        int offering,
        int group,
        string courseCode,
        decimal credits,
        int startHour,
        int startMinute,
        int endHour,
        int endMinute) =>
        new(
            Id(offering),
            Id(group),
            $"G{group}",
            courseCode,
            $"{courseCode} title",
            credits,
            "published",
            false,
            30,
            10,
            $"offering/{offering}",
            $"group/{group}",
            [
                new(
                    Id(500 + group),
                    DayOfWeek.Monday,
                    new TimeOnly(startHour, startMinute),
                    new TimeOnly(endHour, endMinute),
                    "Lecture",
                    "A-101",
                    "Smart Village",
                    "Dr. Salma",
                    [],
                    "Africa/Cairo")
            ]);

    private static ServerScheduleConflictPresentation Presentation(
        ScheduleConflict conflict) =>
        new(
            conflict.Code,
            Participant(conflict.First),
            Participant(conflict.Second),
            conflict.DayOfWeek,
            conflict.OverlapStartLocal,
            conflict.OverlapEndLocal,
            conflict.Message,
            conflict.Actions.Select(action =>
                new ServerConflictResolutionActionPresentation(
                    action.Action,
                    action.TargetGroupId.ToString("D"),
                    action.Label,
                    action.Route)).ToArray());

    private static ServerConflictGroupPresentation Participant(
        ScheduleConflictParticipant participant) =>
        new(
            participant.GroupId.ToString("D"),
            participant.GroupCode,
            participant.CourseCode,
            participant.SubjectTitle,
            participant.StartLocal,
            participant.EndLocal);

    private static Guid Id(int value) =>
        Guid.Parse($"01252000-0000-0000-0000-{value:000000000000}");

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 16, 9, 0, 0, TimeSpan.Zero);
    }

    private sealed class OwnerReader : IRegistrationPlanOwnerReader
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

    private sealed class PlanContext(params RegistrationPlanGroupSnapshot[] groups)
        : IRegistrationPlanContextReader
    {
        public int ReadCalls { get; private set; }

        public Task<RegistrationPlanContextSnapshot?> ReadAsync(
            Guid studentId,
            Guid termId,
            IReadOnlyCollection<Guid> groupIds,
            DateTime evaluatedAtUtc,
            CancellationToken cancellationToken = default)
        {
            ReadCalls++;
            return Task.FromResult<RegistrationPlanContextSnapshot?>(
                studentId == StudentId && termId == TermId
                    ? new(
                        "academic/1",
                        Id(800),
                        "DEMO-POC-2026.1",
                        "policy/demo-poc",
                        "catalogue/1",
                        groups.Where(group => groupIds.Contains(group.GroupId)).ToArray())
                    : null);
        }
    }

    private sealed class RecordingPlanStore : IRegistrationPlanStore
    {
        private RegistrationPlan? _plan;

        public int ReplaceCalls { get; private set; }

        public Task<RegistrationPlan?> ReadAsync(
            Guid studentId,
            Guid termId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _plan is not null
                && _plan.StudentId == studentId
                && _plan.TermId == termId
                    ? _plan
                    : null);

        public Task<RegistrationPlanStoreResult> ReplaceAsync(
            RegistrationPlanStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            ReplaceCalls++;
            _plan = new(
                Id(900),
                command.StudentId,
                command.TermId,
                command.TotalCredits,
                command.State,
                command.Conflicts,
                command.Validation);
            _plan.ReplaceSelections(
                command.Selections,
                command.TotalCredits,
                command.State,
                command.Conflicts,
                command.Validation);
            typeof(RegistrationPlan)
                .GetProperty(nameof(RegistrationPlan.Version))!
                .SetValue(_plan, BitConverter.GetBytes(1));
            return Task.FromResult(new RegistrationPlanStoreResult(
                RegistrationPlanStoreOutcome.Updated,
                _plan));
        }
    }
}
