using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Registration;

[Collection(Spec014SqlWorkstreamCollection.Name)]
public sealed class RegistrationIdempotencyFailureTests(
    Spec014SqlWorkstreamDatabase database)
{
    [Fact]
    public void Store_exposes_claim_observation_and_final_replay_operations()
    {
        var methods = typeof(RegistrationSubmissionStore).GetMethods()
            .Where(method => method.DeclaringType == typeof(RegistrationSubmissionStore))
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ClaimOrObserveAsync", methods);
        Assert.Contains("ClaimOrReplayAsync", methods);
        Assert.Contains("ClaimInsideTransactionAsync", methods);
        Assert.Contains("WaitForFinalResultAsync", methods);
        Assert.Contains("ReplayAsync", methods);
        Assert.Contains("ReplayCommittedAsync", methods);
        Assert.Contains("ReadFinalByRequestAsync", methods);
    }

    [Fact]
    public void Database_model_scopes_the_single_claim_by_student_term_and_request()
    {
        using var context = CreateContext();
        var submission = context.Model.FindEntityType(typeof(RegistrationSubmission));

        Assert.NotNull(submission);
        Assert.Contains(submission!.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual([
                nameof(RegistrationSubmission.StudentId),
                nameof(RegistrationSubmission.TermId),
                nameof(RegistrationSubmission.ClientRequestId)]));
    }

    [Fact]
    public async Task Observation_window_is_hard_bounded_to_500_milliseconds()
    {
        await using var context = CreateContext();
        var store = new RegistrationSubmissionStore(context, TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            store.WaitForFinalResultAsync(
                new RegistrationRequestScope(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                "payload-hash",
                TimeSpan.FromMilliseconds(501),
                CancellationToken.None));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Claim_requires_current_transaction_and_rollback_leaves_no_processing_row()
    {
        var seed = await database.SeedAsync();
        var scope = new RegistrationRequestScope(
            seed.StudentId,
            seed.TermId,
            Guid.NewGuid());
        await using var context = database.CreateContext();
        var store = new RegistrationSubmissionStore(context, TimeProvider.System);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.ClaimInsideTransactionAsync(
                scope,
                "payload-a",
                DateTime.UtcNow));

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var claim = await store.ClaimInsideTransactionAsync(
                scope,
                "payload-a",
                DateTime.UtcNow);
            Assert.True(claim.MayExecute);
            await transaction.RollbackAsync();
        }

        await using var freshContext = database.CreateContext();
        var freshStore = new RegistrationSubmissionStore(
            freshContext,
            TimeProvider.System);
        Assert.Null(await freshStore.ReadFinalByRequestAsync(scope));
        Assert.Equal(
            0,
            await freshContext.Set<RegistrationSubmission>()
                .CountAsync(submission =>
                    submission.StudentId == scope.StudentId &&
                    submission.TermId == scope.TermId &&
                    submission.ClientRequestId == scope.ClientRequestId));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Accepted_and_rejected_results_replay_payload_bound_and_cross_term_is_independent()
    {
        var seed = await database.SeedAsync();
        var key = Guid.NewGuid();
        var acceptedScope = new RegistrationRequestScope(
            seed.StudentId,
            seed.TermId,
            key);
        var rejectedScope = new RegistrationRequestScope(
            seed.StudentId,
            seed.TermId,
            Guid.NewGuid());
        var crossTermScope = new RegistrationRequestScope(
            seed.StudentId,
            seed.SecondTermId,
            key);

        await using (var context = database.CreateContext())
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var store = new RegistrationSubmissionStore(context, TimeProvider.System);
            var accepted = await store.ClaimOrReplayAsync(
                acceptedScope,
                "payload-a",
                DateTime.UtcNow);
            Assert.NotNull(accepted.Submission);
            await store.FinalizeAcceptedAsync(
                accepted.Submission!,
                "ACCEPTED",
                $"REG-{Guid.NewGuid():N}",
                "{}",
                "{}",
                DateTime.UtcNow);

            var rejected = await store.ClaimOrReplayAsync(
                rejectedScope,
                "payload-r",
                DateTime.UtcNow);
            Assert.NotNull(rejected.Submission);
            await store.FinalizeRejectedAsync(
                rejected.Submission!,
                "GROUP_FULL",
                "{}",
                DateTime.UtcNow);
            await transaction.CommitAsync();
        }

        await using (var context = database.CreateContext())
        {
            var store = new RegistrationSubmissionStore(context, TimeProvider.System);
            var acceptedReplay = await store.ReplayCommittedAsync(
                acceptedScope,
                "payload-a");
            var rejectedReplay = await store.ReplayCommittedAsync(
                rejectedScope,
                "payload-r");
            var mismatch = await store.ReplayCommittedAsync(
                acceptedScope,
                "different-payload");
            Assert.Equal(SubmissionClaimStatus.Replayed, acceptedReplay.Status);
            Assert.Equal("ACCEPTED", acceptedReplay.Submission!.ResultCode);
            Assert.Equal(SubmissionClaimStatus.Replayed, rejectedReplay.Status);
            Assert.Equal("GROUP_FULL", rejectedReplay.Submission!.ResultCode);
            Assert.Equal(SubmissionClaimStatus.PayloadMismatch, mismatch.Status);
            Assert.Equal("IDEMPOTENCY_KEY_REUSED", mismatch.ReasonCode);
        }

        await using (var context = database.CreateContext())
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var store = new RegistrationSubmissionStore(context, TimeProvider.System);
            var independent = await store.ClaimInsideTransactionAsync(
                crossTermScope,
                "payload-for-another-term",
                DateTime.UtcNow);
            Assert.True(independent.MayExecute);
            await store.FinalizeRejectedAsync(
                independent.Submission!,
                "WINDOW_CLOSED",
                "{}",
                DateTime.UtcNow);
            await transaction.CommitAsync();
        }
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Uncommitted_claim_is_observed_for_at_most_500ms_without_durable_processing_disclosure()
    {
        var seed = await database.SeedAsync();
        var scope = new RegistrationRequestScope(
            seed.StudentId,
            seed.TermId,
            Guid.NewGuid());
        await using var winnerContext = database.CreateContext();
        await using var winnerTransaction =
            await winnerContext.Database.BeginTransactionAsync();
        var winnerStore = new RegistrationSubmissionStore(
            winnerContext,
            TimeProvider.System);
        var claim = await winnerStore.ClaimInsideTransactionAsync(
            scope,
            "payload-a",
            DateTime.UtcNow);
        Assert.True(claim.MayExecute);

        await using var observerContext = database.CreateContext();
        var observerStore = new RegistrationSubmissionStore(
            observerContext,
            TimeProvider.System);
        var stopwatch = Stopwatch.StartNew();
        var observation = await observerStore.WaitForFinalResultAsync(
            scope,
            "payload-a",
            TimeSpan.FromMilliseconds(150));
        stopwatch.Stop();

        Assert.Equal(SubmissionClaimStatus.InProgress, observation.Status);
        Assert.Null(observation.Submission);
        Assert.InRange(stopwatch.ElapsedMilliseconds, 100, 500);
        await winnerTransaction.RollbackAsync();
        Assert.Null(await observerStore.ReadFinalByRequestAsync(scope));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Simultaneous_first_use_loser_times_out_boundedly_and_never_claims_or_executes()
    {
        var seed = await database.SeedAsync();
        var scope = new RegistrationRequestScope(
            seed.StudentId,
            seed.TermId,
            Guid.NewGuid());
        await using var winnerContext = database.CreateContext();
        await using var winnerTransaction =
            await winnerContext.Database.BeginTransactionAsync();
        var winnerStore = new RegistrationSubmissionStore(
            winnerContext,
            TimeProvider.System);
        var winner = await winnerStore.ClaimInsideTransactionAsync(
            scope,
            "payload-a",
            DateTime.UtcNow);
        Assert.True(winner.MayExecute);

        await using var loserContext = database.CreateContext();
        await using var loserTransaction =
            await loserContext.Database.BeginTransactionAsync();
        var loserStore = new RegistrationSubmissionStore(
            loserContext,
            TimeProvider.System);
        var stopwatch = Stopwatch.StartNew();
        await Assert.ThrowsAsync<RegistrationClaimContendedException>(() =>
            loserStore.ClaimOrReplayAsync(
                scope,
                "payload-a",
                DateTime.UtcNow));
        stopwatch.Stop();

        Assert.InRange(stopwatch.ElapsedMilliseconds, 350, 800);
        await loserTransaction.RollbackAsync();

        await using var observerContext = database.CreateContext();
        var observation = await new RegistrationSubmissionStore(
                observerContext,
                TimeProvider.System)
            .WaitForFinalResultAsync(
                scope,
                "payload-a",
                TimeSpan.FromMilliseconds(150));
        Assert.Equal(SubmissionClaimStatus.InProgress, observation.Status);
        Assert.False(observation.MayExecute);
        Assert.Null(observation.Submission);

        await winnerStore.FinalizeRejectedAsync(
            winner.Submission!,
            "GROUP_FULL",
            "{}",
            DateTime.UtcNow);
        await winnerTransaction.CommitAsync();

        await using var replayContext = database.CreateContext();
        var replay = await new RegistrationSubmissionStore(
                replayContext,
                TimeProvider.System)
            .ReplayCommittedAsync(scope, "payload-a");
        Assert.Equal(SubmissionClaimStatus.Replayed, replay.Status);
        Assert.Equal("GROUP_FULL", replay.Submission!.ResultCode);
        Assert.Equal(
            1,
            await replayContext.Set<RegistrationSubmission>().CountAsync(
                submission =>
                    submission.StudentId == scope.StudentId &&
                    submission.TermId == scope.TermId &&
                    submission.ClientRequestId == scope.ClientRequestId));
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Spec014Model;Trusted_Connection=True")
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}
