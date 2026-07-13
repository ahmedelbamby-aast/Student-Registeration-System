using System.Xml.Linq;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class ModuleDependencyTests
{
    private static readonly string[] BusinessModules =
    [
        "IdentityAccess",
        "Academics",
        "Scheduling",
        "Registration",
        "StaffAdministration"
    ];

    private static readonly DependencyRule[] Rules =
    [
        new(
            "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj",
            Allowed()),
        new(
            "src/StudentRegistration.Client/StudentRegistration.Client.csproj",
            Allowed("src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj")),
        new(
            "src/StudentRegistration.IdentityAccess/StudentRegistration.IdentityAccess.csproj",
            Allowed("src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj")),
        new(
            "src/StudentRegistration.Academics/StudentRegistration.Academics.csproj",
            Allowed("src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj")),
        new(
            "src/StudentRegistration.Scheduling/StudentRegistration.Scheduling.csproj",
            Allowed("src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj")),
        new(
            "src/StudentRegistration.Registration/StudentRegistration.Registration.csproj",
            Allowed(
                "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj",
                "src/StudentRegistration.IdentityAccess/StudentRegistration.IdentityAccess.csproj",
                "src/StudentRegistration.Academics/StudentRegistration.Academics.csproj",
                "src/StudentRegistration.Scheduling/StudentRegistration.Scheduling.csproj")),
        new(
            "src/StudentRegistration.StaffAdministration/StudentRegistration.StaffAdministration.csproj",
            Allowed(
                "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj",
                "src/StudentRegistration.Academics/StudentRegistration.Academics.csproj",
                "src/StudentRegistration.Scheduling/StudentRegistration.Scheduling.csproj")),
        new(
            "src/StudentRegistration.Infrastructure.SqlServer/StudentRegistration.Infrastructure.SqlServer.csproj",
            Allowed(
                "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj",
                "src/StudentRegistration.IdentityAccess/StudentRegistration.IdentityAccess.csproj",
                "src/StudentRegistration.Academics/StudentRegistration.Academics.csproj",
                "src/StudentRegistration.Scheduling/StudentRegistration.Scheduling.csproj",
                "src/StudentRegistration.Registration/StudentRegistration.Registration.csproj",
                "src/StudentRegistration.StaffAdministration/StudentRegistration.StaffAdministration.csproj")),
        new(
            "src/StudentRegistration.Api/StudentRegistration.Api.csproj",
            Allowed(
                "src/StudentRegistration.Client/StudentRegistration.Client.csproj",
                "src/StudentRegistration.Contracts/StudentRegistration.Contracts.csproj",
                "src/StudentRegistration.IdentityAccess/StudentRegistration.IdentityAccess.csproj",
                "src/StudentRegistration.Academics/StudentRegistration.Academics.csproj",
                "src/StudentRegistration.Scheduling/StudentRegistration.Scheduling.csproj",
                "src/StudentRegistration.Registration/StudentRegistration.Registration.csproj",
                "src/StudentRegistration.StaffAdministration/StudentRegistration.StaffAdministration.csproj",
                "src/StudentRegistration.Infrastructure.SqlServer/StudentRegistration.Infrastructure.SqlServer.csproj"))
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

    [Fact]
    public void Declared_project_graph_is_governed_self_reference_free_and_acyclic()
    {
        var governedProjects = Rules
            .Select(rule => rule.ProjectPath)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var rule in Rules)
        {
            Assert.DoesNotContain(rule.ProjectPath, rule.AllowedProjectReferences);
            Assert.All(
                rule.AllowedProjectReferences,
                reference => Assert.Contains(reference, governedProjects));
        }

        Assert.False(ContainsCycle(Rules));
    }

    [Fact]
    public void Forbidden_reference_and_cycle_fixtures_are_rejected()
    {
        const string contracts = "StudentRegistration.Contracts";
        const string academics = "StudentRegistration.Academics";
        const string registration = "StudentRegistration.Registration";
        DependencyRule[] allowlist =
        [
            new(contracts, Allowed()),
            new(academics, Allowed(contracts)),
            new(registration, Allowed(contracts))
        ];
        IReadOnlyDictionary<string, IReadOnlySet<string>> forbiddenActualGraph =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
            {
                [contracts] = Allowed(),
                [academics] = Allowed(contracts),
                [registration] = Allowed(contracts, academics)
            };

        var forbiddenReferences = FindForbiddenReferences(
            allowlist,
            forbiddenActualGraph);

        Assert.Contains(
            $"{registration} -> {academics}",
            forbiddenReferences);

        DependencyRule[] cycle =
        [
            new(academics, Allowed(registration)),
            new(registration, Allowed(academics))
        ];
        Assert.True(ContainsCycle(cycle));
    }

    [Fact]
    public void Business_modules_do_not_reference_other_module_Internals()
    {
        foreach (var consumer in BusinessModules)
        {
            var projectDirectory = RepositoryFiles.PathTo(
                $"src/StudentRegistration.{consumer}");
            foreach (var file in SourceFiles(projectDirectory))
            {
                var source = File.ReadAllText(file);
                Assert.DoesNotContain("InternalsVisibleTo", source, StringComparison.Ordinal);

                foreach (var provider in BusinessModules.Where(
                    provider => !string.Equals(provider, consumer, StringComparison.Ordinal)))
                {
                    Assert.DoesNotContain(
                        $"StudentRegistration.{provider}.Internal",
                        source,
                        StringComparison.Ordinal);

                    foreach (Match match in Regex.Matches(
                        source,
                        $@"StudentRegistration\.{Regex.Escape(provider)}\.(?<surface>[A-Za-z0-9_.]+)"))
                    {
                        Assert.StartsWith(
                            "Application.Ports",
                            match.Groups["surface"].Value,
                            StringComparison.Ordinal);
                    }
                }
            }
        }
    }

    [Fact]
    public void Canonical_boundary_record_matches_the_executable_allowlist()
    {
        var boundaries = RepositoryFiles.Read("docs/architecture/module-boundaries.md");

        foreach (var rule in Rules)
        {
            var project = Path.GetFileNameWithoutExtension(rule.ProjectPath);
            var references = rule.AllowedProjectReferences.Count == 0
                ? "None"
                : string.Join(", ", rule.AllowedProjectReferences
                    .Select(Path.GetFileNameWithoutExtension)
                    .Order(StringComparer.Ordinal));

            Assert.Contains(
                $"| {project} | {references} |",
                boundaries,
                StringComparison.Ordinal);
        }
    }

    private static IReadOnlySet<string> Allowed(params string[] projectPaths) =>
        new HashSet<string>(projectPaths, StringComparer.Ordinal);

    private static bool ContainsCycle(IEnumerable<DependencyRule> rules)
    {
        var graph = rules.ToDictionary(
            rule => rule.ProjectPath,
            rule => rule.AllowedProjectReferences,
            StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        return graph.Keys.Any(Visit);

        bool Visit(string project)
        {
            if (visited.Contains(project))
            {
                return false;
            }

            if (!visiting.Add(project))
            {
                return true;
            }

            if (graph[project].Any(Visit))
            {
                return true;
            }

            visiting.Remove(project);
            visited.Add(project);
            return false;
        }
    }

    private static IReadOnlyList<string> FindForbiddenReferences(
        IEnumerable<DependencyRule> rules,
        IReadOnlyDictionary<string, IReadOnlySet<string>> actualGraph)
    {
        var allowedGraph = rules.ToDictionary(
            rule => rule.ProjectPath,
            rule => rule.AllowedProjectReferences,
            StringComparer.Ordinal);

        return actualGraph
            .SelectMany(pair => pair.Value
                .Where(reference =>
                    !allowedGraph.TryGetValue(pair.Key, out var allowed) ||
                    !allowed.Contains(reference))
                .Select(reference => $"{pair.Key} -> {reference}"))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> SourceFiles(string projectDirectory) =>
        Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(projectDirectory, path)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "bin" or "obj"));

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
