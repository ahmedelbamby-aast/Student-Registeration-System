using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Academics.Application.Ports;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

public static class AcademicSqlServerRegistration
{
    public static IServiceCollection AddStudentRegistrationAcademicSqlServer(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<AcademicStore>();
        services.TryAddScoped<IAcademicContextReader>(ResolveAcademicStore);
        services.TryAddScoped<IAcademicStudentScopeReader>(ResolveAcademicStore);
        services.TryAddScoped<IAdminAcademicStore>(ResolveAcademicStore);
        services.TryAddScoped<IRegistrationWindowStore>(ResolveAcademicStore);
        services.TryAddScoped<IStudentAcademicProfileStore>(ResolveAcademicStore);
        services.TryAddScoped<IDemoStudentProfileSeedStore>(ResolveAcademicStore);
        return services;
    }

    private static AcademicStore ResolveAcademicStore(IServiceProvider services) =>
        services.GetRequiredService<AcademicStore>();
}
