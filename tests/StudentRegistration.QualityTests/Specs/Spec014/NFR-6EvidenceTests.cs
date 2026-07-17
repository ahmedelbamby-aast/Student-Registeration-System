using StudentRegistration.Infrastructure.SqlServer.Registration;

namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class NFR_6EvidenceTests
{
    [Fact]
    public async Task Required_concurrency_metrics_are_observable_with_privacy_safe_dimensions()
    {
        var evidence = await Spec014EvidenceGate.GetAsync();
        var metrics = evidence.Metrics;

        Assert.Contains(SqlSeatAllocator.DeadlockMetricName, metrics.PublishedMetricNames);
        Assert.Contains(SqlSeatAllocator.LockWaitMetricName, metrics.PublishedMetricNames);
        Assert.Contains(
            RegistrationSubmissionStore.IdempotentReplayMetricName,
            metrics.PublishedMetricNames);
        Assert.Contains(
            SqlSeatAllocator.CapacityConflictMetricName,
            metrics.PublishedMetricNames);
        Assert.Contains(
            EnrollmentCounterReconciler.CounterMismatchMetricName,
            metrics.PublishedMetricNames);
        Assert.True(metrics.DeadlockCount >= 0);
        Assert.True(metrics.LockWaitP95Milliseconds >= 0);
        Assert.True(metrics.IdempotentReplayCount > 0);
        Assert.True(metrics.CapacityConflictCount > 0);
        Assert.True(metrics.ReconciliationMismatchCount > 0);
        Assert.Equal(0, metrics.UnsafeMetricTagCount);
        Assert.Equal(0, evidence.PrivacyViolations);
        Assert.Equal(1, evidence.Boundary.ImpersonationAttemptsRejected);
        Assert.Equal(1, evidence.Boundary.HttpUnauthorizedResponses);
        Assert.Equal(1, evidence.Boundary.HttpAntiforgeryRejections);
    }
}
