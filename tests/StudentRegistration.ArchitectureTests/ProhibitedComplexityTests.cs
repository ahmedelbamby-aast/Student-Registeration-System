using System.Text.Json;
using System.Xml.Linq;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class ProhibitedComplexityTests
{
    private static readonly string[] ForbiddenPackageFragments =
    [
        "MassTransit",
        "RabbitMQ",
        "Kafka",
        "EventStore",
        "Dapr",
        "KubernetesClient",
        "Microsoft.Orleans"
    ];

    private static readonly string[] ForbiddenSourceTokens =
    [
        "IRepository<",
        "GenericRepository",
        "IUnitOfWork",
        "EventSourcedAggregate",
        "DynamicRuleDsl",
        "InstitutionWideSolver"
    ];

    [Fact]
    public void Message_broker_and_other_rejected_infrastructure_have_an_explicit_complexity_gate()
    {
        var contract = RepositoryFiles.Read(
            "specs/004-architecture-engineering-principles/contracts/prohibited-complexity.md");

        RepositoryFiles.ContainsAll(
            contract,
            "generic repository",
            "microservices",
            "message broker",
            "durable external consumer",
            "event sourcing",
            "dynamic rule DSL",
            "institution-wide solver",
            "Kubernetes",
            "rejected",
            "separately approved ADR/spec");
    }

    [Fact]
    public void Rejected_packages_and_source_patterns_remain_absent()
    {
        foreach (var projectPath in Directory.EnumerateFiles(
            RepositoryFiles.PathTo("src"),
            "*.csproj",
            SearchOption.AllDirectories))
        {
            var project = XDocument.Load(projectPath);
            var packages = project.Descendants("PackageReference")
                .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty)
                .ToArray();
            Assert.All(
                ForbiddenPackageFragments,
                forbidden => Assert.DoesNotContain(
                    packages,
                    package => package.Contains(forbidden, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var sourcePath in SourceFiles())
        {
            var source = File.ReadAllText(sourcePath);
            Assert.All(
                ForbiddenSourceTokens,
                forbidden => Assert.DoesNotContain(
                    forbidden,
                    source,
                    StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ADR_registry_requires_an_approved_decision_and_architecture_test_update()
    {
        var registry = RepositoryFiles.Read("docs/adr/README.md");
        RepositoryFiles.ContainsAll(
            registry,
            "schemaVersion: 1.0",
            "ownerSpec: SPEC-004",
            "module dependency",
            "deployment decision",
            "approved",
            "Ahmed ELbamby",
            "architecture-test update",
            "ModuleDependencyTests");

        using var schema = JsonDocument.Parse(RepositoryFiles.Read(
            "specs/004-architecture-engineering-principles/schemas/architecture-decision.schema.json"));
        var statuses = schema.RootElement.GetProperty("properties")
            .GetProperty("status")
            .GetProperty("enum")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Equal(["accepted", "superseded"], statuses);

        var acceptedDecision = RepositoryFiles.Read("docs/adr/ADR-001-modular-monolith.md");
        RepositoryFiles.ContainsAll(
            acceptedDecision,
            "Status: Accepted",
            "Approval: Ahmed ELbamby",
            "Rejected alternatives",
            "Revisit trigger");
    }

    private static IEnumerable<string> SourceFiles() =>
        Directory.EnumerateFiles(
                RepositoryFiles.PathTo("src"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(RepositoryFiles.Root, path)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "bin" or "obj"));
}
