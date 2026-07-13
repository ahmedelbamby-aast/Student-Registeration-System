using System.Text.Json;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.MigrationTests;

public sealed class MigrationBundleTests
{
    private const string ReadmePath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/README.md";

    [Fact]
    public void Controlled_migration_contract_is_reviewed_ordered_and_never_runs_at_startup()
    {
        var contract = RepositoryFiles.Read(ReadmePath);

        RepositoryFiles.ContainsAll(
            contract,
            "controlled-migrations/1.0",
            "reviewed script or bundle",
            "no application-startup migration",
            "pinned source and migration version",
            "artifact hash",
            "reviewer approval",
            "operator approval",
            "backup reference",
            "restore verification",
            "forward rehearsal",
            "rollback rehearsal",
            "no bootstrap, seed, or reset in production",
            "No production bundle or migration execution is approved by this document.");

        using var manifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/persistence-manifest.json"));
        var migrations = manifest.RootElement.GetProperty("migrations")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(
            [
                "S1IdentityAcademicFoundation",
                "S2CatalogueScheduling",
                "S4DiscoveryPlanning",
                "S6Registration",
                "S7StaffAdminOperations"
            ],
            migrations.Select(migration => migration.GetProperty("id").GetString()!).ToArray());
        Assert.All(
            migrations,
            migration => Assert.StartsWith(
                "src/StudentRegistration.Infrastructure.SqlServer/Migrations/",
                migration.GetProperty("path").GetString()!,
                StringComparison.Ordinal));
        foreach (var migration in migrations)
        {
            RepositoryFiles.ContainsAll(
                contract,
                migration.GetProperty("id").GetString()!,
                migration.GetProperty("path").GetString()!,
                $"SPEC-{migration.GetProperty("owner").GetString()}");
        }

        var apiSource = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(
                    RepositoryFiles.PathTo("src/StudentRegistration.Api"),
                    "*.cs",
                    SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.DoesNotMatch(
            new Regex(@"\.(?:Migrate|MigrateAsync|EnsureCreated|EnsureCreatedAsync)\s*\(",
                RegexOptions.CultureInvariant),
            apiSource);
    }
}
