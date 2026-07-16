using System.Reflection;
using System.Text.RegularExpressions;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec012;

public sealed class AC_4Tests
{
    private static readonly Guid ApplicationUserId = Id(1);
    private static readonly Guid StudentId = Id(2);
    private static readonly Guid TermId = Id(3);

    [Fact]
    public void Versioned_editing_contract_keeps_capacity_advisory_and_load_fixed_at_18()
    {
        var requirements = Normalized(
            "specs/012-schedule-builder-conflicts/requirements.md");
        RepositoryFiles.ContainsAll(
            requirements,
            "at most one group per course offering",
            "explicit GET, versioned PUT, and non-mutating validate contract",
            "`defaultTargetCredits=18`",
            "`maximumAllowedCredits=18`",
            "server-authored `loadReasons` with policy/source provenance",
            "no overload path or GPA-derived 12-credit maximum",
            "One active RegistrationPlan per authenticated student and term",
            "direct-object access to another student's plan returns no data",
            "capacity displayed in a plan as advisory",
            "changed, full, closed, cancelled, or unpublished group",
            "change/remove actions and blocks review",
            "reject update and return current plan");

        var approval = Normalized(
            "specs/012-schedule-builder-conflicts/checklists/approval.md");
        RepositoryFiles.ContainsAll(
            approval,
            "`spec012-credit-load/1.0`",
            "2026-07-16",
            "empty, current, replaced, validated, or authorized stale-current",
            "`defaultTargetCredits=18`",
            "`maximumAllowedCredits=18`",
            "server-authored `loadReasons`",
            "There is no overload path or GPA-derived 12-credit branch");
    }

    [Fact]
    public void Versioned_plan_service_has_read_replace_and_validate_seams()
    {
        var service = Assembly.Load("StudentRegistration.Registration")
            .GetType(
                "StudentRegistration.Registration.Application.RegistrationPlanService");
        Assert.True(
            service is not null,
            "T036 must deliver RegistrationPlanService after this acceptance test is red.");

        var methods = service!.GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        Assert.Contains(
            methods,
            method => method.Name.Contains("Read", StringComparison.OrdinalIgnoreCase)
                || method.Name.Contains("Get", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            methods,
            method => method.Name.Contains("Replace", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            methods,
            method => method.Name.Contains("Validate", StringComparison.OrdinalIgnoreCase));

        var contract = Normalized(
            "specs/012-schedule-builder-conflicts/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "complete desired selection",
            "one atomic operation",
            "`409 DUPLICATE_OFFERING_SELECTION`",
            "`409 StaleRegistrationPlanResponse`",
            "only the owner's current plan",
            "Capacity shown in `GroupDto` is advisory",
            "never reserve, decrement, allocate, or promise a seat");
    }

    [Fact]
    public async Task Student_can_add_change_remove_and_recover_from_stale_advisory_state()
    {
        var store = new PlanStore();
        var context = new PlanContext(
            Group(10, 20, "CS101", capacity: 30, enrolled: 30),
            Group(11, 21, "DS201", capacity: 30, enrolled: 10));
        var service = new RegistrationPlanService(
            new OwnerReader(),
            store,
            context,
            new ScheduleConflictDetector(),
            new FixedTimeProvider());

        var added = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(RegistrationPlanService.InitialEmptyVersion, [Id(20)]));
        Assert.Equal(RegistrationPlanOperationOutcome.Updated, added.Outcome);
        Assert.NotNull(added.Plan);
        Assert.Equal([Id(20)], added.Plan.SelectedGroups.Select(group => group.GroupId));
        Assert.Contains(added.Plan.SelectionIssues, issue => issue.Code == "GROUP_FULL");
        Assert.True(added.Plan.ReviewBlocked);
        AssertFixedLoad(added.Plan);

        var changed = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(added.Plan.RowVersion, [Id(21)]));
        Assert.Equal(RegistrationPlanOperationOutcome.Updated, changed.Outcome);
        Assert.NotNull(changed.Plan);
        Assert.Equal([Id(21)], changed.Plan.SelectedGroups.Select(group => group.GroupId));
        AssertFixedLoad(changed.Plan);

        var stale = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(added.Plan.RowVersion, [Id(20)]));
        Assert.Equal(RegistrationPlanOperationOutcome.StaleVersion, stale.Outcome);
        Assert.NotNull(stale.Plan);
        Assert.Equal(changed.Plan.RowVersion, stale.Plan.RowVersion);
        Assert.Equal([Id(21)], stale.Plan.SelectedGroups.Select(group => group.GroupId));
        AssertFixedLoad(stale.Plan);

        var cleared = await service.ReplaceAsync(
            ApplicationUserId,
            TermId,
            new(changed.Plan.RowVersion, []));
        Assert.Equal(RegistrationPlanOperationOutcome.Updated, cleared.Outcome);
        Assert.NotNull(cleared.Plan);
        Assert.Empty(cleared.Plan.SelectedGroups);
        Assert.Equal(0m, cleared.Plan.TotalCredits);
        AssertFixedLoad(cleared.Plan);

        var writesBeforeValidation = store.ReplaceCalls;
        var validated = await service.ValidateAsync(ApplicationUserId, TermId);
        Assert.Equal(RegistrationPlanOperationOutcome.Validated, validated.Outcome);
        Assert.NotNull(validated.Plan);
        Assert.Equal(writesBeforeValidation, store.ReplaceCalls);
        Assert.Equal(0, context.SeatMutationCalls);
        AssertFixedLoad(validated.Plan);
    }

    private static void AssertFixedLoad(RegistrationPlanView plan)
    {
        Assert.Equal(18m, plan.DefaultTargetCredits);
        Assert.Equal(18m, plan.MaximumAllowedCredits);
        var reason = Assert.Single(plan.LoadReasons);
        Assert.Equal(PlanContext.PolicySetId, reason.PolicySetId);
        Assert.Equal("DEMO-POC-2026.1", reason.PolicyVersion);
        Assert.Equal("DEMO-APPROVAL-2026.1", reason.SourceReference);
    }

    private static RegistrationPlanGroupSnapshot Group(
        int offering,
        int group,
        string courseCode,
        int capacity,
        int enrolled) =>
        new(
            Id(offering),
            Id(group),
            $"G{group}",
            courseCode,
            $"{courseCode} title",
            3m,
            "published",
            false,
            capacity,
            enrolled,
            $"offering/{offering}",
            $"group/{group}",
            [
                new(
                    Id(100 + group),
                    DayOfWeek.Monday,
                    new TimeOnly(8 + group % 4, 0),
                    new TimeOnly(8 + group % 4, 50),
                    "Lecture",
                    "A-101",
                    "Smart Village",
                    "Dr. Salma",
                    [],
                    "Africa/Cairo")
            ]);

    private static Guid Id(int value) =>
        Guid.Parse($"01240000-0000-0000-0000-{value:000000000000}");

    private static string Normalized(string path) =>
        Regex.Replace(RepositoryFiles.Read(path), @"\s+", " ");

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
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
                        groups.Where(group => groupIds.Contains(group.GroupId)).ToArray())
                    : null);
    }

    private sealed class PlanStore : IRegistrationPlanStore
    {
        private RegistrationPlan? _plan;
        private int _version;

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
                Id(900),
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
            return Task.FromResult(new RegistrationPlanStoreResult(
                RegistrationPlanStoreOutcome.Updated,
                _plan));
        }
    }
}
