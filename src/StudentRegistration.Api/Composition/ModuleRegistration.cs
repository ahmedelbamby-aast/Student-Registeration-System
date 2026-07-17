using StudentRegistration.Api.Endpoints;
using StudentRegistration.Api.Operations;
using StudentRegistration.Academics.Endpoints;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.IdentityAccess.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.Registration.Endpoints;
using StudentRegistration.Scheduling.Endpoints;
using StudentRegistration.StaffAdministration.Endpoints;

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
        builder.Services.AddStudentRegistrationAcademicModule(builder.Configuration);
        builder.Services.AddStudentRegistrationRegistrationModule();

        var app = builder.Build();
        app.UseSafeApiErrors();
        app.UseStudentRegistrationWebApp();
        app.UseStudentRegistrationObservability();
        app.UseRouting();
        app.UseStudentRegistrationIdentitySecurity();
        app.MapSpec018Endpoints();
        app.MapSpec007Endpoints();
        app.MapSpec008Endpoints();
        app.MapSpec009Endpoints();
        app.MapSpec010Endpoints();
        app.MapSpec011Endpoints();
        app.MapSpec012Endpoints();
        app.MapSpec013Endpoints();
        app.MapSpec014Endpoints();
        app.MapSpec015Endpoints();
        app.MapSpec016Endpoints();
        app.MapSpec017Endpoints();
        if (app.Environment.IsDevelopment() ||
            app.Environment.IsEnvironment("Testing"))
        {
            app.MapOpenApi().AllowAnonymous();
        }
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
        services.AddStudentRegistrationOpenApi();
        services.AddStudentRegistrationObservability();
        services.TryAddScoped<IOperationalMetricReader, OperationalMetricReader>();
        services.TryAddScoped<AdminMetricsQuery>();
        services.TryAddScoped<AuditEventQueries>();
        services.TryAddScoped<AuditExportService>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, AdminExportWorker>());

        return services;
    }
}
