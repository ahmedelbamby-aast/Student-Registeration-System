using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class StaffAvailabilityOwnershipTests
{
    [Fact]
    public void StaffAdministration_has_no_availability_entity_repository_or_DbContext_dependency()
    {
        var root = RepositoryFiles.PathTo("src/StudentRegistration.StaffAdministration");
        var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).ToArray();
        var source = string.Join("\n", files.Select(File.ReadAllText));

        Assert.DoesNotContain("class StaffTermAvailability", source, StringComparison.Ordinal);
        Assert.DoesNotContain("class StaffAvailabilityRepository", source, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistrationDbContext", source, StringComparison.Ordinal);
        Assert.Contains("IStaffAvailabilityPort", source, StringComparison.Ordinal);
    }

    [Fact]
    public void StaffAdministration_project_depends_on_Scheduling_not_SQL_infrastructure()
    {
        var project = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/StudentRegistration.StaffAdministration.csproj");

        Assert.Contains("StudentRegistration.Scheduling", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistration.Infrastructure.SqlServer", project, StringComparison.Ordinal);
        Assert.DoesNotContain("EntityFrameworkCore", project, StringComparison.Ordinal);
    }

    [Fact]
    public void Composition_registers_the_Scheduling_port_with_its_SQL_adapter()
    {
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/SqlServerPersistenceRegistration.cs");

        RepositoryFiles.ContainsAll(
            registration,
            "TryAddScoped<IStaffAvailabilityPort, SqlStaffAvailabilityPort>()",
            "TryAddScoped<StaffAvailabilityFacade>()");
    }
}
