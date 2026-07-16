using System.Reflection;

namespace StudentRegistration.IntegrationTests.Specs.Spec012.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Stale_update_returns_only_the_current_authorized_plan_and_loses_no_update()
    {
        var studentId = Guid.NewGuid();
        var otherStudentId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var plans = new VersionedPlanStoreDouble();
        plans.Create(studentId, termId, "version-1", ["group-a"]);

        var firstEditorVersion = plans.Read(studentId, termId)!.Version;
        var secondEditorVersion = plans.Read(studentId, termId)!.Version;

        var winner = plans.Replace(
            studentId,
            termId,
            firstEditorVersion,
            ["group-b"]);
        var stale = plans.Replace(
            studentId,
            termId,
            secondEditorVersion,
            ["group-c"]);
        var otherStudent = plans.Replace(
            otherStudentId,
            termId,
            secondEditorVersion,
            ["group-c"]);

        Assert.Equal(ReplaceOutcome.Replaced, winner.Outcome);
        Assert.Equal(ReplaceOutcome.StaleVersion, stale.Outcome);
        Assert.NotNull(stale.CurrentAuthorizedPlan);
        Assert.Equal("version-2", stale.CurrentAuthorizedPlan.Version);
        Assert.Equal(["group-b"], stale.CurrentAuthorizedPlan.SelectedGroupIds);
        Assert.Equal(["group-b"], plans.Read(studentId, termId)!.SelectedGroupIds);

        Assert.Equal(ReplaceOutcome.NotFoundOrOutsideContext, otherStudent.Outcome);
        Assert.Null(otherStudent.CurrentAuthorizedPlan);
        Assert.Null(plans.Read(otherStudentId, termId));
    }

    [Fact]
    public void Production_stale_current_plan_behavior_requires_the_registration_plan_service()
    {
        var service = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Application.RegistrationPlanService");

        Assert.True(
            service is not null,
            "RegistrationPlanService must reject a stale complete replacement and return only the authenticated owner's current plan before EC-2 can pass against production code.");
        Assert.Contains(
            service!.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.Name.Contains("Replace", StringComparison.Ordinal));
    }

    private enum ReplaceOutcome
    {
        Replaced,
        StaleVersion,
        NotFoundOrOutsideContext
    }

    private sealed record PlanSnapshot(
        string Version,
        IReadOnlyList<string> SelectedGroupIds);

    private sealed record ReplaceResult(
        ReplaceOutcome Outcome,
        PlanSnapshot? CurrentAuthorizedPlan);

    private sealed class VersionedPlanStoreDouble
    {
        private readonly Dictionary<(Guid StudentId, Guid TermId), MutablePlan> _plans = [];

        public void Create(
            Guid studentId,
            Guid termId,
            string version,
            IReadOnlyList<string> selectedGroupIds) =>
            _plans.Add(
                (studentId, termId),
                new MutablePlan(ParseVersion(version), [.. selectedGroupIds]));

        public PlanSnapshot? Read(Guid studentId, Guid termId) =>
            _plans.TryGetValue((studentId, termId), out var plan)
                ? plan.Snapshot()
                : null;

        public ReplaceResult Replace(
            Guid authenticatedStudentId,
            Guid authorizedTermId,
            string expectedVersion,
            IReadOnlyList<string> selectedGroupIds)
        {
            if (!_plans.TryGetValue(
                (authenticatedStudentId, authorizedTermId),
                out var plan))
            {
                return new(ReplaceOutcome.NotFoundOrOutsideContext, null);
            }

            if (!string.Equals(
                expectedVersion,
                plan.Snapshot().Version,
                StringComparison.Ordinal))
            {
                return new(ReplaceOutcome.StaleVersion, plan.Snapshot());
            }

            plan.SelectedGroupIds = [.. selectedGroupIds];
            plan.Version++;
            return new(ReplaceOutcome.Replaced, plan.Snapshot());
        }

        private static int ParseVersion(string version) =>
            int.Parse(version.Split('-')[1]);

        private sealed class MutablePlan(int version, string[] selectedGroupIds)
        {
            public int Version { get; set; } = version;

            public string[] SelectedGroupIds { get; set; } = selectedGroupIds;

            public PlanSnapshot Snapshot() =>
                new($"version-{Version}", [.. SelectedGroupIds]);
        }
    }
}
