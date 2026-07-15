using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.Api.Composition;

public static class AcademicModuleRegistration
{
    private const string DefaultTimeZoneId = "Africa/Cairo";
    private const string DefaultSupportReferencePath = "/support/reference";

    public static IServiceCollection AddStudentRegistrationAcademicModule(
        this IServiceCollection services) =>
        AddStudentRegistrationAcademicModule(
            services,
            DefaultTimeZoneId,
            DefaultSupportReferencePath);

    public static IServiceCollection AddStudentRegistrationAcademicModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return AddStudentRegistrationAcademicModule(
            services,
            configuration["AcademicContext:TimeZoneId"] ?? DefaultTimeZoneId,
            configuration["AcademicContext:SupportReferencePath"] ??
                DefaultSupportReferencePath);
    }

    private static IServiceCollection AddStudentRegistrationAcademicModule(
        IServiceCollection services,
        string timeZoneId,
        string supportReferencePath)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthoritativeTime();
        services.TryAddSingleton(new AcademicContextOptions(timeZoneId));
        services.TryAddSingleton(
            new AcademicSessionContextAdapterOptions(supportReferencePath));
        services.TryAddScoped<AcademicContextResolver>();
        services.TryAddScoped<RegistrationWindowService>();
        services.TryAddScoped<StudentAcademicProfileService>();
        services.TryAddScoped<AdminAcademicManagementService>();
        services.TryAddScoped<
            IAcademicSessionContextAdapter,
            AcademicSessionContextAdapter>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IAuthorizationHandler,
                StudentSelfAcademicProfileAuthorizationHandler>());
        return services;
    }
}

internal sealed class StudentSelfAcademicProfileAuthorizationHandler
    : AuthorizationHandler<OwnStudentResourceRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnStudentResourceRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext &&
            httpContext.Request.Path.Equals(
                "/api/students/me/academic-context",
                StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var applicationUserId) &&
            applicationUserId != Guid.Empty)
        {
            // This route has no client-selected student identifier. Its handler
            // passes this same authenticated ApplicationUser ID to ReadOwnAsync.
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
