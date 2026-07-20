using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class TermAdministrationPageVisualFrozenContractTests
{
    private const string ManifestPath =
        "tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-02/baseline-targets.json";

    [Fact]
    public void Adm_02_visual_target_matrix_has_all_16_governed_targets_and_owner_source()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-02.md");
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = manifest.RootElement;

        Assert.Contains("ADM-02-VIS-T196", design, StringComparison.Ordinal);
        Assert.Equal("ADM-02", root.GetProperty("routeId").GetString());
        Assert.Equal("frontend-fixture/1.0", root.GetProperty("fixtureVersion").GetString());
        Assert.Equal("design-token-contract/1.0", root.GetProperty("tokenVersion").GetString());
        var status = root.GetProperty("status").GetString();
        Assert.Contains(status, new[] { "pending-approval", "approved" });
        Assert.False(root.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", root.GetProperty("approvalAuthority").GetString());

        var targets = root.GetProperty("targets").EnumerateArray().ToArray();
        Assert.Equal(16, targets.Length);
        var expected = new[]
        {
            "chrome-375", "chrome-768", "chrome-1280", "chrome-1920",
            "edge-375", "edge-768", "edge-1280", "edge-1920",
            "firefox-375", "firefox-768", "firefox-1280", "firefox-1920",
            "webkit-375", "webkit-768", "webkit-1280", "webkit-1920"
        };
        Assert.Equal(
            expected,
            targets.Select(target =>
                $"{target.GetProperty("browser").GetString()}-{target.GetProperty("viewport").GetInt32()}")
                .ToArray());
        Assert.All(targets, target =>
        {
            if (string.Equals(status, "pending-approval", StringComparison.Ordinal))
            {
                Assert.Equal("PENDING", target.GetProperty("browserName").GetString());
                Assert.Equal("PENDING", target.GetProperty("browserBuild").GetString());
                Assert.Equal("PENDING", target.GetProperty("engine").GetString());
                Assert.Equal("PENDING", target.GetProperty("sha256").GetString());
                Assert.Equal("PENDING", target.GetProperty("artifactSha256").GetString());
            }
            else
            {
                Assert.NotEqual("PENDING", target.GetProperty("browserName").GetString());
                Assert.NotEqual("PENDING", target.GetProperty("browserBuild").GetString());
                Assert.NotEqual("PENDING", target.GetProperty("engine").GetString());
                Assert.NotEqual("PENDING", target.GetProperty("sha256").GetString());
                Assert.Equal(
                    target.GetProperty("sha256").GetString(),
                    target.GetProperty("artifactSha256").GetString());
            }
        });
        if (string.Equals(status, "approved", StringComparison.Ordinal))
        {
            Assert.Equal("Ahmed ELbamby", root.GetProperty("approvedBy").GetString());
            Assert.Equal("2026-07-19", root.GetProperty("approvedOn").GetString());
        }
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor"),
            "ADM-02-VIS-T196: the baseline owner page is still missing until SPEC-008/T080.");
    }
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class TermAdministrationPageVisualTests(VisualRegressionFixture fixture)
{
    private const string ManifestPath =
        "tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-02/baseline-targets.json";
    private const string DraftPage = """
        {
          "items": [
            {
              "id": "term-fall-2026",
              "code": "2026-FALL",
              "displayName": "Fall 2026",
              "timeZoneId": "Africa/Cairo",
              "teachingStartsOn": "2026-09-13",
              "teachingEndsOn": "2027-01-14",
              "state": "draft",
              "rowVersion": "term-rv-1",
              "windows": [
                {
                  "id": "window-all",
                  "scopeType": "all-students",
                  "scopeValue": null,
                  "opensAtUtc": "2026-08-20T06:00:00Z",
                  "closesAtUtc": "2026-08-27T18:00:00Z",
                  "lifecycleState": "draft",
                  "computedState": "upcoming",
                  "rowVersion": "window-rv-1"
                }
              ]
            }
          ],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "code,id"
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
    public async Task Adm_02_matches_the_Ahmed_approved_cross_browser_baseline(
        string browserName,
        int width)
    {
        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var approvalRun = string.Equals(
            Environment.GetEnvironmentVariable("SPEC008_ADM02_BASELINE_APPROVER"),
            "Ahmed ELbamby",
            StringComparison.Ordinal);
        if (manifest.RootElement.GetProperty("status").GetString() == "pending-approval" &&
            !approvalRun)
        {
            var pendingTarget = Assert.Single(
                manifest.RootElement.GetProperty("targets").EnumerateArray(),
                item =>
                    item.GetProperty("browser").GetString() == browserName &&
                    item.GetProperty("viewport").GetInt32() == width);
            Assert.Equal("PENDING", pendingTarget.GetProperty("sha256").GetString());
            Assert.Equal("PENDING", pendingTarget.GetProperty("artifactSha256").GetString());
            return;
        }

        await using var context = await fixture.OpenContextAsync(browserName, width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/admin/terms**", route => route.FulfillAsync(
            new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = DraftPage
            }));
        await page.GotoAsync("/admin/terms", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Term administration", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-route-id='ADM-02'][data-state='success']")
            .WaitForAsync();
        await page.EvaluateAsync("() => document.fonts.ready");

        var actual = await StableVisualCapture.CaptureAsync(
            page, "ADM-02", browserName, width);
        var fileName = $"{browserName}-{width}-success.png";
        var baselinePath = RepositoryFiles.PathTo(
            $"tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-02/{fileName}");
        if (approvalRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllBytesAsync(baselinePath, actual);
            return;
        }

        Assert.Equal("approved", manifest.RootElement.GetProperty("status").GetString());
        Assert.True(
            File.Exists(baselinePath),
            $"ADM-02-VIS-T196 approved baseline is missing: {fileName}");
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
