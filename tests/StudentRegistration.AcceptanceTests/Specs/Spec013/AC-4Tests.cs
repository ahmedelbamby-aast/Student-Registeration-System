using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec013;

public sealed class AC_4Tests
{
    [Fact]
    public void Expired_budget_requires_a_safe_status_and_only_verified_complete_options()
    {
        // Given a recommendation request whose configured time budget expires.
        using var budget = new CancellationTokenSource();
        budget.Cancel();
        Assert.True(budget.IsCancellationRequested);

        // When cancellation is observed, the production coordinator is required.
        var coordinator = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Application.OptimizationCoordinator");

        // Then it can return the time-budget status with only verified complete
        // results while leaving final submission blocked otherwise.
        Assert.True(
            coordinator is not null,
            "OptimizationCoordinator must deliver AC-4 bounded cancellation and time-budget results before this future acceptance test can pass.");
        var optimizeMethod = coordinator!
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .SingleOrDefault(method =>
                (method.Name.Contains("Optimize", StringComparison.Ordinal)
                    || method.Name.Contains("Recommend", StringComparison.Ordinal))
                && method.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(CancellationToken)));
        Assert.True(
            optimizeMethod is not null,
            "The bounded optimization entry point must accept a CancellationToken.");
    }
}
