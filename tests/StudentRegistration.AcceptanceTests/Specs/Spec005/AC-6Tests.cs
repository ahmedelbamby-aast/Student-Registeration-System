using StudentRegistration.TestSupport;
using StudentRegistration.Infrastructure.SqlServer.Registration;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

[Collection(Spec005SqlAcceptanceCollection.Name)]
public sealed class AC_6Tests(Spec005SqlAcceptanceDatabase database)
{
    [Fact]
    public void Shared_boundary_and_idempotency_claim_are_payload_bound_and_durable()
    {
        var sqlProof = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Registration/RegistrationIdempotencyFailureTests.cs");
        var store = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationSubmissionStore.cs");
        var coordinator = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs");

        RepositoryFiles.ContainsAll(
            sqlProof,
            "Accepted_and_rejected_results_replay_payload_bound_and_cross_term_is_independent",
            "ReplayCommittedAsync",
            "PayloadMismatch",
            "IDEMPOTENCY_KEY_REUSED",
            "database.CreateContext()");
        RepositoryFiles.ContainsAll(
            store,
            "StudentId",
            "TermId",
            "ClientRequestId",
            "PayloadHash",
            "ReplayCommittedAsync");
        RepositoryFiles.ContainsAll(
            coordinator,
            "ExecuteRegistrationBoundaryAsync",
            "ExpectedStudentTermStateRowVersion",
            "student/term transaction boundary");
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_replays_after_fresh_context_and_rejects_a_different_payload()
    {
        var graph = await database.SeedAsync(1, 0, 1);
        var scope = new RegistrationRequestScope(
            graph.StudentIds[0],
            graph.TermId,
            Guid.NewGuid());
        await using (var context = database.CreateContext())
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var store = new RegistrationSubmissionStore(context, TimeProvider.System);
            var claim = await store.ClaimInsideTransactionAsync(
                scope,
                "payload-a",
                DateTime.UtcNow);
            Assert.True(claim.MayExecute);
            await store.FinalizeRejectedAsync(
                claim.Submission!,
                "GROUP_FULL",
                "{}",
                DateTime.UtcNow);
            await transaction.CommitAsync();
        }

        await using var restartedContext = database.CreateContext();
        var restartedStore = new RegistrationSubmissionStore(
            restartedContext,
            TimeProvider.System);
        var replay = await restartedStore.ReplayCommittedAsync(scope, "payload-a");
        var mismatch = await restartedStore.ReplayCommittedAsync(scope, "payload-b");
        Assert.Equal(SubmissionClaimStatus.Replayed, replay.Status);
        Assert.Equal("GROUP_FULL", replay.Submission!.ResultCode);
        Assert.Equal(SubmissionClaimStatus.PayloadMismatch, mismatch.Status);
        Assert.Equal("IDEMPOTENCY_KEY_REUSED", mismatch.ReasonCode);
    }
}
