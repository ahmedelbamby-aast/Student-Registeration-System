using System.Diagnostics;
using System.Globalization;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec004;

public sealed class LocalArtifactRetentionTests
{
    private const string ScriptPath = "ops/scripts/Remove-ExpiredLocalArtifacts.ps1";

    [Fact]
    public async Task Cleanup_is_repository_bounded_and_removes_only_artifacts_older_than_seven_days()
    {
        var script = RepositoryFiles.Read(ScriptPath);
        RepositoryFiles.ContainsAll(
            script,
            "SupportsShouldProcess",
            "RetentionDays = 7",
            ".AddDays(-$RetentionDays)",
            "Resolve-Path -LiteralPath",
            "Remove-Item -LiteralPath",
            ".local",
            "credentials",
            "logs",
            "exports");

        var ignore = RepositoryFiles.Read(".gitignore");
        RepositoryFiles.ContainsAll(ignore, ".local/", "credentials/", "logs/", "exports/");
        var runbook = RepositoryFiles.Read("ops/runbooks/local-artifact-retention.md");
        RepositoryFiles.ContainsAll(
            runbook,
            "seven days",
            "Git-ignored",
            "WhatIf",
            "repository-bounded");

        var relativeSandbox = $".local/retention-test-{Guid.NewGuid():N}";
        var sandbox = RepositoryFiles.PathTo(relativeSandbox);
        Directory.CreateDirectory(sandbox);
        var expired = Path.Combine(sandbox, "expired.log");
        var current = Path.Combine(sandbox, "current.log");
        var now = DateTimeOffset.UtcNow;

        try
        {
            await File.WriteAllTextAsync(expired, "synthetic expired evidence");
            await File.WriteAllTextAsync(current, "synthetic current evidence");
            File.SetLastWriteTimeUtc(expired, now.AddDays(-8).UtcDateTime);
            File.SetLastWriteTimeUtc(current, now.AddDays(-1).UtcDateTime);

            using var process = new Process
            {
                StartInfo = CreateStartInfo(relativeSandbox, now)
            };
            Assert.True(process.Start());
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            Assert.True(
                process.ExitCode == 0,
                $"Cleanup failed. stdout: {await stdout} stderr: {await stderr}");
            Assert.False(File.Exists(expired));
            Assert.True(File.Exists(current));
        }
        finally
        {
            if (Directory.Exists(sandbox))
            {
                Directory.Delete(sandbox, recursive: true);
            }
        }
    }

    private static ProcessStartInfo CreateStartInfo(
        string relativeSandbox,
        DateTimeOffset now)
    {
        var startInfo = new ProcessStartInfo("pwsh")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(RepositoryFiles.PathTo(ScriptPath));
        startInfo.ArgumentList.Add("-RepositoryRoot");
        startInfo.ArgumentList.Add(RepositoryFiles.Root);
        startInfo.ArgumentList.Add("-ArtifactDirectories");
        startInfo.ArgumentList.Add(relativeSandbox);
        startInfo.ArgumentList.Add("-RetentionDays");
        startInfo.ArgumentList.Add("7");
        startInfo.ArgumentList.Add("-NowUtc");
        startInfo.ArgumentList.Add(now.ToString("O", CultureInfo.InvariantCulture));
        return startInfo;
    }
}
