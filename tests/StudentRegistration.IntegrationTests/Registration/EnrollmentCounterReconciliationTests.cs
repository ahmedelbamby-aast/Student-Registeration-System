using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;

namespace StudentRegistration.IntegrationTests.Registration;

[Collection(Spec014SqlWorkstreamCollection.Name)]
public sealed class EnrollmentCounterReconciliationTests(
    Spec014SqlWorkstreamDatabase database)
{
    [Fact]
    public void Reconciler_is_an_internal_service_boundary_without_public_admin_repair_surface()
    {
        var methods = typeof(EnrollmentCounterReconciler).GetMethods()
            .Where(method => method.DeclaringType == typeof(EnrollmentCounterReconciler))
            .Select(method => method.Name)
            .ToArray();

        Assert.Contains("ReconcileAsync", methods);
        Assert.Contains("RepairAsync", methods);
        Assert.DoesNotContain(methods, method => method.Contains("Admin", StringComparison.Ordinal));
        Assert.Equal("registration.counter_mismatches", EnrollmentCounterReconciler.CounterMismatchMetricName);
    }

    [Fact]
    public async Task Repair_rejects_non_service_and_missing_permission_callers_before_database_access()
    {
        await using var context = CreateContext();
        var reconciler = new EnrollmentCounterReconciler(context, TimeProvider.System);
        var request = new CounterRepairRequest(
            Guid.NewGuid(),
            [1, 2, 3, 4, 5, 6, 7, 8],
            "evidence-hash",
            "correlation-id");

        var admin = new ReconciliationServiceIdentity("admin", false, ["Registration.Reconcile"]);
        var serviceWithoutPermission = new ReconciliationServiceIdentity("worker", true, []);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            reconciler.RepairAsync(admin, request, CancellationToken.None));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            reconciler.RepairAsync(serviceWithoutPermission, request, CancellationToken.None));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Mismatch_pauses_and_two_replicas_share_one_authorized_audited_repair()
    {
        var seed = await database.SeedAsync(
            groupCapacities: [3],
            initialEnrolledCount: 2,
            activeEnrollmentCount: 1);
        CounterReconciliationResult mismatch;
        await using (var context = database.CreateContext())
        {
            var reconciler = new EnrollmentCounterReconciler(
                context,
                TimeProvider.System);
            mismatch = await reconciler.ReconcileAsync(seed.GroupIds[0]);
        }

        Assert.True(mismatch.MismatchDetected);
        Assert.True(mismatch.RegistrationPaused);
        Assert.Equal(2, mismatch.StoredCount);
        Assert.Equal(1, mismatch.ActiveEnrollmentCount);

        var request = new CounterRepairRequest(
            seed.GroupIds[0],
            mismatch.ObservedGroupVersion,
            mismatch.EnrollmentEvidenceHash,
            "repair-test");
        await using (var deniedContext = database.CreateContext())
        {
            var denied = new EnrollmentCounterReconciler(
                deniedContext,
                TimeProvider.System);
            var admin = new ReconciliationServiceIdentity(
                "admin",
                false,
                [EnrollmentCounterReconciler.ReconcilePermission]);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                denied.RepairAsync(admin, request));
        }

        var identity = new ReconciliationServiceIdentity(
            "registration-reconciliation-worker",
            true,
            [EnrollmentCounterReconciler.ReconcilePermission]);
        var gate = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var ready = 0;
        async Task<CounterRepairResult> RepairAsync()
        {
            await using var context = database.CreateContext();
            var reconciler = new EnrollmentCounterReconciler(
                context,
                TimeProvider.System);
            if (Interlocked.Increment(ref ready) == 2)
            {
                gate.TrySetResult();
            }

            await gate.Task.WaitAsync(TimeSpan.FromSeconds(10));
            return await reconciler.RepairAsync(identity, request);
        }

        var repairs = await Task.WhenAll(RepairAsync(), RepairAsync());

        Assert.Single(repairs, result => !result.IsReplay);
        Assert.Single(repairs, result => result.IsReplay);
        Assert.All(repairs, result =>
        {
            Assert.Equal(1, result.RepairedCount);
            Assert.False(result.RegistrationPaused);
        });
        Assert.Equal(
            1,
            await database.ScalarAsync<int>(
                "SELECT [EnrolledCount] FROM [scheduling].[SectionGroups] WHERE [Id] = @id",
                new Microsoft.Data.SqlClient.SqlParameter("@id", seed.GroupIds[0])));
        Assert.Equal(
            1,
            await database.ScalarAsync<int>(
                "SELECT COUNT(*) FROM [audit].[AuditEvents] WHERE [Action] = 'RegistrationCounterRepaired' AND [EntityId] = @id",
                new Microsoft.Data.SqlClient.SqlParameter("@id", seed.GroupIds[0].ToString("N"))));
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Spec014Model;Trusted_Connection=True")
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}
