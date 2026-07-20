using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class StudentAdministrationPageVisualFrozenContractTests
{
    private const string ManifestPath =
        "tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-04/baseline-targets.json";

    [Fact]
    public void Adm04_visual_target_matrix_has_exactly_sixteen_governed_targets()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-04.md");
        using var targets = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var root = targets.RootElement;

        Assert.Contains("ADM-04-VIS-T206", design, StringComparison.Ordinal);
        Assert.Equal("ADM-04", root.GetProperty("routeId").GetString());
        Assert.Equal("success", root.GetProperty("state").GetString());
        Assert.Equal("frontend-fixture/1.0", root.GetProperty("fixtureVersion").GetString());
        Assert.False(root.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", root.GetProperty("approvalAuthority").GetString());
        Assert.Equal(16, root.GetProperty("targets").GetArrayLength());
        Assert.Equal(
            [
                "chrome-375", "chrome-768", "chrome-1280", "chrome-1920",
                "edge-375", "edge-768", "edge-1280", "edge-1920",
                "firefox-375", "firefox-768", "firefox-1280", "firefox-1920",
                "webkit-375", "webkit-768", "webkit-1280", "webkit-1920"
            ],
            root.GetProperty("targets").EnumerateArray()
                .Select(target =>
                    $"{target.GetProperty("browser").GetString()}-{target.GetProperty("viewport").GetInt32()}")
                .ToArray());

        var status = root.GetProperty("status").GetString();
        Assert.Contains(status, new[] { "pending-implementation", "approved" });
        if (string.Equals(status, "pending-implementation", StringComparison.Ordinal))
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("approvedBy").ValueKind);
            Assert.Equal(JsonValueKind.Null, root.GetProperty("approvedOn").ValueKind);
            Assert.All(
                root.GetProperty("targets").EnumerateArray(),
                target =>
                {
                    Assert.Equal("PENDING", target.GetProperty("sha256").GetString());
                    Assert.Equal("PENDING", target.GetProperty("artifactSha256").GetString());
                    Assert.False(RepositoryFiles.Exists(
                        $"tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-04/{target.GetProperty("file").GetString()}"));
                });
        }
    }

    [Fact]
    public void Adm04_visual_owner_page_exists_before_baseline_approval_can_begin()
    {
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor"),
            "ADM-04-VIS-T206: SPEC-008/T082 must deliver the page before Ahmed can approve baselines.");
    }
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class StudentAdministrationPageVisualTests(VisualRegressionFixture fixture)
{
    private const string ManifestPath =
        "tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-04/baseline-targets.json";

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
    public async Task Adm04_matches_the_approved_cross_browser_success_baseline(
        string browserName,
        int width)
    {
        await using var context = await fixture.OpenContextAsync(browserName, width);
        var page = await LoadSelectedStudentAsync(context);
        await page.EvaluateAsync("() => document.fonts.ready");

        var actual = await StableVisualCapture.CaptureAsync(
            page, "ADM-04", browserName, width);
        var fileName = $"{browserName}-{width}-success.png";
        var baselinePath = RepositoryFiles.PathTo(
            $"tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-04/{fileName}");
        if (string.Equals(
                Environment.GetEnvironmentVariable("SPEC008_ADM04_BASELINE_APPROVER"),
                "Ahmed ELbamby",
                StringComparison.Ordinal))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllBytesAsync(baselinePath, actual);
            return;
        }

        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        Assert.Equal("approved", manifest.RootElement.GetProperty("status").GetString());
        Assert.Equal("Ahmed ELbamby", manifest.RootElement.GetProperty("approvedBy").GetString());
        Assert.True(
            File.Exists(baselinePath),
            $"ADM-04-VIS-T206 baseline is pending Ahmed's approval: {fileName}");
        var target = Assert.Single(
            manifest.RootElement.GetProperty("targets").EnumerateArray(),
            item => item.GetProperty("file").GetString() == fileName);
        var expectedHash = target.GetProperty("sha256").GetString();
        Assert.NotEqual("PENDING", expectedHash);
        Assert.Equal(
            expectedHash,
            Convert.ToHexString(SHA256.HashData(actual)).ToLowerInvariant());
    }

    private static async Task<IPage> LoadSelectedStudentAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.RouteAsync("**/api/context", route => JsonAsync(route, AppContext));
        await page.RouteAsync("**/api/admin/students?*", route => JsonAsync(route, SearchResults));
        await page.RouteAsync(
            "**/api/admin/students/student-001/academic-context?*",
            route => JsonAsync(route, AcademicDetail));
        await page.GotoAsync("/admin/students", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Student administration", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-testid='student-search-input']").FillAsync("20260001");
        await page.GetByRole(AriaRole.Button, new() { Name = "Search Students", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Open academic record", Exact = true })
            .ClickAsync();
        await page.Locator("[data-testid='ADM-04-COMP-STATE-SUCCESS']").WaitForAsync();
        return page;
    }

    private static Task JsonAsync(IRoute route, string body) => route.FulfillAsync(
        new RouteFulfillOptions
        {
            Status = 200,
            ContentType = "application/json",
            Body = body
        });

    private const string AppContext = """
        {"serverTimeUtc":"2026-07-14T10:15:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,"registrationTerm":{"id":"term-2026-fall","code":"2026-FALL","label":"Fall 2026","state":"registrationOpen","rowVersion":"term-rv-4"},"registrationWindowState":"none","registrationWindow":null,"serviceState":"available","displayName":"Ahmed Admin","authorizedRoles":["Admin"],"activeRole":"Admin","sessionState":"active","expiresAtUtc":"2026-07-14T18:00:00Z","supportReferencePath":"/support"}
        """;

    private const string SearchResults = """
        {"items":[{"studentId":"student-001","universityId":"20260001","programCode":"AI","cohort":"2026","standing":"Good standing","dataVersion":"profile-v7"}],"page":1,"pageSize":20,"totalCount":1,"sort":"universityId,studentId"}
        """;

    private const string AcademicDetail = """
        {
          "studentId":"student-001","termId":"term-2026-fall","universityId":"20260001","programCode":"AI","cohort":"2026","currentGpa":3.42,"earnedCredits":84,"standing":"Good standing",
          "transcriptSummary":{"attemptedCredits":87,"earnedCredits":84,"attemptCount":29},
          "transcriptAttempts":{"items":[{"attemptId":"attempt-401","supersedesAttemptId":null,"courseCode":"AIC401","termCode":"2026-Spring","credits":3,"grade":"A","status":"passed","provenance":"Synthetic SIS"}],"page":1,"pageSize":20,"totalCount":1,"sort":"termCode,courseCode,attemptId"},
          "activeHolds":[{"termId":"term-2026-fall","code":"ADVISING","message":"Meet the academic adviser.","blocksRegistration":false,"effectiveFromUtc":"2026-07-01T00:00:00Z","effectiveToUtc":null,"source":"Synthetic SIS","holdId":"hold-001","sourceReference":"SIS-H-001"}],
          "dataVersion":"profile-v7","dataAsOfUtc":"2026-07-14T09:45:00Z",
          "provenance":{"items":[{"source":"Synthetic SIS","reference":"SIS-20260001","importedAtUtc":"2026-07-14T09:45:00Z"}],"page":1,"pageSize":20,"totalCount":1,"sort":"importedAtUtc,reference"},
          "studentRowVersion":"student-rv-7","studentTermStateRowVersion":"term-state-rv-4"
        }
        """;
}
