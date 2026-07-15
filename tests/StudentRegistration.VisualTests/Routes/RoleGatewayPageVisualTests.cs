using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class RoleGatewayPageVisualFrozenContractTests
{
    [Fact]
    public void Auth_01_visual_target_matrix_is_complete_and_owner_source_exists()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-01.md");
        using var targets = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/Spec008/AUTH-01/baseline-targets.json"));
        var root = targets.RootElement;

        Assert.Contains("AUTH-01-VIS-T126", design, StringComparison.Ordinal);
        Assert.Equal("AUTH-01", root.GetProperty("routeId").GetString());
        Assert.Equal("frontend-fixture/1.0", root.GetProperty("fixtureVersion").GetString());
        Assert.Equal(16, root.GetProperty("targets").GetArrayLength());
        Assert.Contains(
            root.GetProperty("status").GetString(),
            new[] { "pending-approval", "approved" });
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RoleGatewayPage.razor"),
            "AUTH-01-VIS-T126: the baseline owner page is still missing.");
    }
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class RoleGatewayPageVisualTests(VisualRegressionFixture fixture)
{
    private const string AvailableContext = """
        {
          "serverTimeUtc": "2026-07-14T10:15:00Z",
          "timeZoneId": "Africa/Cairo",
          "teachingTermLabel": "Summer 2026",
          "registrationTermLabel": "Fall 2026",
          "registrationWindowState": "open",
          "serviceState": "available"
        }
        """;

    [Theory]
    [InlineData("chrome", 375)]
    [InlineData("chrome", 768)]
    [InlineData("chrome", 1280)]
    [InlineData("chrome", 1920)]
    [InlineData("edge", 375)]
    [InlineData("edge", 768)]
    [InlineData("edge", 1280)]
    [InlineData("edge", 1920)]
    [InlineData("firefox", 375)]
    [InlineData("firefox", 768)]
    [InlineData("firefox", 1280)]
    [InlineData("firefox", 1920)]
    [InlineData("webkit", 375)]
    [InlineData("webkit", 768)]
    [InlineData("webkit", 1280)]
    [InlineData("webkit", 1920)]
    public async Task Auth_01_matches_the_approved_cross_browser_baseline(
        string browserName,
        int width)
    {
        await using var context = await fixture.OpenContextAsync(browserName, width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/public/context", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = AvailableContext
            }));
        await page.GotoAsync("/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Role gateway", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-route-id='AUTH-01'][data-state='success']")
            .WaitForAsync();
        await page.EvaluateAsync("() => document.fonts.ready");

        var actual = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true
        });
        var fileName = $"{browserName}-{width}-success.png";
        var baselinePath = RepositoryFiles.PathTo(
            $"tests/StudentRegistration.VisualTests/Baselines/Spec008/AUTH-01/{fileName}");
        if (string.Equals(
                Environment.GetEnvironmentVariable("SPEC008_AUTH01_BASELINE_APPROVER"),
                "Ahmed ELbamby",
                StringComparison.Ordinal))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllBytesAsync(baselinePath, actual);
            return;
        }

        Assert.True(
            File.Exists(baselinePath),
            $"AUTH-01-VIS-T126 baseline is not approved yet: {fileName}");

        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/Spec008/AUTH-01/baseline-targets.json"));
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
