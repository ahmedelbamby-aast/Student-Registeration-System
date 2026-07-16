using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec010;

public sealed class OfferingAdministrationPageFrozenContractTests
{
    [Fact]
    public void Adm_06_owner_contract_and_page_are_present()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-06.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/offerings\"",
            "\"pageName\": \"OfferingAdministrationPage.razor\"",
            "ADM-06-missing-resource-v1",
            "ADM-06-overlap-v1",
            "ADM-06-capacity-mismatch-v1",
            "ADM-06-stale-edit-v1",
            "ADM-06-publish-v1");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor"),
            "SPEC-010/T099 must deliver the ADM-06 owner page.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class OfferingAdministrationPageFeatureTests(Spec008BrowserFixture fixture)
{
    private const string OfferingId = "00000000-0000-0000-0000-000000001001";
    private const string GroupId = "00000000-0000-0000-0000-000000001002";

    [Fact]
    public async Task Adm_06_edit_posts_group_version_and_never_publishes()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? updateBody = null;
        var publishCalls = 0;
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            var path = Path(route);
            if (route.Request.Method == "PUT" && path == $"/api/admin/groups/{GroupId}")
            {
                updateBody = route.Request.PostData;
                await JsonAsync(route, 200, GroupJson(capacity: 35, groupVersion: "group-rv-2"));
                return;
            }

            if (path.EndsWith("/publish", StringComparison.Ordinal))
            {
                publishCalls++;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.Locator("#offering-group-capacity").FillAsync("35");
        await page.Locator("#offering-change-reason").FillAsync("Increase planned capacity");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Save group", Exact = true })
            .ClickAsync();

        await page.GetByText("group-rv-2", new() { Exact = true }).WaitForAsync();
        Assert.NotNull(updateBody);
        using var payload = JsonDocument.Parse(updateBody);
        Assert.Equal("offering-rv-1", payload.RootElement
            .GetProperty("expectedOfferingRowVersion").GetString());
        Assert.Equal("group-rv-1", payload.RootElement
            .GetProperty("expectedGroupRowVersion").GetString());
        Assert.Equal(35, payload.RootElement.GetProperty("capacity").GetInt32());
        Assert.Equal(0, publishCalls);
    }

    [Fact]
    public async Task Adm_06_validate_lists_all_blockers_and_disables_publish()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (Path(route).EndsWith("/validate", StringComparison.Ordinal))
            {
                await JsonAsync(route, 200, InvalidValidationJson());
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Validate offering", Exact = true })
            .ClickAsync();

        await page.GetByText("MISSING_TEACHING_ASSISTANT", new() { Exact = true })
            .WaitForAsync();
        Assert.True(await page.GetByText("ROOM_CONFLICT", new() { Exact = true })
            .IsVisibleAsync());
        Assert.True(await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Publish offering", Exact = true })
            .IsDisabledAsync());
    }

    [Fact]
    public async Task Adm_06_publish_uses_preview_dependency_versions_and_xsrf_once()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? publishBody = null;
        string? xsrf = null;
        var calls = 0;
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            var path = Path(route);
            if (path.EndsWith("/validate", StringComparison.Ordinal))
            {
                await JsonAsync(route, 200, ValidValidationJson());
                return;
            }

            if (path.EndsWith("/publish", StringComparison.Ordinal))
            {
                calls++;
                publishBody = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out xsrf);
                await JsonAsync(route, 200, OfferingJson(state: "Published"));
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Validate offering", Exact = true })
            .ClickAsync();
        await page.GetByText("Offering validation passed", new() { Exact = true })
            .WaitForAsync();
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Publish offering", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Confirm publish", Exact = true })
            .ClickAsync();

        await page.GetByText("Published by the server", new() { Exact = true })
            .WaitForAsync();
        Assert.Equal(1, calls);
        Assert.Equal("adm06-xsrf", xsrf);
        Assert.NotNull(publishBody);
        using var payload = JsonDocument.Parse(publishBody);
        Assert.Equal("preview-010", payload.RootElement.GetProperty("previewToken").GetString());
        Assert.Equal("group-rv-1", payload.RootElement
            .GetProperty("expectedGroupRowVersions").GetProperty(GroupId).GetString());
        Assert.NotEqual(Guid.Empty, payload.RootElement.GetProperty("clientRequestId").GetGuid());
    }

    [Fact]
    public async Task Adm_06_stale_publish_requires_refresh_without_false_success()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (Path(route).EndsWith("/validate", StringComparison.Ordinal))
            {
                await JsonAsync(route, 200, ValidValidationJson());
                return;
            }

            if (Path(route).EndsWith("/publish", StringComparison.Ordinal))
            {
                await JsonAsync(route, 409, """
                    {
                      "code": "STALE_PREVIEW",
                      "message": "A dependency changed after validation.",
                      "correlationId": "ADM-06-STALE",
                      "currentVersion": "offering-rv-2"
                    }
                    """);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Validate offering", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Publish offering", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Confirm publish", Exact = true })
            .ClickAsync();

        await page.GetByText("STALE_PREVIEW", new() { Exact = true }).WaitForAsync();
        Assert.True(await page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Refresh and review", Exact = true })
            .IsVisibleAsync());
        Assert.Equal(0, await page.GetByText("Published by the server", new() { Exact = true })
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
                Value = "adm06-xsrf",
                Url = fixture.BaseAddress.AbsoluteUri
            }
        ]);
        return context;
    }

    private static async Task OpenAsync(IPage page)
    {
        await page.GotoAsync("/admin/offerings", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Offering administration", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-route-id='ADM-06'][data-state='success']").WaitForAsync();
    }

    private static async Task<bool> TryInitialDataAsync(IRoute route)
    {
        if (route.Request.Method != "GET")
        {
            return false;
        }

        if (Path(route) == "/api/admin/offerings")
        {
            await JsonAsync(route, 200, OfferingPageJson());
            return true;
        }

        if (Path(route) == $"/api/offerings/{OfferingId}")
        {
            await JsonAsync(route, 200, OfferingJson());
            return true;
        }

        return false;
    }

    private static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new()
        {
            Status = status,
            ContentType = "application/json",
            Body = body
        });

    private static string OfferingPageJson() => $$"""
        {
          "items": [{
            "id": "{{OfferingId}}",
            "termId": "00000000-0000-0000-0000-000000001010",
            "courseId": "00000000-0000-0000-0000-000000001011",
            "courseCode": "DS413",
            "courseTitle": "Project I",
            "state": "Draft",
            "groupCount": 1,
            "rowVersion": "offering-rv-1"
          }],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "courseCode,id"
        }
        """;

    private static string OfferingJson(
        int capacity = 30,
        string state = "Draft",
        string groupVersion = "group-rv-1") => $$"""
        {
          "id": "{{OfferingId}}",
          "termId": "00000000-0000-0000-0000-000000001010",
          "courseId": "00000000-0000-0000-0000-000000001011",
          "courseCode": "DS413",
          "courseTitle": "Project I",
          "state": "{{state}}",
          "groups": [{
            "id": "{{GroupId}}",
            "offeringId": "{{OfferingId}}",
            "groupCode": "A",
            "capacity": {{capacity}},
            "enrolledCount": 12,
            "registrationPaused": false,
            "state": "{{state}}",
            "selectable": false,
            "nonSelectableReasons": [],
            "staff": [
              {
                "meetingSlotId": "00000000-0000-0000-0000-000000001020",
                "activityType": "Lecture",
                "staffId": "00000000-0000-0000-0000-000000001030",
                "role": "Lecturer",
                "name": "Dr. Nadia"
              },
              {
                "meetingSlotId": "00000000-0000-0000-0000-000000001021",
                "activityType": "Tutorial",
                "staffId": "00000000-0000-0000-0000-000000001031",
                "role": "TeachingAssistant",
                "name": "Eng. Omar"
              }
            ],
            "meetings": [
              {
                "id": "00000000-0000-0000-0000-000000001020",
                "activityType": "Lecture",
                "dayOfWeek": 0,
                "startLocal": "09:00:00",
                "endLocal": "10:30:00",
                "roomId": "00000000-0000-0000-0000-000000001040",
                "roomCode": "A-101",
                "location": "Smart Village"
              },
              {
                "id": "00000000-0000-0000-0000-000000001021",
                "activityType": "Tutorial",
                "dayOfWeek": 2,
                "startLocal": "11:00:00",
                "endLocal": "12:30:00",
                "roomId": "00000000-0000-0000-0000-000000001041",
                "roomCode": "LAB-2",
                "location": "Smart Village"
              }
            ],
            "rowVersion": "{{groupVersion}}"
          }],
          "rowVersion": "offering-rv-1"
        }
        """;

    private static string GroupJson(
        int capacity = 30,
        string state = "Draft",
        string groupVersion = "group-rv-1")
    {
        using var document = JsonDocument.Parse(
            OfferingJson(capacity, state, groupVersion));
        return document.RootElement.GetProperty("groups")[0].GetRawText();
    }

    private static string InvalidValidationJson() => $$"""
        {
          "valid": false,
          "reasons": [
            {
              "code": "MISSING_TEACHING_ASSISTANT",
              "message": "Tutorial requires a Teaching Assistant.",
              "resourceIds": ["{{GroupId}}"]
            },
            {
              "code": "ROOM_CONFLICT",
              "message": "A-101 overlaps group B.",
              "resourceIds": ["00000000-0000-0000-0000-000000001040"],
              "overlapStartLocal": "09:30:00",
              "overlapEndLocal": "10:00:00"
            }
          ],
          "previewToken": null,
          "dependencyVersions": {
            "offering": "offering-rv-1",
            "groups": { "{{GroupId}}": "group-rv-1" },
            "rooms": {},
            "staffTermAvailability": {}
          }
        }
        """;

    private static string ValidValidationJson() => $$"""
        {
          "valid": true,
          "reasons": [],
          "previewToken": "preview-010",
          "dependencyVersions": {
            "offering": "offering-rv-1",
            "groups": { "{{GroupId}}": "group-rv-1" },
            "rooms": {
              "00000000-0000-0000-0000-000000001040": "room-rv-1",
              "00000000-0000-0000-0000-000000001041": "room-rv-2"
            },
            "staffTermAvailability": {
              "00000000-0000-0000-0000-000000001050": "staff-rv-1"
            }
          }
        }
        """;
}
