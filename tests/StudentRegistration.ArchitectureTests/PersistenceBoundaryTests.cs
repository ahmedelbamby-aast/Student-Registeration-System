using System.Text.Json;
using System.Xml.Linq;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class PersistenceBoundaryTests
{
    private const string ContextPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs";

    private static readonly string[] BusinessModules =
    [
        "IdentityAccess",
        "Academics",
        "Scheduling",
        "Registration",
        "StaffAdministration"
    ];

    [Fact]
    public void Single_context_is_owned_by_infrastructure_and_uses_module_configurations()
    {
        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/persistence-manifest.json"));
        Assert.Equal(
            ContextPath,
            manifest.RootElement.GetProperty("dbContext").GetProperty("path").GetString());

        var context = RepositoryFiles.Read(ContextPath);
        RepositoryFiles.ContainsAll(
            context,
            "public sealed class StudentRegistrationDbContext : DbContext",
            "DbContextOptions<StudentRegistrationDbContext>",
            "DbSet<AuditEvent>",
            "ApplyConfigurationsFromAssembly",
            "base.OnModelCreating(modelBuilder)");

        var contexts = Directory
            .EnumerateFiles(RepositoryFiles.PathTo("src"), "*DbContext.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Select(path => Path.GetRelativePath(RepositoryFiles.Root, path).Replace('\\', '/'))
            .ToArray();
        Assert.Equal([ContextPath], contexts);
    }

    [Fact]
    public void Business_modules_have_no_ef_provider_or_infrastructure_reference()
    {
        foreach (var module in BusinessModules)
        {
            var projectPath =
                $"src/StudentRegistration.{module}/StudentRegistration.{module}.csproj";
            var project = XDocument.Parse(RepositoryFiles.Read(projectPath));

            Assert.DoesNotContain(
                project.Descendants("PackageReference"),
                reference => (reference.Attribute("Include")?.Value ?? string.Empty)
                    .StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
            Assert.DoesNotContain(
                project.Descendants("ProjectReference"),
                reference => (reference.Attribute("Include")?.Value ?? string.Empty)
                    .Contains("Infrastructure.SqlServer", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Query_reads_are_projected_no_tracking_and_commands_use_focused_transactions()
    {
        var boundary = RepositoryFiles.Read("docs/architecture/persistence-boundary.md");

        RepositoryFiles.ContainsAll(
            boundary,
            "single StudentRegistrationDbContext",
            "Infrastructure.SqlServer",
            "AsNoTracking",
            "Select",
            "DTO",
            "atomic transaction",
            "cross-module transaction",
            "architecture review",
            "distributed transaction",
            "stale cache",
            "advisory",
            "final registration",
            "authoritative SQL",
            "revalidate");

        var context = RepositoryFiles.Read(ContextPath);
        Assert.DoesNotContain("generic repository", context, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("unit of work", context, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveChangesAsync", context, StringComparison.Ordinal);
    }

    private static bool IsGeneratedPath(string path)
    {
        var relative = Path.GetRelativePath(RepositoryFiles.Root, path);
        return relative
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");
    }
}
