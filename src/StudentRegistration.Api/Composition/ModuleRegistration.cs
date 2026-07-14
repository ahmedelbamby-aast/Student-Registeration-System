using StudentRegistration.Api.Endpoints;
using StudentRegistration.Api.Operations;
using StudentRegistration.IdentityAccess.Endpoints;

namespace StudentRegistration.Api.Composition;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddStudentRegistrationSqlServer(builder.Configuration);
        // The SPEC-018 security gate validates inputs, then delegates once to
        // AddStudentRegistrationDataProtection for the canonical key setup.
        builder.Services.AddStudentRegistrationSecurity(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddStudentRegistrationIdentitySecurity(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddStudentRegistrationModules();

        var app = builder.Build();
        app.UseSafeApiErrors();
        app.UseStudentRegistrationWebApp();
        app.UseStudentRegistrationObservability();
        app.UseRouting();
        app.UseStudentRegistrationIdentitySecurity();
        app.MapSpec018Endpoints();
        app.MapSpec007Endpoints();
        app.MapStudentRegistrationWebAppFallback();
        app.Run();
    }
}

public static class ModuleRegistration
{
    public static IServiceCollection AddStudentRegistrationModules(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthoritativeTime();
        services.AddStudentRegistrationJsonContracts();
        services.AddSafeApiErrors();
        services.AddStudentRegistrationObservability();

        return services;
    }
}
