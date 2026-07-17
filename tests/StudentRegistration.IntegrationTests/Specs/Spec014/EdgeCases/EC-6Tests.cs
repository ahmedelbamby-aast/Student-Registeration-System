namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_6Tests
{
    [Fact]
    public async Task Different_plans_across_replicas_serialize_on_student_term_state()
    {
        var canonicalDatabaseBoundary = new SharedBoundary();
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        string? committedPlan = null;
        var outcomes = new List<string>();
        var observedScopes = new List<(Guid StudentId, Guid TermId)>();
        var replicas = new HashSet<string>(StringComparer.Ordinal);

        async Task SubmitFromReplicaAsync(string replica, string plan)
        {
            await canonicalDatabaseBoundary.ExecuteAsync(() =>
            {
                replicas.Add(replica);
                observedScopes.Add((studentId, termId));
                if (committedPlan is null)
                {
                    committedPlan = plan;
                    outcomes.Add("ACCEPTED");
                }
                else
                {
                    outcomes.Add("PLAN_CHANGED");
                }

                return Task.CompletedTask;
            });
        }

        await Task.WhenAll(
            SubmitFromReplicaAsync("replica-a", "plan-a"),
            SubmitFromReplicaAsync("replica-b", "plan-b"));

        Assert.Equal(1, canonicalDatabaseBoundary.MaximumConcurrent);
        Assert.Equal(2, replicas.Count);
        Assert.Single(observedScopes.Distinct());
        Assert.Single(outcomes, "ACCEPTED");
        Assert.Single(outcomes, "PLAN_CHANGED");

        ProductionEdgeCapability.Require(
            "StudentRegistration.Registration",
            "StudentRegistration.Registration.Application.RegistrationTransactionCoordinator",
            "ExecuteRegistrationPlanAsync");
    }
}
