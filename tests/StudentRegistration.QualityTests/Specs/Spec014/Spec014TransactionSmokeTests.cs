namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class Spec014TransactionSmokeTests
{
    [Fact]
    public Task Complete_transaction_runs_inside_the_configured_retrying_execution_strategy() =>
        Spec014RegistrationLoadHarness.RunRetriableTransactionSmokeAsync();

    [Fact]
    public async Task Mixed_same_rate_diagnostic_records_status_and_failure_kinds()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SPEC018_RUN_MIXED_DIAGNOSTIC"),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        var evidence = await Spec014RegistrationLoadHarness.RunMixedDiagnosticAsync();
        Assert.Equal(2_250, evidence.Submissions.CompletedRequests);
        Assert.Equal(9_000, evidence.Reads.CompletedRequests);
    }
}
