namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class Spec014TransactionSmokeTests
{
    [Fact]
    public Task Complete_transaction_runs_inside_the_configured_retrying_execution_strategy() =>
        Spec014RegistrationLoadHarness.RunRetriableTransactionSmokeAsync();
}
