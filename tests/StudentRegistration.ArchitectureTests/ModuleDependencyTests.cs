using System.Xml.Linq;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class ModuleDependencyTests
{
    private static readonly DependencyRule[] Rules =
    [
        new(
            "src/StudentRegistration.Client/StudentRegistration.Client.csproj",
            Allowed()),
        new(
            "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj",
            Allowed()),
        new(
            "src/StudentRegistration.Infrastructure.SqlServer/StudentRegistration.Infrastructure.SqlServer.csproj",
            Allowed("src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj"))
    ];

    [Fact]
    public void Every_source_project_has_an_explicit_dependency_rule()
    {
        var expectedProjects = Rules
            .Select(rule => rule.ProjectPath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actualProjects = Directory
            .EnumerateFiles(
                RepositoryFiles.PathTo("src"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(RepositoryFiles.Root, path)
                .Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedProjects, actualProjects);
    }

    [Fact]
    public void Project_references_match_the_explicit_allowlist()
    {
        foreach (var rule in Rules)
        {
            var project = XDocument.Parse(RepositoryFiles.Read(rule.ProjectPath));
            var actualReferences = project
                .Descendants("ProjectReference")
                .Select(reference => ResolveReference(rule.ProjectPath, reference))
                .Order(StringComparer.Ordinal)
                .ToArray();
            var expectedReferences = rule.AllowedProjectReferences
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedReferences, actualReferences);
        }
    }

    private static IReadOnlySet<string> Allowed(params string[] projectPaths) =>
        new HashSet<string>(projectPaths, StringComparer.Ordinal);

    private static string ResolveReference(
        string ownerProjectPath,
        XElement projectReference)
    {
        var include = projectReference.Attribute("Include")?.Value;
        Assert.False(
            string.IsNullOrWhiteSpace(include),
            $"ProjectReference in {ownerProjectPath} has no Include path.");
        var normalizedInclude = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        var projectDirectory = Path.GetDirectoryName(
            RepositoryFiles.PathTo(ownerProjectPath))!;
        var absoluteReference = Path.GetFullPath(
            Path.Combine(projectDirectory, normalizedInclude));

        return Path.GetRelativePath(RepositoryFiles.Root, absoluteReference)
            .Replace('\\', '/');
    }
}

internal sealed record DependencyRule(
    string ProjectPath,
    IReadOnlySet<string> AllowedProjectReferences);
