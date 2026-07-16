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
        services.TryAddScoped<ICurrentPlanReader, EmptyCurrentPlanReader>();
        return services;
    }
}
