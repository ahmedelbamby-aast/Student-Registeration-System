using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public static class Spec003RouteVisualAssertions
{
    public static TheoryData<string, int> BrowserWidths => new()
    {
        { "chrome", 375 }, { "chrome", 768 }, { "chrome", 1280 }, { "chrome", 1920 },
        { "edge", 375 }, { "edge", 768 }, { "edge", 1280 }, { "edge", 1920 },
        { "firefox", 375 }, { "firefox", 768 }, { "firefox", 1280 }, { "firefox", 1920 },
        { "webkit", 375 }, { "webkit", 768 }, { "webkit", 1280 }, { "webkit", 1920 }
    };

    public static void AssertFrozenContract(
        string routeId,
        string taskId,
        string pageName)
    {
        var design = RepositoryFiles.Read(
            $"specs/003-ux-storyboard-accessibility/design/pages/{routeId}.md");
        var page = RepositoryFiles.Read($"src/StudentRegistration.Client/Pages/{pageName}.razor");
        var scopedCssPath = $"src/StudentRegistration.Client/Pages/{pageName}.razor.css";
        var responsiveCss = RepositoryFiles.Exists(scopedCssPath)
            ? RepositoryFiles.Read(scopedCssPath)
            : RepositoryFiles.Read("src/StudentRegistration.Client/wwwroot/css/app.css");

        RepositoryFiles.ContainsAll(
            design,
            $"{routeId}-VIS-{taskId}",
            "\"375\"", "\"768\"", "\"1280\"", "\"1920\"");
        RepositoryFiles.ContainsAll(page, $"data-route-id=\"{routeId}\"", "<h1");
        Assert.Contains("@media", responsiveCss, StringComparison.Ordinal);
        Assert.DoesNotContain("inline-size: 1920px", responsiveCss, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("min-inline-size: 1024px", responsiveCss, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task AssertApprovedBaselineAsync(
        VisualRegressionFixture fixture,
        string routeId,
        string route,
        string browser,
        int width)
    {
        var manifestPath =
            $"tests/StudentRegistration.VisualTests/Baselines/Spec003/{routeId}/baseline-targets.json";
        var approvalRun = string.Equals(
            Environment.GetEnvironmentVariable("SPEC003_ROUTE_BASELINE_APPROVER"),
            "Ahmed ELbamby",
            StringComparison.Ordinal);
        if (!RepositoryFiles.Exists(manifestPath) && !approvalRun)
        {
            return;
        }

        using var manifest = RepositoryFiles.Exists(manifestPath)
            ? JsonDocument.Parse(RepositoryFiles.Read(manifestPath))
            : null;
        if (!approvalRun)
        {
            Assert.Equal("approved", manifest!.RootElement.GetProperty("status").GetString());
            Assert.False(manifest.RootElement.GetProperty("automaticReplacementAllowed").GetBoolean());
            Assert.Equal("Ahmed ELbamby", manifest.RootElement.GetProperty("approvedBy").GetString());
        }

        await using var context = await fixture.OpenContextAsync(browser, width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", api => api.FulfillAsync(new()
        {
            Status = 403,
            ContentType = "application/json",
            Body = "{\"code\":\"FORBIDDEN\",\"message\":\"This route is not authorized.\",\"correlationId\":\"SPEC003-VIS-SAFE-REF\"}"
        }));
        await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator($"[data-route-id='{routeId}']").Last.WaitForAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.EvaluateAsync("() => document.fonts.ready");
        var actual = await page.ScreenshotAsync(new() { FullPage = true });
        var fileName = $"{browser}-{width}-denied.png";
        var baselinePath = RepositoryFiles.PathTo(
            $"tests/StudentRegistration.VisualTests/Baselines/Spec003/{routeId}/{fileName}");
        if (approvalRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllBytesAsync(baselinePath, actual);
            return;
        }

        var target = Assert.Single(
            manifest!.RootElement.GetProperty("targets").EnumerateArray(),
            item => item.GetProperty("browser").GetString() == browser &&
                    item.GetProperty("viewport").GetInt32() == width);
        var expectedHash = target.GetProperty("sha256").GetString();
        Assert.Equal(expectedHash, Convert.ToHexString(SHA256.HashData(actual)).ToLowerInvariant());
    }
}
