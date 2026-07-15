using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec008;

public sealed class TermAdministrationPageFrozenContractTests
{
    [Fact]
    public void Adm_02_owner_metadata_and_all_five_journeys_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-02.md");
        var fixture = RepositoryFiles.Read(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec008/ADM-02/route-contract.json");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/terms\"",
            "\"pageName\": \"TermAdministrationPage.razor\"",
            "GET /api/admin/terms",
            "POST /api/admin/terms",
            "PUT /api/admin/terms/{termId}",
            "POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish",
            "ADM-02-E2E-PRIMARY",
            "ADM-02-E2E-FAILURE");
        RepositoryFiles.ContainsAll(
            fixture,
            "ADM-02-create-v1",
            "ADM-02-invalid-dates-v1",
            "ADM-02-overlap-v1",
            "ADM-02-concurrent-edit-v1",
            "ADM-02-publish-v1");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor"),
            "ADM-02-E2E-PRIMARY: SPEC-008/T080 must deliver TermAdministrationPage.razor.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class TermAdministrationPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string EmptyPage = """
        {
          "items": [],
          "page": 1,
          "pageSize": 20,
          "totalCount": 0,
          "sort": "code,id"
        }
        """;

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

    private const string CreatedTerm = """
        {
          "id": "term-fall-2026",
          "code": "2026-FALL",
          "displayName": "Fall 2026",
          "timeZoneId": "Africa/Cairo",
          "teachingStartsOn": "2026-09-13",
          "teachingEndsOn": "2027-01-14",
          "state": "draft",
          "rowVersion": "term-rv-created",
          "windows": [
            {
              "id": "window-all",
              "scopeType": "all-students",
              "scopeValue": null,
              "opensAtUtc": "2026-08-20T06:00:00Z",
              "closesAtUtc": "2026-08-27T18:00:00Z",
              "lifecycleState": "draft",
              "computedState": "upcoming",
              "rowVersion": "window-rv-created"
            }
          ]
        }
        """;

    private const string PublishedTerm = """
        {
          "id": "term-fall-2026",
          "code": "2026-FALL",
          "displayName": "Fall 2026",
          "timeZoneId": "Africa/Cairo",
          "teachingStartsOn": "2026-09-13",
          "teachingEndsOn": "2027-01-14",
          "state": "registrationOpen",
          "rowVersion": "term-rv-published",
          "windows": [
            {
              "id": "window-all",
              "scopeType": "all-students",
              "scopeValue": null,
              "opensAtUtc": "2026-08-20T06:00:00Z",
              "closesAtUtc": "2026-08-27T18:00:00Z",
              "lifecycleState": "published",
              "computedState": "upcoming",
              "rowVersion": "window-rv-published"
            }
          ]
        }
        """;

    private const string RefetchedStalePage = """
        {
          "items": [
            {
              "id": "term-fall-2026",
              "code": "2026-FALL",
              "displayName": "Fall 2026 updated by another administrator",
              "timeZoneId": "Africa/Cairo",
              "teachingStartsOn": "2026-09-20",
              "teachingEndsOn": "2027-01-14",
              "state": "draft",
              "rowVersion": "term-rv-2",
              "windows": [
                {
                  "id": "window-all",
                  "scopeType": "all-students",
                  "scopeValue": null,
                  "opensAtUtc": "2026-08-20T06:00:00Z",
                  "closesAtUtc": "2026-08-27T18:00:00Z",
                  "lifecycleState": "draft",
                  "computedState": "upcoming",
                  "rowVersion": "window-rv-2"
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

    [Fact]
    public async Task Adm_02_create_v1_posts_reason_source_client_id_window_and_XSRF_then_waits_for_server_success()
    {
        EnsureOwnerPageDelivered();
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? capturedBody = null;
        string? capturedXsrf = null;
        var createCalls = 0;
        await page.RouteAsync("**/api/admin/terms**", async route =>
        {
            var path = new Uri(route.Request.Url).AbsolutePath;
            if (route.Request.Method == "GET" && path == "/api/admin/terms")
            {
                await JsonAsync(route, 200, EmptyPage);
                return;
            }

            if (route.Request.Method == "POST" && path == "/api/admin/terms")
            {
                createCalls++;
                capturedBody = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out capturedXsrf);
                await JsonAsync(route, 201, CreatedTerm);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page, expectedState: "empty");
        await FillNewTermAsync(page, "2026-FALL", "Fall 2026", "2026-09-13", "2027-01-14");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Add registration window", Exact = true })
            .ClickAsync();
        await page.Locator("#window-opens-at").FillAsync("2026-08-20T08:00");
        await page.Locator("#window-closes-at").FillAsync("2026-08-27T20:00");
        await FillReasonAndSourceAsync(page, "Create the Fall registration term", "ADM-02 demo");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Create term", Exact = true })
            .ClickAsync();

        await page.GetByText("term-rv-created", new PageGetByTextOptions { Exact = true })
            .WaitForAsync();
        Assert.Equal(1, createCalls);
        Assert.Equal("adm02-xsrf", capturedXsrf);
        Assert.NotNull(capturedBody);
        using var body = JsonDocument.Parse(capturedBody);
        Assert.NotEqual(Guid.Empty, body.RootElement.GetProperty("clientRequestId").GetGuid());
        Assert.Equal(
            "Create the Fall registration term",
            body.RootElement.GetProperty("reason").GetString());
        Assert.Equal("ADM-02 demo", body.RootElement.GetProperty("source").GetString());
        Assert.Equal("2026-FALL", body.RootElement.GetProperty("term").GetProperty("code").GetString());
        Assert.Single(body.RootElement.GetProperty("windows").EnumerateArray());
    }

    [Fact]
    public async Task Adm_02_invalid_dates_v1_links_fields_focuses_summary_and_sends_no_command()
    {
        EnsureOwnerPageDelivered();
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var mutationCalls = 0;
        await page.RouteAsync("**/api/admin/terms**", route =>
        {
            if (route.Request.Method != "GET")
            {
                mutationCalls++;
            }

            return JsonAsync(route, 200, EmptyPage);
        });

        await OpenAsync(page, expectedState: "empty");
        await FillNewTermAsync(page, "2026-FALL", "Fall 2026", "2027-01-14", "2026-09-13");
        await FillReasonAndSourceAsync(page, "Create the Fall registration term", "ADM-02 demo");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Create term", Exact = true })
            .ClickAsync();

        var summary = page.Locator("[data-testid='term-validation-summary']");
        await summary.WaitForAsync();
        Assert.Equal(
            "term-validation-summary",
            await page.EvaluateAsync<string>("() => document.activeElement?.dataset?.testid || ''"));
        Assert.Contains(
            "term-teaching-end-error",
            await page.Locator("#term-teaching-start").GetAttributeAsync("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.Contains(
            "term-teaching-end-error",
            await page.Locator("#term-teaching-end").GetAttributeAsync("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.Equal(0, mutationCalls);
    }

    [Fact]
    public async Task Adm_02_overlap_v1_shows_every_conflicting_window_and_never_reports_publish_success()
    {
        EnsureOwnerPageDelivered();
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var publishCalls = 0;
        await page.RouteAsync("**/api/admin/terms**", async route =>
        {
            var path = new Uri(route.Request.Url).AbsolutePath;
            if (route.Request.Method == "GET")
            {
                await JsonAsync(route, 200, DraftPage);
                return;
            }

            if (route.Request.Method == "POST" && path.EndsWith("/publish", StringComparison.Ordinal))
            {
                publishCalls++;
                await JsonAsync(route, 409, """
                    {
                      "code": "WINDOW_OVERLAP",
                      "message": "The registration window overlaps existing windows.",
                      "correlationId": "ADM-02-OVERLAP-REF",
                      "fieldErrors": {
                        "windows": [
                          "Window A: 20 Aug 2026 08:00–27 Aug 2026 20:00 Africa/Cairo",
                          "Window B: 25 Aug 2026 08:00–30 Aug 2026 20:00 Africa/Cairo"
                        ]
                      }
                    }
                    """);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page, expectedState: "success");
        await SelectFallTermAsync(page);
        await FillReasonAndSourceAsync(page, "Publish the Fall registration window", "ADM-02 demo");
        await PublishAndConfirmAsync(page);

        await page.GetByText("WINDOW_OVERLAP", new PageGetByTextOptions { Exact = true })
            .WaitForAsync();
        Assert.True(await page.GetByText(
            "Window A: 20 Aug 2026 08:00–27 Aug 2026 20:00 Africa/Cairo",
            new PageGetByTextOptions { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText(
            "Window B: 25 Aug 2026 08:00–30 Aug 2026 20:00 Africa/Cairo",
            new PageGetByTextOptions { Exact = true }).IsVisibleAsync());
        Assert.Equal(1, publishCalls);
        Assert.Equal(0, await page.GetByText("Published successfully", new() { Exact = true }).CountAsync());
    }

    [Fact]
    public async Task Adm_02_concurrent_edit_v1_refetches_changed_fields_and_preserves_the_safe_draft_without_overwrite()
    {
        EnsureOwnerPageDelivered();
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var getCalls = 0;
        var updateCalls = 0;
        await page.RouteAsync("**/api/admin/terms**", async route =>
        {
            var path = new Uri(route.Request.Url).AbsolutePath;
            if (route.Request.Method == "GET")
            {
                getCalls++;
                await JsonAsync(route, 200, getCalls == 1 ? DraftPage : RefetchedStalePage);
                return;
            }

            if (route.Request.Method == "PUT" && path == "/api/admin/terms/term-fall-2026")
            {
                updateCalls++;
                await JsonAsync(route, 409, """
                    {
                      "code": "STALE_VERSION",
                      "message": "The term changed after it was loaded.",
                      "correlationId": "ADM-02-STALE-REF",
                      "currentVersion": "term-rv-2"
                    }
                    """);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page, expectedState: "success");
        await SelectFallTermAsync(page);
        await page.Locator("#term-display-name").FillAsync("My safe Fall draft");
        await FillReasonAndSourceAsync(page, "Adjust the Fall term teaching dates", "ADM-02 demo");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Save changes", Exact = true })
            .ClickAsync();

        await page.GetByText("STALE_VERSION", new PageGetByTextOptions { Exact = true })
            .WaitForAsync();
        await page.Locator("[data-testid='stale-changed-fields']").WaitForAsync();
        Assert.Equal("My safe Fall draft", await page.Locator("#term-display-name").InputValueAsync());
        Assert.True(await page.GetByText(
            "Fall 2026 updated by another administrator",
            new PageGetByTextOptions { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("2026-09-20", new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(2, getCalls);
        Assert.Equal(1, updateCalls);
    }

    [Fact]
    public async Task Adm_02_publish_v1_posts_both_versions_and_XSRF_then_renders_only_server_accepted_publication()
    {
        EnsureOwnerPageDelivered();
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? capturedBody = null;
        string? capturedXsrf = null;
        var publishCalls = 0;
        await page.RouteAsync("**/api/admin/terms**", async route =>
        {
            var path = new Uri(route.Request.Url).AbsolutePath;
            if (route.Request.Method == "GET")
            {
                await JsonAsync(route, 200, DraftPage);
                return;
            }

            if (route.Request.Method == "POST" && path ==
                "/api/admin/terms/term-fall-2026/registration-windows/window-all/publish")
            {
                publishCalls++;
                capturedBody = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out capturedXsrf);
                await JsonAsync(route, 200, PublishedTerm);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page, expectedState: "success");
        await SelectFallTermAsync(page);
        await FillReasonAndSourceAsync(page, "Publish the Fall registration window", "ADM-02 demo");
        await PublishAndConfirmAsync(page);

        await page.GetByText("window-rv-published", new PageGetByTextOptions { Exact = true })
            .WaitForAsync();
        Assert.True(await page.GetByText("published", new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(1, publishCalls);
        Assert.Equal("adm02-xsrf", capturedXsrf);
        Assert.NotNull(capturedBody);
        using var body = JsonDocument.Parse(capturedBody);
        Assert.Equal("term-rv-1", body.RootElement.GetProperty("expectedTermRowVersion").GetString());
        Assert.Equal("window-rv-1", body.RootElement.GetProperty("expectedWindowRowVersion").GetString());
        Assert.Equal(
            "Publish the Fall registration window",
            body.RootElement.GetProperty("reason").GetString());
    }

    private async Task<IBrowserContext> ContextWithXsrfAsync()
    {
        var context = await fixture.OpenContextAsync();
        await context.AddCookiesAsync(
        [
            new Cookie
            {
                Name = "XSRF-TOKEN",
                Value = "adm02-xsrf",
                Url = fixture.BaseAddress.AbsoluteUri
            }
        ]);
        return context;
    }

    private static void EnsureOwnerPageDelivered() =>
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor"),
            "TermAdministrationPage.razor is intentionally absent until SPEC-008/T080; the ADM-02 journey remains red.");

    private static async Task OpenAsync(IPage page, string expectedState)
    {
        await page.GotoAsync("/admin/terms", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Term administration", Exact = true })
            .WaitForAsync();
        await page.Locator($"[data-route-id='ADM-02'][data-state='{expectedState}']")
            .WaitForAsync();
    }

    private static async Task FillNewTermAsync(
        IPage page,
        string code,
        string displayName,
        string teachingStartsOn,
        string teachingEndsOn)
    {
        await page.Locator("#term-code").FillAsync(code);
        await page.Locator("#term-display-name").FillAsync(displayName);
        await page.Locator("#term-time-zone").FillAsync("Africa/Cairo");
        await page.Locator("#term-teaching-start").FillAsync(teachingStartsOn);
        await page.Locator("#term-teaching-end").FillAsync(teachingEndsOn);
    }

    private static async Task FillReasonAndSourceAsync(IPage page, string reason, string source)
    {
        await page.Locator("#term-change-reason").FillAsync(reason);
        await page.Locator("#term-change-source").FillAsync(source);
    }

    private static async Task SelectFallTermAsync(IPage page)
    {
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Review Fall 2026", Exact = true })
            .ClickAsync();
        await page.Locator("#term-display-name").WaitForAsync();
    }

    private static async Task PublishAndConfirmAsync(IPage page)
    {
        var trigger = page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Publish registration window", Exact = true });
        await trigger.ClickAsync();
        var dialog = page.Locator("[data-testid='publish-registration-window-dialog']");
        await dialog.WaitForAsync();
        await dialog.GetByRole(
                AriaRole.Button,
                new LocatorGetByRoleOptions { Name = "Confirm publish", Exact = true })
            .ClickAsync();
    }

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new RouteFulfillOptions
        {
            Status = status,
            ContentType = "application/json",
            Body = body
        });
}
