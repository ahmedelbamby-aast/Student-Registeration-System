using System.Diagnostics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec004;

public sealed class NFR_1EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-004-NFR-1.md";
    private const string ArchitectureProject =
        "tests/StudentRegistration.ArchitectureTests/StudentRegistration.ArchitectureTests.csproj";
    private const string ArchitectureFixture =
        "StudentRegistration.ArchitectureTests.ModuleDependencyTests.Forbidden_reference_and_cycle_fixtures_are_rejected";

    [Fact]
    public async Task Forbidden_reference_and_cycle_fault_injections_have_measurable_release_evidence()
    {
        var result = await RunArchitectureFixtureAsync();

        Assert.True(
            result.ExitCode == 0,
            $"The focused architecture fixture failed with exit code {result.ExitCode}.{Environment.NewLine}{result.Output}");
        Assert.Matches(@"Failed:\s+0\b", result.Output);
        Assert.Matches(@"Passed:\s+1\b", result.Output);

        var evidence = RepositoryFiles.Read(EvidencePath);
        var measuredResult = RepositoryFiles.Section(evidence, "Measured result");

        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-004 NFR-1",
            ArchitectureFixture,
            "dotnet test",
            "--filter",
            "**Result: PASS.**");
        RepositoryFiles.ContainsAll(
            measuredResult,
            "| Focused architecture fixtures executed | 1 |",
            "| Forbidden-reference violations injected | 1 |",
            "| Cyclic dependency graphs injected | 1 |",
            "| Invalid dependency conditions rejected | 2 |",
            "| Failed architecture fixtures | 0 |");
    }

    private static async Task<ProcessResult> RunArchitectureFixtureAsync()
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
        startInfo.ArgumentList.Add(ArchitectureProject);
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add($"FullyQualifiedName={ArchitectureFixture}");
        startInfo.ArgumentList.Add("--logger");
        startInfo.ArgumentList.Add("console;verbosity=minimal");

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start(), "The focused architecture-test process did not start.");

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException(
                "The focused architecture fixture exceeded the two-minute evidence timeout.");
        }

        var output = string.Join(
            Environment.NewLine,
            await standardOutput,
            await standardError);
        return new ProcessResult(process.ExitCode, output);
    }

    private sealed record ProcessResult(int ExitCode, string Output);
}
