using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec004;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-004-NFR-4.md";

    private static readonly string[] BusinessModules =
    [
        "StudentRegistration.IdentityAccess",
        "StudentRegistration.Academics",
        "StudentRegistration.Scheduling",
        "StudentRegistration.Registration",
        "StudentRegistration.StaffAdministration"
    ];

    private static readonly FrameworkRule[] ForbiddenFrameworkRules =
    [
        new(
            "ASP.NET or Blazor",
            @"\bMicrosoft\.AspNetCore(?:\.[A-Za-z0-9_]+)*\b|" +
            @"\b(?:ComponentBase|RenderFragment|ControllerBase|HttpContext|IActionResult)\b"),
        new(
            "EF Core",
            @"\bMicrosoft\.EntityFrameworkCore(?:\.[A-Za-z0-9_]+)*\b|" +
            @"\b(?:DbContext|DbSet|IEntityTypeConfiguration|EntityTypeBuilder|UseSqlServer)\b"),
        new(
            "SQL Server",
            @"\b(?:Microsoft\.Data\.SqlClient|System\.Data\.SqlClient)(?:\.[A-Za-z0-9_]+)*\b|" +
            @"\bSql(?:Connection|Command|Parameter|DataReader|Transaction)\b")
    ];

    [Fact]
    public void Every_business_module_domain_is_free_of_web_ui_ef_and_sql_dependencies()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-004/NFR-4",
            "five business modules",
            "ASP.NET",
            "Blazor",
            "EF Core",
            "SQL Server",
            "controlled negative fixture");

        Assert.Equal(5, BusinessModules.Length);
        foreach (var module in BusinessModules)
        {
            Assert.Contains(module, evidence, StringComparison.Ordinal);
            Assert.True(
                RepositoryFiles.Exists($"src/{module}/{module}.csproj"),
                $"The governed business-module project is missing: {module}");

            foreach (var sourcePath in EnumerateDomainSource(module))
            {
                var source = File.ReadAllText(sourcePath);
                var violations = FindFrameworkViolations(source);
                Assert.True(
                    violations.Count == 0,
                    $"{Path.GetRelativePath(RepositoryFiles.Root, sourcePath)} contains " +
                    $"forbidden Domain dependencies: {string.Join(", ", violations)}");
            }
        }
    }

    [Fact]
    public void Validator_rejects_a_controlled_prohibited_sample_and_accepts_plain_domain_code()
    {
        const string prohibitedSample = """
            using Microsoft.AspNetCore.Components;
            using Microsoft.EntityFrameworkCore;
            using Microsoft.Data.SqlClient;

            internal sealed class ProhibitedDomainSample : ComponentBase
            {
                private DbContext? Context { get; init; }
                private SqlConnection? Connection { get; init; }
            }
            """;

        var violations = FindFrameworkViolations(prohibitedSample);
        Assert.Equal(
            ["ASP.NET or Blazor", "EF Core", "SQL Server"],
            violations.Order(StringComparer.Ordinal));

        const string plainDomainSample = """
            namespace Example.Domain;

            internal sealed record StudentPlanId(Guid Value);
            """;
        Assert.Empty(FindFrameworkViolations(plainDomainSample));
    }

    private static IEnumerable<string> EnumerateDomainSource(string module)
    {
        var domainRoot = RepositoryFiles.PathTo($"src/{module}/Domain");
        return Directory.Exists(domainRoot)
            ? Directory.EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories)
            : [];
    }

    private static IReadOnlyList<string> FindFrameworkViolations(string source) =>
        ForbiddenFrameworkRules
            .Where(rule => Regex.IsMatch(
                source,
                rule.Pattern,
                RegexOptions.CultureInvariant))
            .Select(rule => rule.Category)
            .ToArray();

    private sealed record FrameworkRule(string Category, string Pattern);
}
