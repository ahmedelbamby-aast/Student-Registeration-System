using System.Diagnostics;
using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SecurityTests;

public sealed class DemoCredentialStorageTests
{
    private static readonly Regex PlaintextCredentialMember = new(
        """(?im)\b(?:public|internal|private|protected)\s+(?:required\s+)?(?:string|char\[\])\s+(?:PlaintextPassword|InitialPassword|InitialPin|GeneratedPin|GeneratedPassword)\b|"(?:plaintextPassword|initialPassword|initialPin|generatedPin|generatedPassword)"\s*:\s*"(?!<|REDACTED|TRANSIENT|HASHED|\$\{)[^"]+"|\[(?:PlaintextPassword|InitialPassword|InitialPin|GeneratedPin|GeneratedPassword)\]""",
        RegexOptions.CultureInvariant);

    private static readonly Regex FullProfileValue = new(
        "(?im)\"(?:nationalId|dateOfBirth|homeAddress|phoneNumber|personalEmail)\"\\s*:\\s*\"(?!<|REDACTED|SYNTHETIC|\\$\\{)[^\"]+\"",
        RegexOptions.CultureInvariant);

    [Fact]
    public void Development_credential_sheet_is_reveal_once_bounded_ignored_and_expiring()
    {
        var writer = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Development/DemoCredentialSheetWriter.cs");
        var gitIgnore = RepositoryFiles.Read(".gitignore");

        RepositoryFiles.ContainsAll(
            writer,
            "DemoCredentialSheetWriter",
            "IHostEnvironment",
            "IsDevelopment()",
            "throw new InvalidOperationException",
            "FileMode.CreateNew",
            "TimeSpan.FromDays(7)",
            "DeleteExpiredAsync",
            ".local",
            "credentials");
        RepositoryFiles.ContainsAll(gitIgnore, ".local/", "credentials/");
        Assert.DoesNotContain("ILogger", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("IsProduction", writer, StringComparison.Ordinal);
    }

    [Fact]
    public void Local_recovery_proof_delivery_is_development_only_bounded_and_never_logged()
    {
        var delivery = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Development/DevelopmentRecoveryProofDelivery.cs");

        RepositoryFiles.ContainsAll(
            delivery,
            "DevelopmentRecoveryProofDelivery",
            "IAccountRecoveryProofDelivery",
            "IsDevelopment()",
            "throw new InvalidOperationException",
            "FileMode.CreateNew",
            "TimeSpan.FromDays(7)",
            "DeleteExpiredAsync",
            ".local",
            "recovery");
        Assert.DoesNotContain("ILogger", delivery, StringComparison.Ordinal);
        Assert.DoesNotContain("Console.", delivery, StringComparison.Ordinal);
    }

    [Fact]
    public void Security_composition_has_no_generated_credential_storage_or_logging_path()
    {
        var securityConfiguration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Operations/SecurityConfiguration.cs");

        RepositoryFiles.ContainsAll(
            securityConfiguration,
            "transient input",
            "ASP.NET Identity",
            "SPEC-007");
        Assert.DoesNotContain("ILogger", securityConfiguration, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", securityConfiguration, StringComparison.Ordinal);
        Assert.DoesNotContain("File.Write", securityConfiguration, StringComparison.Ordinal);
        Assert.DoesNotContain("PasswordHash", securityConfiguration, StringComparison.Ordinal);
    }

    [Fact]
    public void Durable_source_migrations_telemetry_and_evidence_have_no_plaintext_credential_or_full_profile_values()
    {
        foreach (var path in SecurityRepositoryScan.DurableArtifactPaths())
        {
            var content = File.ReadAllText(path);
            var relativePath = Path.GetRelativePath(RepositoryFiles.Root, path)
                .Replace(Path.DirectorySeparatorChar, '/');

            Assert.False(
                PlaintextCredentialMember.IsMatch(content),
                $"Durable plaintext generated-credential shape found in {relativePath}.");
            Assert.False(
                FullProfileValue.IsMatch(content),
                $"Durable full-profile value found in {relativePath}.");
        }
    }

    [Fact]
    public void Approved_identity_contract_requires_hash_only_persistence_and_transient_plaintext()
    {
        var identityRequirements = RepositoryFiles.Read(
            "specs/007-identity-account-lifecycle/requirements.md");
        var operationsRequirements = RepositoryFiles.Read(
            "specs/018-quality-security-scalability-operations/requirements.md");
        var dbContext = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs");

        RepositoryFiles.ContainsAll(
            identityRequirements,
            "Only ASP.NET Core Identity password hashes are persisted",
            "persist plaintext credentials");
        RepositoryFiles.ContainsAll(
            operationsRequirements,
            "only an ASP.NET Identity password hash",
            "plaintext credentials MUST NOT appear in SQL");
        Assert.DoesNotContain("PlaintextPassword", dbContext, StringComparison.Ordinal);
        Assert.DoesNotContain("InitialPin", dbContext, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generated_demo_credential_verifies_through_canonical_identity_hasher_and_sql_contains_only_the_hash()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = RepositoryFiles.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(
            "tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj");
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add(
            "FullyQualifiedName=StudentRegistration.IntegrationTests.Specs.Spec007.IdentityAccountStorePersistenceTests.Real_sql_enforces_unique_identity_and_single_use_lifecycle_transitions");
        startInfo.ArgumentList.Add("--logger");
        startInfo.ArgumentList.Add("console;verbosity=minimal");

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start(), "The focused real-SQL identity proof did not start.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("The focused real-SQL identity proof exceeded five minutes.");
        }

        var output = string.Join(Environment.NewLine, await outputTask, await errorTask);
        Assert.True(process.ExitCode == 0, output);
        Assert.Matches(@"Failed:\s+0\b", output);
        Assert.Matches(@"Passed:\s+1\b", output);
    }
}

internal static class SecurityRepositoryScan
{
    private static readonly string[] DurableRoots =
    {
        "src",
        "docs/release-evidence"
    };

    private static readonly HashSet<string> DurableExtensions = new(
        new[] { ".cs", ".json", ".sql", ".xml", ".md" },
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> TrackedRelativePaths()
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = RepositoryFiles.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("ls-files");

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.True(process.ExitCode == 0, $"git ls-files failed: {error}");
        return output
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => path.Replace('\\', '/'))
            .ToArray();
    }

    public static IEnumerable<string> DurableArtifactPaths()
    {
        foreach (var root in DurableRoots.Select(RepositoryFiles.PathTo).Where(Directory.Exists))
        {
            foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                if (IsGeneratedPath(path) || !DurableExtensions.Contains(Path.GetExtension(path)))
                {
                    continue;
                }

                yield return path;
            }
        }
    }

    private static bool IsGeneratedPath(string path)
    {
        var segments = Path.GetRelativePath(RepositoryFiles.Root, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("TestResults", StringComparison.OrdinalIgnoreCase));
    }
}
