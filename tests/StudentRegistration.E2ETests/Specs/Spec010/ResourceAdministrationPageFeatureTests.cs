using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec010;

public sealed class ResourceAdministrationPageFrozenContractTests
{
    [Fact]
    public void Adm_07_owner_contract_page_and_no_override_boundary_are_present()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-07.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/resources\"",
            "\"pageName\": \"ResourceAdministrationPage.razor\"",
            "ADM-07-imported-v1",
            "ADM-07-empty-v1",
            "ADM-07-unavailable-v1",
            "ADM-07-overlap-v1",
            "ADM-07-stale-v1",
            "ADM-07-keyboard-text-entry-v1",
            "no Admin override");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/ResourceAdministrationPage.razor"),
            "SPEC-010/T101 must deliver the ADM-07 owner page.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class ResourceAdministrationPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string RoomId = "00000000-0000-0000-0000-000000001101";
    private const string AvailabilityId = "00000000-0000-0000-0000-000000001102";
    private const string AlertId = "00000000-0000-0000-0000-000000001103";

    [Fact]
    public async Task Adm_07_imported_availability_is_read_only_and_has_no_mutation_request()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var availabilityMutations = 0;
        await page.RouteAsync("**/api/**", async route =>
        {
            if (Path(route).Contains("staff-availability", StringComparison.Ordinal)
                && route.Request.Method != "GET")
            {
                availabilityMutations++;
            }

            if (!await TryInitialDataAsync(route))
            {
                await route.AbortAsync();
            }
        });

        await OpenAsync(page);

        var region = page.Locator("[data-testid='read-only-staff-availability']");
        var declarations = page.Locator("[data-testid='staff-availability-declarations']");
        Assert.True(await region.GetByText("Read-only Staff declaration", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await declarations.GetByText("staff-rv-1", new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(0, await declarations.Locator("input, textarea, select").CountAsync());
        Assert.Equal(0, await page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions
            {
                NameRegex = new Regex("edit|override", RegexOptions.IgnoreCase)
            }).CountAsync());
        Assert.Equal(0, availabilityMutations);
    }

    [Fact]
    public async Task Adm_07_keyboard_room_edit_posts_expected_version_and_xsrf()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? body = null;
        string? xsrf = null;
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (route.Request.Method == "PUT" && Path(route) == $"/api/admin/rooms/{RoomId}")
            {
                body = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out xsrf);
                await JsonAsync(route, 200, RoomJson(capacity: 40, version: "room-rv-2"));
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.Locator("#resource-room-capacity").FillAsync("40");
        await page.Locator("#resource-room-reason").FillAsync("Increase room capacity");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Save room", Exact = true })
            .ClickAsync();

        await page.GetByText("room-rv-2", new() { Exact = true }).WaitForAsync();
        Assert.Equal("adm07-xsrf", xsrf);
        Assert.NotNull(body);
        using var payload = JsonDocument.Parse(body);
        Assert.Equal("room-rv-1", payload.RootElement.GetProperty("expectedRowVersion").GetString());
        Assert.Equal(40, payload.RootElement.GetProperty("capacity").GetInt32());
    }

    [Fact]
    public async Task Adm_07_alert_revalidate_then_resolve_uses_current_alert_versions()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? revalidateBody = null;
        string? resolveBody = null;
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            var path = Path(route);
            if (path.EndsWith("/revalidate", StringComparison.Ordinal))
            {
                revalidateBody = route.Request.PostData;
                await JsonAsync(route, 200, AlertJson("Revalidated", "alert-rv-2", valid: true));
                return;
            }

            if (path.EndsWith("/resolve", StringComparison.Ordinal))
            {
                resolveBody = route.Request.PostData;
                await JsonAsync(route, 200, AlertJson("Resolved", "alert-rv-3", valid: true));
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Revalidate impact", Exact = true })
            .ClickAsync();
        await page.GetByText("Revalidated", new() { Exact = true }).WaitForAsync();
        await page.Locator("#resource-alert-resolution-reason").FillAsync(
            "The corrected room now satisfies the schedule.");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Resolve impact", Exact = true })
            .ClickAsync();

        await page.GetByText("Resolved", new() { Exact = true }).WaitForAsync();
        Assert.NotNull(revalidateBody);
        using var revalidate = JsonDocument.Parse(revalidateBody);
        Assert.Equal("alert-rv-1", revalidate.RootElement
            .GetProperty("expectedAlertRowVersion").GetString());
        Assert.NotNull(resolveBody);
        using var resolve = JsonDocument.Parse(resolveBody);
        Assert.Equal("alert-rv-2", resolve.RootElement
            .GetProperty("expectedAlertRowVersion").GetString());
    }

    [Fact]
    public async Task Adm_07_stale_room_edit_requires_refresh_and_does_not_claim_success()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (route.Request.Method == "PUT")
            {
                await JsonAsync(route, 409, """
                    {
                      "code": "STALE_VERSION",
                      "message": "The room changed after it was loaded.",
                      "correlationId": "ADM-07-STALE",
                      "currentVersion": "room-rv-2"
                    }
                    """);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.Locator("#resource-room-capacity").FillAsync("40");
        await page.Locator("#resource-room-reason").FillAsync("Increase room capacity");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Save room", Exact = true })
            .ClickAsync();

        await page.GetByText("STALE_VERSION", new() { Exact = true }).WaitForAsync();
        Assert.True(await page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Refresh and review", Exact = true })
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Room saved by the server", new() { Exact = true })
            .CountAsync());
    }

    private async Task<IBrowserContext> ContextWithXsrfAsync()
    {
        var context = await fixture.OpenContextAsync();
        await context.AddCookiesAsync(
        [
            new Cookie
            {
                Name = "XSRF-TOKEN",
                Value = "adm07-xsrf",
                Url = fixture.BaseAddress.AbsoluteUri
            }
        ]);
        return context;
    }

    private static async Task OpenAsync(IPage page)
    {
        await page.GotoAsync(
            "/admin/resources?termId=00000000-0000-0000-0000-000000001121",
            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Resource administration", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-route-id='ADM-07'][data-state='success']").WaitForAsync();
    }

    private static async Task<bool> TryInitialDataAsync(IRoute route)
    {
        if (route.Request.Method != "GET")
        {
            return false;
        }

        switch (Path(route))
        {
            case "/api/admin/rooms":
                await JsonAsync(route, 200, RoomPageJson());
                return true;
            case "/api/admin/staff-availability":
                await JsonAsync(route, 200, AvailabilityPageJson());
                return true;
            case "/api/admin/schedule-impact-alerts":
                await JsonAsync(route, 200, AlertPageJson());
                return true;
            default:
                return false;
        }
    }

    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new()
        {
            Status = status,
            ContentType = "application/json",
            Body = body
        });

    private static string RoomPageJson() => $$"""
        {
          "items": [{{RoomJson()}}],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "code,id"
        }
        """;

    private static string RoomJson(int capacity = 35, string version = "room-rv-1") => $$"""
        {
          "id": "{{RoomId}}",
          "code": "A-101",
          "location": "Smart Village",
          "capacity": {{capacity}},
          "state": "Available",
          "rowVersion": "{{version}}"
        }
        """;

    private static string AvailabilityPageJson() => $$"""
        {
          "items": [{
            "id": "{{AvailabilityId}}",
            "staffId": "00000000-0000-0000-0000-000000001120",
            "staffName": "Dr. Nadia",
            "termId": "00000000-0000-0000-0000-000000001121",
            "deadlineUtc": "2026-08-01T00:00:00Z",
            "rowVersion": "staff-rv-1",
            "ranges": [{
              "dayOfWeek": 0,
              "startLocal": "08:00:00",
              "endLocal": "16:00:00",
              "kind": "Available"
            }]
          }],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "staffName,id"
        }
        """;

    private static string AlertPageJson() => $$"""
        {
          "items": [{{AlertJson("Open", "alert-rv-1", valid: false)}}],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "detectedAtUtc-desc,id"
        }
        """;

    private static string AlertJson(string state, string version, bool valid) => $$"""
        {
          "id": "{{AlertId}}",
          "groupId": "00000000-0000-0000-0000-000000001130",
          "staffTermAvailabilityId": "{{AvailabilityId}}",
          "roomId": "{{RoomId}}",
          "reasonCode": "STAFF_UNAVAILABLE",
          "state": "{{state}}",
          "detectedGroupRowVersion": "group-rv-1",
          "detectedRoomRowVersion": "room-rv-1",
          "detectedStaffTermAvailabilityRowVersion": "staff-rv-1",
          "lastValidation": {{(valid ? """
            {
              "valid": true,
              "reasons": [],
              "groupRowVersion": "group-rv-1",
              "roomRowVersions": { "00000000-0000-0000-0000-000000001101": "room-rv-1" },
              "staffTermAvailabilityRowVersions": { "00000000-0000-0000-0000-000000001102": "staff-rv-1" },
              "validatedAtUtc": "2026-07-16T10:00:00Z"
            }
            """ : "null")}},
          "detectedAtUtc": "2026-07-16T09:00:00Z",
          "revalidatedAtUtc": {{(valid ? "\"2026-07-16T10:00:00Z\"" : "null")}},
          "resolvedAtUtc": {{(state == "Resolved" ? "\"2026-07-16T10:30:00Z\"" : "null")}},
          "rowVersion": "{{version}}"
        }
        """;
}
