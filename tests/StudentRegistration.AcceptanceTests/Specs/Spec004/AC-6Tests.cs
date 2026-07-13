using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec004;

public sealed class AC_6Tests
{
    private static readonly string[] CanonicalProjects =
    [
        "StudentRegistration.Client",
        "StudentRegistration.Api",
        "StudentRegistration.Contracts",
        "StudentRegistration.IdentityAccess",
        "StudentRegistration.Academics",
        "StudentRegistration.Scheduling",
        "StudentRegistration.Registration",
        "StudentRegistration.StaffAdministration",
        "StudentRegistration.Infrastructure.SqlServer"
    ];

    private static readonly string[] BusinessModules =
    [
        "StudentRegistration.IdentityAccess",
        "StudentRegistration.Academics",
        "StudentRegistration.Scheduling",
        "StudentRegistration.Registration",
        "StudentRegistration.StaffAdministration"
    ];

    [Fact]
    public void Solution_uses_the_approved_stack_shape_and_framework_free_domains()
    {
        // Given the approved solution manifest and compiled dependency boundaries.
        var compose = RepositoryFiles.Read("infra/docker/compose.development.yml");
        var sqlFixture = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerContainerFixture.cs");
        var sdk = RepositoryFiles.Read("global.json");
        var clientProject = RepositoryFiles.Read(
            "src/StudentRegistration.Client/StudentRegistration.Client.csproj");
        var infrastructureProject = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/StudentRegistration.Infrastructure.SqlServer.csproj");
        var solution = RepositoryFiles.Read("StudentRegistration.slnx");

        // When architecture conformance evaluates the stack and exact project shape.
        RepositoryFiles.ContainsAll(compose, "mssql/server:2022", "MSSQL_PID", "Developer");
        Assert.DoesNotContain("Enterprise", compose, StringComparison.OrdinalIgnoreCase);
        RepositoryFiles.ContainsAll(
            sqlFixture,
            "MsSql",
            "StudentRegistration_Test_",
            "DisposeAsync");
        RepositoryFiles.ContainsAll(sdk, "10.0.301", "latestPatch");
        RepositoryFiles.ContainsAll(
            clientProject,
            "Microsoft.NET.Sdk.BlazorWebAssembly",
            "Microsoft.AspNetCore.Components.WebAssembly");
        RepositoryFiles.ContainsAll(
            infrastructureProject,
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.SqlServer");

        Assert.All(
            CanonicalProjects,
            project => Assert.Contains(
                $"src/{project}/{project}.csproj",
                solution,
                StringComparison.Ordinal));
        Assert.False(RepositoryFiles.Exists("src/StudentRegistration.Server/StudentRegistration.Server.csproj"));
        Assert.False(RepositoryFiles.Exists("src/StudentRegistration.Domain/StudentRegistration.Domain.csproj"));
        Assert.False(RepositoryFiles.Exists("src/StudentRegistration.Application/StudentRegistration.Application.csproj"));
        Assert.False(RepositoryFiles.Exists("src/StudentRegistration.Infrastructure/StudentRegistration.Infrastructure.csproj"));

        // Then business Domain source contains no web, UI, EF Core, or SQL Server dependency.
        AssertBusinessDomainsAreFrameworkFree();

        var approvedStackChecks = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/ApprovedStackTests.cs");
        var dependencyChecks = RepositoryFiles.Read(
            "tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs");
        RepositoryFiles.ContainsAll(
            approvedStackChecks,
            "BlazorWebAssembly",
            "EntityFrameworkCore",
            "Testcontainers",
            "Assert.");
        Assert.All(
            CanonicalProjects,
            project => Assert.Contains(project, dependencyChecks, StringComparison.Ordinal));
    }

    private static void AssertBusinessDomainsAreFrameworkFree()
    {
        string[] forbiddenTokens =
        [
            "Microsoft.AspNetCore",
            "Microsoft.AspNetCore.Components",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Data.SqlClient"
        ];

        foreach (var module in BusinessModules)
        {
            var domainPath = RepositoryFiles.PathTo($"src/{module}/Domain");
            if (!Directory.Exists(domainPath))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(domainPath, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(file);
                Assert.All(
                    forbiddenTokens,
                    forbidden => Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal));
            }
        }
    }
}
