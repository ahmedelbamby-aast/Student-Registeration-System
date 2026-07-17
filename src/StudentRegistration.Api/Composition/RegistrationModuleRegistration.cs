using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Api.Composition;

public static class RegistrationModuleRegistration
{
    public static IServiceCollection AddStudentRegistrationRegistrationModule(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<GroupSummaryProjection>();
        services.TryAddScoped<EligibilityService>();
        services.TryAddScoped<OfferingSearchQuery>();
        services.TryAddScoped<ScheduleConflictDetector>();
        services.TryAddScoped<RegistrationPlanService>();
        services.TryAddSingleton(
            new OptimizerConfiguration(
                "1.0.0",
                [
                    ScoreFactor.PreferenceViolations,
                    ScoreFactor.IdleMinutes,
                    ScoreFactor.TeachingDays,
                    ScoreFactor.StableGroupTuple
                ],
                "SPEC-013-GATE-A-2026-07-13"));
        services.TryAddScoped<ScheduleScorer>();
        services.TryAddScoped<ScheduleOptimizer>();
        services.TryAddScoped<OptimizationCoordinator>();
        services.TryAddScoped<RecommendationApplicationService>();
        services.TryAddScoped<RegistrationCommandFactory>();
        services.TryAddScoped<RegistrationTransactionCoordinator>();
        services.TryAddScoped<RegistrationEndpointService>();
        services.TryAddScoped<RegistrationReceiptService>();
        services.TryAddScoped<RegistrationRecordQueries>();
        services.TryAddScoped<ICurrentPlanReader, EmptyCurrentPlanReader>();
        return services;
    }
}
