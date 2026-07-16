using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

internal static class IdentityRouteVisualAssertions
{
    internal static void AssertFrozenContract(
        string routeId,
        string designPath,
        string taskId,
        string pagePath,
        string targetManifestPath)
    {
        var design = RepositoryFiles.Read(designPath);
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(targetManifestPath));
        var root = manifest.RootElement;

        Assert.Contains(taskId, design, StringComparison.Ordinal);
        Assert.Equal(routeId, root.GetProperty("routeId").GetString());
        Assert.Equal("frontend-fixture/1.0", root.GetProperty("fixtureVersion").GetString());
        Assert.Equal("approved", root.GetProperty("status").GetString());
        Assert.Equal(16, root.GetProperty("targets").GetArrayLength());
        Assert.True(RepositoryFiles.Exists(pagePath));
    }

    internal static async Task AssertBaselineAsync(
        VisualRegressionFixture fixture,
        string routeId,
        string route,
        string heading,
        string browserName,
        int width,
        Func<IPage, Task>? configure = null)
    {
        await using var context = await fixture.OpenContextAsync(browserName, width);
        var page = await context.NewPageAsync();
        if (configure is not null)
        {
            await configure(page);
        }

        await page.GotoAsync(route, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = heading, Exact = true })
            .WaitForAsync();
        await page.EvaluateAsync("() => document.fonts.ready");

        var actual = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true
        });
        var fileName = $"{browserName}-{width}-default.png";
        var folder = $"tests/StudentRegistration.VisualTests/Baselines/Spec007/{routeId}";
        var baselinePath = RepositoryFiles.PathTo($"{folder}/{fileName}");
        if (string.Equals(
                Environment.GetEnvironmentVariable("SPEC007_IDENTITY_BASELINE_APPROVER"),
                "Ahmed ELbamby",
                StringComparison.Ordinal))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllBytesAsync(baselinePath, actual);
            return;
        }

        Assert.True(File.Exists(baselinePath), $"{routeId} baseline is missing: {fileName}");
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(
            $"{folder}/baseline-targets.json"));
        Assert.Equal("approved", manifest.RootElement.GetProperty("status").GetString());
        var target = Assert.Single(
            manifest.RootElement.GetProperty("targets").EnumerateArray(),
            item => item.GetProperty("file").GetString() == fileName);
        var expectedHash = target.GetProperty("sha256").GetString();
        Assert.NotEqual("PENDING", expectedHash);
        Assert.Equal(
            expectedHash,
            Convert.ToHexString(SHA256.HashData(actual)).ToLowerInvariant());
    }
}
