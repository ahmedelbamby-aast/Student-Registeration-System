using System.Xml.Linq;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class DtoIsolationTests
{
    private const string SharedBoundaryPath =
        "src/StudentRegistration.Contracts/SharedContracts.cs";

    [Fact]
    public void DTO_contract_project_is_framework_free_and_declares_the_EF_boundary()
    {
        var boundary = RepositoryFiles.Read(SharedBoundaryPath);
        RepositoryFiles.ContainsAll(
            boundary,
            "AssemblyMetadata",
            "StudentRegistration.Contracts.Boundary",
            "DTOs and value identifiers only",
            "EF entities and persistence navigation graphs are forbidden");

        var project = XDocument.Parse(RepositoryFiles.Read(
            "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj"));
        Assert.Empty(project.Descendants("ProjectReference"));
        Assert.DoesNotContain(
            project.Descendants("PackageReference"),
            reference => (reference.Attribute("Include")?.Value ?? string.Empty)
                .StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Contracts_source_exposes_no_persistence_type_or_navigation_graph()
    {
        string[] forbiddenTokens =
        [
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Data.SqlClient",
            "StudentRegistration.Infrastructure.SqlServer",
            "DbContext",
            "DbSet<",
            "IEntityTypeConfiguration<",
            "[Key]"
        ];
        var contractSources = Directory
            .EnumerateFiles(
                RepositoryFiles.PathTo("src/StudentRegistration.Contracts"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Select(File.ReadAllText)
            .ToArray();

        Assert.NotEmpty(contractSources);
        foreach (var source in contractSources)
        {
            Assert.All(
                forbiddenTokens,
                forbidden => Assert.DoesNotContain(
                    forbidden,
                    source,
                    StringComparison.Ordinal));
        }
    }

    private static bool IsGeneratedPath(string path)
    {
        var relative = Path.GetRelativePath(RepositoryFiles.Root, path);
        return relative
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");
    }
}
