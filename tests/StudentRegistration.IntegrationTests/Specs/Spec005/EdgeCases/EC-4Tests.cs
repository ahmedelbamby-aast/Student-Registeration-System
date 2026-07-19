using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.IntegrationTests.Registration;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

[Collection(Spec014SqlWorkstreamCollection.Name)]
public sealed class EC_4Tests(Spec014SqlWorkstreamDatabase database)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Enrollment_counter_mismatch_alerts_and_requires_authorized_audited_repair_without_rewriting_history()
    {
        var seed = await database.SeedAsync(
            groupCapacities: [3],
            initialEnrolledCount: 2,
            activeEnrollmentCount: 1);
        Guid[] enrollmentIdsBefore;
        await using (var before = database.CreateContext())
        {
            enrollmentIdsBefore = await before.Set<Enrollment>()
                .AsNoTracking()
                .Where(enrollment => enrollment.GroupId == seed.GroupIds[0])
                .OrderBy(enrollment => enrollment.Id)
                .Select(enrollment => enrollment.Id)
                .ToArrayAsync();
        }

        CounterReconciliationResult mismatch;
        await using (var detection = database.CreateContext())
        {
            mismatch = await new EnrollmentCounterReconciler(
                detection,
                TimeProvider.System).ReconcileAsync(seed.GroupIds[0]);
        }

        Assert.True(mismatch.MismatchDetected);
        Assert.True(mismatch.RegistrationPaused);
        Assert.Equal(2, mismatch.StoredCount);
        Assert.Equal(1, mismatch.ActiveEnrollmentCount);
        Assert.Equal(
            1,
            await database.ScalarAsync<int>(
                "SELECT COUNT(*) FROM [audit].[AuditEvents] WHERE [Action] = 'RegistrationCounterMismatchAlerted' AND [EntityId] = @id",
                new Microsoft.Data.SqlClient.SqlParameter(
                    "@id",
                    seed.GroupIds[0].ToString("N"))));

        var request = new CounterRepairRequest(
            seed.GroupIds[0],
            mismatch.ObservedGroupVersion,
            mismatch.EnrollmentEvidenceHash,
            "spec005-ec4");
        await using (var denied = database.CreateContext())
        {
            var reconciler = new EnrollmentCounterReconciler(
                denied,
                TimeProvider.System);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                reconciler.RepairAsync(
                    new ReconciliationServiceIdentity(
                        "admin",
                        false,
                        [EnrollmentCounterReconciler.ReconcilePermission]),
                    request));
        }

        CounterRepairResult repaired;
        await using (var repair = database.CreateContext())
        {
            repaired = await new EnrollmentCounterReconciler(
                repair,
                TimeProvider.System).RepairAsync(
                    new ReconciliationServiceIdentity(
                        "registration-reconciliation-worker",
                        true,
                        [EnrollmentCounterReconciler.ReconcilePermission]),
                    request);
        }

        Assert.False(repaired.IsReplay);
        Assert.Equal(1, repaired.RepairedCount);
        Assert.False(repaired.RegistrationPaused);
        Assert.Equal(
            1,
            await database.ScalarAsync<int>(
                "SELECT COUNT(*) FROM [audit].[AuditEvents] WHERE [Action] = 'RegistrationCounterRepaired' AND [EntityId] = @id",
                new Microsoft.Data.SqlClient.SqlParameter(
                    "@id",
                    seed.GroupIds[0].ToString("N"))));

        await using var after = database.CreateContext();
        var enrollmentIdsAfter = await after.Set<Enrollment>()
            .AsNoTracking()
            .Where(enrollment => enrollment.GroupId == seed.GroupIds[0])
            .OrderBy(enrollment => enrollment.Id)
            .Select(enrollment => enrollment.Id)
            .ToArrayAsync();
        Assert.Equal(enrollmentIdsBefore, enrollmentIdsAfter);
    }
}
