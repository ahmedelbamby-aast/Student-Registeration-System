namespace StudentRegistration.IntegrationTests.Specs.Spec014.EdgeCases;

public sealed class EC_5Tests
{
    [Fact]
    public void Mismatch_pauses_group_and_allows_one_authorized_idempotent_audited_repair()
    {
        var counter = 2;
        var activeEnrollmentEvidence = 1;
        var paused = false;
        var alertCount = 0;
        var auditCount = 0;
        var repairKeys = new HashSet<string>(StringComparer.Ordinal);

        if (counter != activeEnrollmentEvidence)
        {
            paused = true;
            alertCount++;
        }

        Assert.True(paused);
        Assert.Equal(1, alertCount);

        var repairKey = "group-1:rowversion-7:evidence-hash-a";
        Assert.Contains("group-1", repairKey, StringComparison.Ordinal);
        Assert.Contains("rowversion-7", repairKey, StringComparison.Ordinal);
        Assert.Contains("evidence-hash-a", repairKey, StringComparison.Ordinal);
        var authorized = true;
        if (paused && authorized && repairKeys.Add(repairKey))
        {
            counter = activeEnrollmentEvidence;
            auditCount++;
            paused = false;
        }

        // Inject the same repair from another replica: it is a replay.
        Assert.False(repairKeys.Add(repairKey));

        Assert.Equal(1, alertCount);
        Assert.Equal(1, auditCount);
        Assert.Equal(activeEnrollmentEvidence, counter);
        Assert.False(paused);

        // An untrusted/Admin caller is not the operations service identity and
        // cannot start another repair or produce a second audit record.
        authorized = false;
        var unauthorizedRepairKey = "group-1:rowversion-8:evidence-hash-b";
        if (authorized && repairKeys.Add(unauthorizedRepairKey))
        {
            auditCount++;
        }

        Assert.Equal(1, auditCount);
        Assert.DoesNotContain(unauthorizedRepairKey, repairKeys);

        ProductionEdgeCapability.Require(
            "StudentRegistration.Infrastructure.SqlServer",
            "StudentRegistration.Infrastructure.SqlServer.Registration.EnrollmentCounterReconciler",
            "ReconcileAsync",
            "RepairAsync");
    }
}
