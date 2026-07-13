using System.Xml.Linq;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec001;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-001-NFR-4.md";

    private static readonly string[] ExpectedProjects =
    [
        "StudentRegistration.Academics",
        "StudentRegistration.Api",
        "StudentRegistration.Client",
        "StudentRegistration.Contracts",
        "StudentRegistration.IdentityAccess",
        "StudentRegistration.Infrastructure.SqlServer",
        "StudentRegistration.Registration",
        "StudentRegistration.Scheduling",
        "StudentRegistration.StaffAdministration"
    ];

    [Fact]
    public void Initial_solution_remains_one_deployable_modular_monolith()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-001/NFR-4",
            "nine governed source projects",
            "one deployable API",
            "sole composition root",
            "explicit and acyclic",
            "two replicas of the same API",
            "no production hosting or topology approval",
            "**Result: PASS.**");

        var projectFiles = Directory
            .EnumerateFiles(
                RepositoryFiles.PathTo("src"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var projectNames = projectFiles
            .Select(path => Path.GetFileNameWithoutExtension(path)!)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(ExpectedProjects.Order(StringComparer.Ordinal), projectNames);

        var webProjects = projectFiles
            .Where(path => File.ReadAllText(path).Contains(
                "Microsoft.NET.Sdk.Web",
                StringComparison.Ordinal))
            .Select(path => Path.GetFileNameWithoutExtension(path)!)
            .ToArray();
        Assert.Equal(["StudentRegistration.Api"], webProjects);

        var compositionRoots = Directory
            .EnumerateFiles(
                RepositoryFiles.PathTo("src"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Where(path => File.ReadAllText(path).Contains(
                "WebApplication.CreateBuilder",
                StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(RepositoryFiles.Root, path))
            .ToArray();
        Assert.Single(compositionRoots);
        Assert.StartsWith(
            "src\\StudentRegistration.Api\\",
            compositionRoots[0],
            StringComparison.OrdinalIgnoreCase);

        var graph = ProjectGraph(projectFiles);
        Assert.Empty(FindCycles(graph));

        var projectText = string.Join(
            Environment.NewLine,
            projectFiles.Select(File.ReadAllText));
        string[] distributedInfrastructureTokens =
        [
            "MassTransit",
            "NServiceBus",
            "Dapr",
            "RabbitMQ",
            "Confluent.Kafka",
            "Microsoft.ServiceFabric"
        ];
        Assert.All(
            distributedInfrastructureTokens,
            token => Assert.DoesNotContain(token, projectText, StringComparison.OrdinalIgnoreCase));

        var replicaEvidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-004-NFR-3.md");
        RepositoryFiles.ContainsAll(
            replicaEvidence,
            "| `api-01` |",
            "| `api-02` |",
            "two application replicas",
            "production SQL topology");
    }

    private static IReadOnlyDictionary<string, string[]> ProjectGraph(
        IEnumerable<string> projectFiles)
    {
        var graph = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var projectFile in projectFiles)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            var document = XDocument.Load(projectFile);
            var references = document
                .Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => Path.GetFileNameWithoutExtension(path!))
                .ToArray();
            Assert.All(
                references,
                reference => Assert.Contains(reference, ExpectedProjects));
            graph.Add(projectName, references);
        }

        return graph;
    }

    private static IReadOnlyList<string> FindCycles(
        IReadOnlyDictionary<string, string[]> graph)
    {
        var indegree = graph.Keys.ToDictionary(
            key => key,
            _ => 0,
            StringComparer.Ordinal);
        foreach (var dependency in graph.Values.SelectMany(value => value))
        {
            indegree[dependency]++;
        }

        var ready = new Queue<string>(indegree
            .Where(item => item.Value == 0)
            .Select(item => item.Key));
        var visited = 0;
        while (ready.TryDequeue(out var project))
        {
            visited++;
            foreach (var dependency in graph[project])
            {
                indegree[dependency]--;
                if (indegree[dependency] == 0)
                {
                    ready.Enqueue(dependency);
                }
            }
        }

        return visited == graph.Count
            ? []
            : indegree.Where(item => item.Value > 0).Select(item => item.Key).ToArray();
    }

    private static bool IsGeneratedPath(string path) =>
        path.Contains(
            $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase) ||
        path.Contains(
            $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase);
}
