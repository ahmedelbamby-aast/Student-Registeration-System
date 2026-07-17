namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class NFR_4EvidenceTests
{
    [Fact]
    public async Task Registration_transactions_are_short_local_and_cancellation_aware_before_commit()
    {
        var evidence = await Spec014EvidenceGate.GetAsync();

        Assert.True(evidence.CancellationBeforeCommitVerified);
        Assert.Equal(0, evidence.RemoteDependencyTypesInsideTransactionBoundary);
        Assert.Equal(0, evidence.RemoteCallsInsideTransactions);
        Assert.Equal(1, evidence.RemoteTrace.FaultsInjected);
        Assert.True(evidence.RemoteTrace.ObservedHttpActivities > 0);
        Assert.Equal(0, evidence.RemoteTrace.RemoteActivitiesInsideTransactions);
        Assert.InRange(evidence.Target.TransactionP95Milliseconds, 0, 2_000);
        Assert.InRange(evidence.Spike.TransactionP95Milliseconds, 0, 2_000);
        Assert.True(evidence.Target.MaximumTransactionMilliseconds > 0);
        Assert.True(evidence.Spike.MaximumTransactionMilliseconds > 0);
    }
}
