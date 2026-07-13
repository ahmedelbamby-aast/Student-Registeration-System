namespace StudentRegistration.Api.Composition;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddStudentRegistrationDataProtection(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddStudentRegistrationModules();

        var app = builder.Build();
        app.UseSafeApiErrors();
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

        return services;
    }
}
