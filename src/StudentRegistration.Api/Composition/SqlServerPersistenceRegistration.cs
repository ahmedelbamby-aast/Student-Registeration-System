using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Api.Composition;

/// <summary>
/// Registers the modular monolith's single SQL Server persistence boundary.
/// Migration creation remains owned by SPEC-008.
/// </summary>
public static class SqlServerPersistenceRegistration
{
    public const string ConnectionStringName = "StudentRegistration";

    public static IServiceCollection AddStudentRegistrationSqlServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "DATABASE_CONNECTION_REQUIRED: Missing ConnectionStrings:StudentRegistration.");
        }

        services.AddDbContext<StudentRegistrationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.EnableRetryOnFailure()));
        services.AddStudentRegistrationAcademicSqlServer();
        return services;
    }
}
