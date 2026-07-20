using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Infrastructure.SqlServer.Admin;
using StudentRegistration.Infrastructure.SqlServer.Academics;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.Infrastructure.SqlServer.Scheduling;
using StudentRegistration.Infrastructure.SqlServer.StaffWorkspace;
using StudentRegistration.Registration.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;

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
        services.AddStudentRegistrationDiscoverySqlServer();
        services.AddStudentRegistrationPlansSqlServer();
        services.AddScheduleRecommendationsSqlServer();
        services.TryAddScoped<SqlCatalogueAdministrationStore>();
        services.TryAddScoped<ICatalogueAdministrationStore>(services =>
            services.GetRequiredService<SqlCatalogueAdministrationStore>());
        services.TryAddScoped<SqlOfferingStore>();
        services.TryAddScoped<IOfferingStore>(services =>
            services.GetRequiredService<SqlOfferingStore>());
        services.TryAddScoped<RegistrationSubmissionStore>();
        services.TryAddScoped<SqlSeatAllocator>();
        services.TryAddScoped<SqlRegistrationEndpointStore>();
        services.TryAddScoped<SqlRegistrationApprovalStore>();
        services.TryAddScoped<IRegistrationApprovalStore>(services =>
            services.GetRequiredService<SqlRegistrationApprovalStore>());
        services.TryAddScoped<SqlRegistrationRecordReader>();
        services.TryAddScoped<IRegistrationEndpointStore>(services =>
            services.GetRequiredService<SqlRegistrationEndpointStore>());
        services.TryAddScoped<IRegistrationLocalTransactionStore>(services =>
            services.GetRequiredService<SqlRegistrationEndpointStore>());
        services.TryAddScoped<StudentRegistration.Registration.Application.Ports.IRegistrationRecordReader>(
            services => services.GetRequiredService<SqlRegistrationRecordReader>());
        services.TryAddScoped<IStaffAvailabilityPort, SqlStaffAvailabilityPort>();
        services.TryAddScoped<StaffAvailabilityFacade>();
        services.TryAddScoped<SqlStaffWorkspaceAdapter>();
        services.TryAddScoped<IStaffWorkspaceReader>(services =>
            services.GetRequiredService<SqlStaffWorkspaceAdapter>());
        services.TryAddScoped<IStaffWorkspaceAuditWriter>(services =>
            services.GetRequiredService<SqlStaffWorkspaceAdapter>());
        services.TryAddScoped<IStaffIdentityResolver>(services =>
            services.GetRequiredService<SqlStaffWorkspaceAdapter>());
        services.TryAddScoped<StaffWorkspaceQueries>();
        services.TryAddScoped<IAdminAuditReader, SqlAdminAuditReader>();
        services.TryAddScoped<IAdminExportStore, SqlAdminExportStore>();
        services.TryAddSingleton<IAdminExportArtifactStore>(_ =>
            new SharedFileAdminExportArtifactStore(
                Path.GetFullPath(
                    configuration["AdminOperations:ExportArtifactRoot"]
                    ?? Path.Combine(
                        AppContext.BaseDirectory,
                        "App_Data",
                        "admin-exports"))));
        return services;
    }
}
