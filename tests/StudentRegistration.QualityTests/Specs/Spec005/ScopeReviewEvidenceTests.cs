using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class ScopeReviewEvidenceTests
{
    [Fact]
    public void Scope_review_records_all_four_exclusions_and_executable_evidence()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-005-scope-review.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-005 Scope Review",
            "**Result: PASS.**",
            "OS-1 — Database-per-module or read replica",
            "OS-2 — Hard deletion or production retention schedule",
            "OS-3 — Automatic correction of invalid curriculum imports",
            "OS-4 — Direct production schema mutation outside migrations",
            "ScopeReviewEvidenceTests",
            "PersistenceBoundaryTests");
    }

    [Fact]
    public void Single_context_and_no_replica_configuration_preserve_os_1()
    {
        var contexts = Directory
            .EnumerateFiles(RepositoryFiles.PathTo("src"), "*DbContext.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path))
            .Select(path => Path.GetRelativePath(RepositoryFiles.Root, path).Replace('\\', '/'))
            .ToArray();

        Assert.Equal(
            ["src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs"],
            contexts);

        foreach (var path in SourceFiles())
        {
            Assert.DoesNotContain("ReadReplica", File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Retention_and_invalid_imports_remain_fail_closed()
    {
        var history = RepositoryFiles.Read(
            "specs/005-erd-data-lifecycle/contracts/immutable-history.md");
        var imports = RepositoryFiles.Read(
            "docs/data/import-provenance-contract.md");
        var invariants = RepositoryFiles.Read(
            "specs/005-erd-data-lifecycle/contracts/check-constraints.md");

        RepositoryFiles.ContainsAll(
            history,
            "Hard deletion and retention for real or production data remain unapproved",
            "fail closed");
        RepositoryFiles.ContainsAll(
            imports,
            "Missing prerequisites reject preview and keep publication blocked",
            "Production data",
            "classification, retention, deletion",
            "remain fail closed");
        RepositoryFiles.ContainsAll(
            invariants,
            "rejected, never clamped or auto-corrected");
    }

    [Fact]
    public void Schema_changes_are_migration_owned_and_development_bootstrap_is_guarded()
    {
        var migrationContract = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Migrations/README.md");
        var initializer = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Development/DemoDatabaseInitializer.cs");

        RepositoryFiles.ContainsAll(
            migrationContract,
            "There is no application-startup migration",
            "reviewed script or bundle",
            "authorized operator");
        RepositoryFiles.ContainsAll(
            initializer,
            "if (!_environment.IsDevelopment())",
            "Synthetic initialization requires the exact Development database target",
            "_dbContext.Database.MigrateAsync");

        var migrationCalls = SourceFiles()
            .Where(path => File.ReadAllText(path).Contains(
                ".Database.MigrateAsync(",
                StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(RepositoryFiles.Root, path).Replace('\\', '/'))
            .ToArray();
        Assert.Equal(
            ["src/StudentRegistration.Api/Development/DemoDatabaseInitializer.cs"],
            migrationCalls);

        foreach (var path in SourceFiles().Where(
                     path => !path.Contains(
                         $"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}",
                         StringComparison.OrdinalIgnoreCase)))
        {
            var source = File.ReadAllText(path);
            Assert.DoesNotMatch(@"(?i)\b(CREATE|ALTER|DROP)\s+(TABLE|SCHEMA|INDEX)\b", source);
        }
    }

    private static IEnumerable<string> SourceFiles() =>
        Directory.EnumerateFiles(RepositoryFiles.PathTo("src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedPath(path));

    private static bool IsGeneratedPath(string path) =>
        Path.GetRelativePath(RepositoryFiles.Root, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");
}
