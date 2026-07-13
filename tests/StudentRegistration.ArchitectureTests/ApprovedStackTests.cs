using System.Text.Json;
using System.Xml.Linq;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class ApprovedStackTests
{
    private const string SqlServerImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";

    [Fact]
    public void Net10_aspnet_blazor_ef_sqlserver_and_bcl_linq_are_pinned()
    {
        using var global = JsonDocument.Parse(RepositoryFiles.Read("global.json"));
        var sdk = global.RootElement.GetProperty("sdk");
        Assert.Equal("10.0.301", sdk.GetProperty("version").GetString());
        Assert.Equal("latestPatch", sdk.GetProperty("rollForward").GetString());
        Assert.False(sdk.GetProperty("allowPrerelease").GetBoolean());

        var client = ReadProject(
            "src/StudentRegistration.Client/StudentRegistration.Client.csproj");
        Assert.Equal("Microsoft.NET.Sdk.BlazorWebAssembly", client.Root?.Attribute("Sdk")?.Value);
        Assert.Equal("net10.0", TargetFramework(client));
        Assert.Equal(
            "10.0.9",
            PackageVersion(client, "Microsoft.AspNetCore.Components.WebAssembly"));

        var infrastructure = ReadProject(
            "src/StudentRegistration.Infrastructure.SqlServer/StudentRegistration.Infrastructure.SqlServer.csproj");
        Assert.Equal("net10.0", TargetFramework(infrastructure));
        Assert.Equal(
            "10.0.9",
            PackageVersion(infrastructure, "Microsoft.EntityFrameworkCore.SqlServer"));
        Assert.Equal(
            "enable",
            infrastructure.Descendants("ImplicitUsings").Single().Value);
        Assert.DoesNotContain(
            infrastructure.Descendants("PackageReference"),
            reference => string.Equals(
                reference.Attribute("Include")?.Value,
                "System.Linq",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Development_compose_pins_the_nonproduction_developer_runtime()
    {
        var compose = RepositoryFiles.Read("infra/docker/compose.development.yml");

        RepositoryFiles.ContainsAll(
            compose,
            "Non-production demo only.",
            "does not define or approve a production SQL Server edition or topology.",
            SqlServerImage,
            "ACCEPT_EULA: \"Y\"",
            "MSSQL_PID: \"Developer\"",
            "SRS_SQL_SA_PASSWORD:?Set SRS_SQL_SA_PASSWORD outside Git",
            "development-sql-data:/var/opt/mssql",
            "SQLCMDPASSWORD");
        Assert.DoesNotContain("2022-latest", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("MSSQL_PID: \"Standard", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("MSSQL_PID: \"Enterprise", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("deploy:", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("replicas:", compose, StringComparison.Ordinal);
        Assert.DoesNotContain("yourStrong", compose, StringComparison.Ordinal);
    }

    [Fact]
    public void Testing_uses_a_bounded_disposable_testcontainer_and_live_runtime_assertions()
    {
        var integrationProject = ReadProject(
            "tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj");
        Assert.Equal(
            "4.13.0",
            PackageVersion(integrationProject, "Testcontainers.MsSql"));

        var fixture = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerContainerFixture.cs");
        RepositoryFiles.ContainsAll(
            fixture,
            SqlServerImage,
            "new MsSqlBuilder(SqlServerImage)",
            ".WithEnvironment(\"MSSQL_PID\", \"Developer\")",
            "RandomNumberGenerator.GetBytes",
            "NullLogger.Instance",
            "TimeSpan.FromMinutes(5)",
            "ProductMajorVersion",
            "Developer Edition",
            "COMPATIBILITY_LEVEL = 160",
            "SPEC004_SQL_RUNTIME_OK",
            "DisposeAsync()");
        Assert.DoesNotContain("new MsSqlBuilder()", fixture, StringComparison.Ordinal);
        Assert.DoesNotContain("2022-latest", fixture, StringComparison.Ordinal);
        Assert.DoesNotContain("WithReuse", fixture, StringComparison.Ordinal);
        Assert.DoesNotContain("yourStrong", fixture, StringComparison.Ordinal);
    }

    private static XDocument ReadProject(string path) =>
        XDocument.Parse(RepositoryFiles.Read(path));

    private static string TargetFramework(XDocument project) =>
        project.Descendants("TargetFramework").Single().Value;

    private static string PackageVersion(XDocument project, string packageName)
    {
        var reference = project.Descendants("PackageReference")
            .SingleOrDefault(candidate => string.Equals(
                candidate.Attribute("Include")?.Value,
                packageName,
                StringComparison.Ordinal));
        Assert.True(reference is not null, $"Package is not referenced: {packageName}");
        var version = reference!.Attribute("Version")?.Value
            ?? reference.Element("Version")?.Value;
        Assert.False(string.IsNullOrWhiteSpace(version), $"Package is not pinned: {packageName}");
        return version!;
    }
}
