using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

public sealed class StudentDashboardPageVisualFrozenContractTests
{
    [Fact]
    public void Stu_01_visual_target_matrix_has_exactly_sixteen_governed_owner_targets()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/STU-01.md");
        using var targets = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/Spec008/STU-01/baseline-targets.json"));
        var root = targets.RootElement;

        Assert.Contains("STU-01-VIS-T151", design, StringComparison.Ordinal);
        Assert.Equal("STU-01", root.GetProperty("routeId").GetString());
        Assert.Equal("STU-01-open-v1", root.GetProperty("journeyFixture").GetString());
        Assert.Equal("frontend-fixture/1.0", root.GetProperty("fixtureVersion").GetString());
        var status = root.GetProperty("status").GetString();
        Assert.Contains(status, new[] { "pending-approval", "approved" });
        Assert.Equal(16, root.GetProperty("targets").GetArrayLength());
        Assert.All(
            root.GetProperty("targets").EnumerateArray(),
            target =>
            {
                Assert.Contains(
                    target.GetProperty("browser").GetString(),
                    new[] { "chrome", "edge", "firefox", "webkit" });
                Assert.Contains(
                    target.GetProperty("viewport").GetInt32(),
                    new[] { 375, 768, 1280, 1920 });
                if (string.Equals(status, "pending-approval", StringComparison.Ordinal))
                {
                    Assert.Equal("PENDING", target.GetProperty("sha256").GetString());
                    Assert.Equal("PENDING", target.GetProperty("artifactSha256").GetString());
                }
                else
                {
                    Assert.NotEqual("PENDING", target.GetProperty("sha256").GetString());
                    Assert.Equal(
                        target.GetProperty("sha256").GetString(),
                        target.GetProperty("artifactSha256").GetString());
                }
            });
        Assert.False(root.GetProperty("automaticReplacementAllowed").GetBoolean());
        Assert.Equal("Ahmed ELbamby", root.GetProperty("approvalAuthority").GetString());
        if (string.Equals(status, "approved", StringComparison.Ordinal))
        {
            Assert.Equal("Ahmed ELbamby", root.GetProperty("approvedBy").GetString());
            Assert.Equal("2026-07-14", root.GetProperty("approvedOn").GetString());
        }
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor"),
            "STU-01-VIS-T151: the baseline owner page is still missing until SPEC-008/T078.");
    }
}

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class StudentDashboardPageVisualTests(VisualRegressionFixture fixture)
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor";

    private const string OpenAppContext = """
        {
          "serverTimeUtc": "2026-07-20T07:15:00Z",
          "timeZoneId": "Africa/Cairo",
          "teachingTerm": {
            "id": "TERM-SUMMER-2026",
            "code": "SUMMER-2026",
            "label": "Summer 2026",
            "state": "teaching",
            "rowVersion": "TEACHING-RV-1"
          },
          "registrationTerm": {
            "id": "TERM-FALL-2026",
            "code": "FALL-2026",
            "label": "Fall 2026",
            "state": "registrationOpen",
            "rowVersion": "TERM-RV-1"
          },
          "registrationWindowState": "open",
          "registrationWindow": {
            "id": "WINDOW-FALL-2026",
            "state": "open",
            "opensAtUtc": "2026-07-19T06:00:00Z",
            "closesAtUtc": "2026-07-21T18:00:00Z",
            "rowVersion": "WINDOW-RV-1"
          },
          "serviceState": "available",
          "displayName": "Synthetic Student One",
          "authorizedRoles": ["Student"],
          "activeRole": "Student",
          "sessionState": "active",
          "expiresAtUtc": "2026-07-20T09:15:00Z",
          "supportReferencePath": "/support/student/STU-01-SAFE-REF"
        }
        """;

    private const string AcademicContext = """
        {
          "universityId": "20260001",
          "programCode": "AI",
          "cohort": "2026",
          "currentGpa": 2.85,
          "earnedCredits": 72,
          "standing": "Good standing",
          "transcriptSummary": {
            "attemptedCredits": 75,
            "earnedCredits": 72,
            "attemptCount": 25
          },
          "transcriptAttempts": {
            "items": [],
            "page": 1,
            "pageSize": 20,
            "totalCount": 0,
            "sort": "termCode,courseCode,attemptId"
          },
          "activeHolds": [],
          "dataVersion": "ACADEMIC-V1",
          "dataAsOfUtc": "2026-07-20T07:14:00Z",
          "provenance": {
            "items": [],
            "page": 1,
            "pageSize": 20,
            "totalCount": 0,
            "sort": "importedAtUtc,reference"
          }
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
    public async Task Stu_01_matches_the_approved_cross_browser_open_state_baseline(
        string browserName,
        int width)
    {
        RequireOwnerPage();
        await using var context = await fixture.OpenContextAsync(browserName, width);
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/context", route => route.FulfillAsync(
            JsonResponse(200, OpenAppContext)));
        await page.RouteAsync("**/api/students/me/academic-context", route => route.FulfillAsync(
            JsonResponse(200, AcademicContext)));
        await page.RouteAsync(
            "**/api/student/registrations/current/timetable",
            route => route.FulfillAsync(JsonResponse(
                503,
                """
                {
                  "code": "CONTEXT_UNAVAILABLE",
                  "message": "The current timetable contract is not available yet.",
                  "correlationId": "STU-01-SAFE-REF"
                }
                """)));
        await page.GotoAsync("/student", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Student dashboard", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-route-id='STU-01'][data-state='success']")
            .WaitForAsync();
        await page.Locator(
                "[data-testid='current-timetable-unavailable'][data-contributor-state='unavailable']")
            .WaitForAsync();
        await page.EvaluateAsync("() => document.fonts.ready");

        var actual = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true
        });
        var fileName = $"{browserName}-{width}-success.png";
        var baselinePath = RepositoryFiles.PathTo(
            $"tests/StudentRegistration.VisualTests/Baselines/Spec008/STU-01/{fileName}");
        if (string.Equals(
                Environment.GetEnvironmentVariable("SPEC008_STU01_BASELINE_APPROVER"),
                "Ahmed ELbamby",
                StringComparison.Ordinal))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            await File.WriteAllBytesAsync(baselinePath, actual);
            return;
        }

        Assert.True(
            File.Exists(baselinePath),
            $"STU-01-VIS-T151 baseline is not approved yet: {fileName}");

        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(
            "tests/StudentRegistration.VisualTests/Baselines/Spec008/STU-01/baseline-targets.json"));
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

    private static RouteFulfillOptions JsonResponse(int status, string body) => new()
    {
        Status = status,
        ContentType = "application/json",
        Body = body
    };

    private static void RequireOwnerPage() =>
        Assert.True(
            RepositoryFiles.Exists(PagePath),
            "StudentDashboardPage.razor is intentionally absent until SPEC-008/T078; the STU-01 visual journey remains red.");
}
