using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class Spec017CompositionBoundaryTests
{
    [Fact]
    public void Spec017_keeps_commands_with_owners_and_places_reads_at_the_expected_boundaries()
    {
        var staffProject = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/StudentRegistration.StaffAdministration.csproj");
        Assert.DoesNotContain("StudentRegistration.Api", staffProject, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistration.Infrastructure.SqlServer", staffProject, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistration.IdentityAccess", staffProject, StringComparison.Ordinal);

        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpoint,
            "MapGet(\"/api/admin/operations/metrics\"",
            "MapGet(\"/api/admin/audit\"",
            "MapPost(\"/api/admin/exports\"",
            "AdminMetricsQuery",
            "AuditEventQueries",
            "AuditExportService");
        Assert.DoesNotContain("DbContext", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteSql", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("IAuditEventWriter", endpoint, StringComparison.Ordinal);

        var worker = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Operations/AdminExportWorker.cs");
        RepositoryFiles.ContainsAll(
            worker,
            "BackgroundService",
            "TryClaimNextAsync",
            "RenewLeaseAsync",
            "PublishAsync",
            "MaximumExportRows");
        Assert.DoesNotContain("ConcurrentQueue", worker, StringComparison.Ordinal);
        Assert.DoesNotContain("Task.Run(", worker, StringComparison.Ordinal);

        var composition = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/ModuleRegistration.cs")
            + RepositoryFiles.Read(
                "src/StudentRegistration.Api/Composition/SqlServerPersistenceRegistration.cs");
        RepositoryFiles.ContainsAll(
            composition,
            "MapSpec017Endpoints",
            "IOperationalMetricReader",
            "IAdminAuditReader",
            "IAdminExportStore",
            "IAdminExportArtifactStore",
            "AdminExportWorker");

        Assert.False(RepositoryFiles.Exists(
            "src/StudentRegistration.StaffAdministration/Application/AdminCommandService.cs"));
        Assert.False(RepositoryFiles.Exists(
            "src/StudentRegistration.StaffAdministration/Application/AdminConfirmationService.cs"));
    }
}
