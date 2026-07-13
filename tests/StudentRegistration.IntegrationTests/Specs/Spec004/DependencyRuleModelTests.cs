using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec004;

public sealed class DependencyRuleModelTests
{
    private const string RulePath =
        "tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs";

    [Fact]
    public void Dependency_rule_has_canonical_ownership_and_an_executable_allowlist_shape()
    {
        using var ownership = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/entity-ownership.json"));
        Assert.Equal(
            RulePath,
            ownership.RootElement.GetProperty("artifactOverrides")
                .GetProperty("004:DependencyRule")
                .GetString());

        var source = RepositoryFiles.Read(RulePath);
        RepositoryFiles.ContainsAll(
            source,
            "internal sealed record DependencyRule(",
            "string ProjectPath",
            "IReadOnlySet<string> AllowedProjectReferences",
            "Every_source_project_has_an_explicit_dependency_rule",
            "Project_references_match_the_explicit_allowlist");
    }
}
