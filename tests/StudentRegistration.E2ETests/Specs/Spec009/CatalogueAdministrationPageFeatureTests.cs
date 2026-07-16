using System.Text.Json;
using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec009;

public sealed class CatalogueAdministrationPageFrozenContractTests
{
    [Fact]
    public void Adm_05_owner_metadata_and_required_journeys_are_frozen()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/ADM-05.md");
        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/admin/catalogue\"",
            "\"pageName\": \"CatalogueAdministrationPage.razor\"",
            "ADM-05-draft-v1",
            "ADM-05-invalid-v1",
            "ADM-05-cycle-v1",
            "ADM-05-simulate-v1",
            "ADM-05-publish-v1",
            "ADM-05-stale-publish-v1",
            "ADM-05-E2E-PRIMARY",
            "ADM-05-E2E-FAILURE");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/CatalogueAdministrationPage.razor"),
            "SPEC-009/T091 must deliver CatalogueAdministrationPage.razor.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class CatalogueAdministrationPageFeatureTests(
    Spec008BrowserFixture fixture)
{
    private const string DraftId = "00000000-0000-0000-0000-000000000901";
    private const string ImportId = "00000000-0000-0000-0000-000000000902";
    private const string VersionId = "00000000-0000-0000-0000-000000000903";
    private const string PolicyId = "00000000-0000-0000-0000-000000000904";

    [Fact]
    public async Task Adm_05_draft_v1_keeps_snapshot_provenance_synthetic_labels_and_versions_together()
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/admin/**", InitialDataAsync);

        await OpenAsync(page);

        Assert.Equal(19, await page.Locator("[data-testid='catalogue-course']").CountAsync());
        Assert.True(await page.GetByText(
            "Official source",
            new PageGetByTextOptions { Exact = true }).First.IsVisibleAsync());
        Assert.True(await page.GetByText(
            "Synthetic fields: Credits, IsActive",
            new PageGetByTextOptions { Exact = true }).First.IsVisibleAsync());
        Assert.True(await page.GetByText(
            "DEMO-CATALOGUE-2026.1",
            new PageGetByTextOptions { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText(
            "draft-rv-1",
            new PageGetByTextOptions { Exact = true }).IsVisibleAsync());
        Assert.False(await page.GetByText(
            "Complete AASTMT curriculum",
            new PageGetByTextOptions { Exact = true }).IsVisibleAsync());
    }

    [Fact]
    public async Task Adm_05_import_posts_source_access_hash_manifest_and_xsrf_once()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? body = null;
        string? xsrf = null;
        var calls = 0;
        await page.RouteAsync("**/api/admin/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (route.Request.Method == "POST"
                && Path(route) == "/api/admin/catalogue/imports")
            {
                calls++;
                body = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out xsrf);
                await JsonAsync(route, 201, ImportJson());
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.Locator("#catalogue-import-source").FillAsync(
            "https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655");
        await page.Locator("#catalogue-import-accessed-on").FillAsync("2026-07-13");
        await page.Locator("#catalogue-import-hash").FillAsync("sha256:IMPORT-009");
        await page.Locator("#catalogue-import-synthetic-fields").FillAsync(
            "Credits, IsActive");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Create import", Exact = true })
            .ClickAsync();

        await page.GetByText("uploaded", new() { Exact = true }).WaitForAsync();
        Assert.Equal(1, calls);
        Assert.Equal("adm05-xsrf", xsrf);
        Assert.NotNull(body);
        using var payload = JsonDocument.Parse(body);
        Assert.Equal(DraftId, payload.RootElement.GetProperty("draftId").GetGuid().ToString());
        Assert.Equal(
            "2026-07-13",
            payload.RootElement.GetProperty("accessedOn").GetString());
        Assert.Equal(
            ["Credits", "IsActive"],
            payload.RootElement.GetProperty("syntheticFields")
                .EnumerateArray()
                .Select(value => value.GetString()));
    }

    [Fact]
    public async Task Adm_05_invalid_v1_focuses_typed_validation_and_sends_no_policy_command()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var mutations = 0;
        await page.RouteAsync("**/api/admin/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            mutations++;
            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.Locator("#normal-max-credits").FillAsync("not-a-number");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Save policy draft", Exact = true })
            .ClickAsync();

        await page.Locator("[data-testid='catalogue-validation-summary']").WaitForAsync();
        Assert.Equal(
            "catalogue-validation-summary",
            await page.EvaluateAsync<string>(
                "() => document.activeElement?.dataset?.testid || ''"));
        Assert.Contains(
            "normal-max-credits-error",
            await page.Locator("#normal-max-credits")
                .GetAttributeAsync("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.Equal(0, mutations);
    }

    [Fact]
    public async Task Adm_05_cycle_v1_lists_the_complete_path_and_direct_edit_links_without_publish()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var publishCalls = 0;
        await page.RouteAsync("**/api/admin/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (route.Request.Method == "POST"
                && Path(route).EndsWith("/validate", StringComparison.Ordinal))
            {
                await JsonAsync(route, 200, """
                    {
                      "valid": false,
                      "errors": [
                        {
                          "row": null,
                          "field": "prerequisites",
                          "code": "PREREQUISITE_CYCLE",
                          "message": "GN121 -> GN211 -> IN221 -> GN121"
                        }
                      ],
                      "previewToken": null,
                      "expectedDraftRowVersion": "draft-rv-1",
                      "dependencyVersions": {
                        "policy": "DEMO-POC-2026.1"
                      }
                    }
                    """);
                return;
            }

            if (Path(route).EndsWith("/publish", StringComparison.Ordinal))
            {
                publishCalls++;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Validate catalogue", Exact = true })
            .ClickAsync();

        await page.GetByText("PREREQUISITE_CYCLE", new() { Exact = true })
            .WaitForAsync();
        Assert.True(await page.GetByText(
            "GN121 -> GN211 -> IN221 -> GN121",
            new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(3, await page.Locator("[data-testid='cycle-edit-link']").CountAsync());
        Assert.True(await page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Publish catalogue", Exact = true })
            .IsDisabledAsync());
        Assert.Equal(0, publishCalls);
    }

    [Fact]
    public async Task Adm_05_simulate_v1_renders_deterministic_explanations_without_persisting()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        var simulationCalls = 0;
        var publishCalls = 0;
        await page.RouteAsync("**/api/admin/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (route.Request.Method == "POST"
                && Path(route).EndsWith("/simulate", StringComparison.Ordinal))
            {
                simulationCalls++;
                await JsonAsync(route, 200, SimulationJson());
                return;
            }

            if (Path(route).EndsWith("/publish", StringComparison.Ordinal))
            {
                publishCalls++;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.Locator("#simulation-fixture").FillAsync("ACTIVE-96-CREDITS");
        await page.Locator("#simulation-courses").FillAsync("DS413");
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Simulate policy", Exact = true })
            .ClickAsync();

        await page.GetByText("Eligible", new() { Exact = true }).WaitForAsync();
        Assert.True(await page.GetByText(
            "DS413_MIN_GPA passed.",
            new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText(
            "Simulation does not publish or save changes.",
            new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(1, simulationCalls);
        Assert.Equal(0, publishCalls);
    }

    [Fact]
    public async Task Adm_05_publish_v1_uses_preview_version_xsrf_and_only_server_accepted_success()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        string? publishBody = null;
        string? xsrf = null;
        await page.RouteAsync("**/api/admin/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (route.Request.Method == "POST"
                && Path(route).EndsWith("/validate", StringComparison.Ordinal))
            {
                await JsonAsync(route, 200, ValidPreviewJson());
                return;
            }

            if (route.Request.Method == "POST"
                && Path(route).EndsWith("/publish", StringComparison.Ordinal))
            {
                publishBody = route.Request.PostData;
                var headers = await route.Request.AllHeadersAsync();
                headers.TryGetValue("x-xsrf-token", out xsrf);
                await JsonAsync(route, 201, PublishedVersionJson());
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Validate catalogue", Exact = true })
            .ClickAsync();
        await page.GetByText("Catalogue validation passed", new() { Exact = true })
            .WaitForAsync();
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Publish catalogue", Exact = true })
            .ClickAsync();
        var dialog = page.Locator("#publish-catalogue-dialog");
        await dialog.WaitForAsync();
        await dialog.GetByRole(
                AriaRole.Button,
                new LocatorGetByRoleOptions { Name = "Confirm publish", Exact = true })
            .ClickAsync();

        await page.GetByText("Published immutable catalogue version", new() { Exact = true })
            .WaitForAsync();
        Assert.Equal("adm05-xsrf", xsrf);
        Assert.NotNull(publishBody);
        using var body = JsonDocument.Parse(publishBody);
        Assert.Equal(
            "draft-rv-1",
            body.RootElement.GetProperty("expectedDraftRowVersion").GetString());
        Assert.Equal(
            "preview-token-009",
            body.RootElement.GetProperty("previewToken").GetString());
        Assert.NotEqual(Guid.Empty, body.RootElement.GetProperty("clientRequestId").GetGuid());
    }

    [Fact]
    public async Task Adm_05_stale_publish_v1_requires_refresh_and_revalidation_without_false_success()
    {
        await using var context = await ContextWithXsrfAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/admin/**", async route =>
        {
            if (await TryInitialDataAsync(route))
            {
                return;
            }

            if (Path(route).EndsWith("/validate", StringComparison.Ordinal))
            {
                await JsonAsync(route, 200, ValidPreviewJson());
                return;
            }

            if (Path(route).EndsWith("/publish", StringComparison.Ordinal))
            {
                await JsonAsync(route, 409, """
                    {
                      "code": "STALE_PREVIEW",
                      "message": "The catalogue changed after validation.",
                      "correlationId": "ADM-05-STALE-REF",
                      "currentVersion": "draft-rv-2"
                    }
                    """);
                return;
            }

            await route.AbortAsync();
        });

        await OpenAsync(page);
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Validate catalogue", Exact = true })
            .ClickAsync();
        await page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions { Name = "Publish catalogue", Exact = true })
            .ClickAsync();
        var dialog = page.Locator("#publish-catalogue-dialog");
        await dialog.WaitForAsync();
        await dialog.GetByRole(
                AriaRole.Button,
                new LocatorGetByRoleOptions { Name = "Confirm publish", Exact = true })
            .ClickAsync();

        await page.GetByText("STALE_PREVIEW", new() { Exact = true }).WaitForAsync();
        Assert.True(await page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Refresh and revalidate", Exact = true })
            .IsVisibleAsync());
        Assert.Equal(
            0,
            await page.GetByText(
                "Published immutable catalogue version",
                new() { Exact = true }).CountAsync());
    }

    private async Task<IBrowserContext> ContextWithXsrfAsync()
    {
        var context = await fixture.OpenContextAsync();
        await context.AddCookiesAsync(
        [
            new Cookie
            {
                Name = "XSRF-TOKEN",
                Value = "adm05-xsrf",
                Url = fixture.BaseAddress.AbsoluteUri
            }
        ]);
        return context;
    }

    private static async Task OpenAsync(IPage page)
    {
        await page.GotoAsync("/admin/catalogue", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions
                {
                    Name = "Catalogue and policy administration",
                    Exact = true
                })
            .WaitForAsync();
        await page.Locator("[data-route-id='ADM-05'][data-state='success']")
            .WaitForAsync();
    }

    private static async Task InitialDataAsync(IRoute route)
    {
        if (!await TryInitialDataAsync(route))
        {
            await route.AbortAsync();
        }
    }

    private static async Task<bool> TryInitialDataAsync(IRoute route)
    {
        if (route.Request.Method != "GET")
        {
            return false;
        }

        switch (Path(route))
        {
            case "/api/admin/programs":
                await JsonAsync(route, 200, ProgramsJson());
                return true;
            case "/api/admin/catalogue/versions":
                await JsonAsync(route, 200, VersionsJson());
                return true;
            case $"/api/admin/catalogue/drafts/{DraftId}":
                await JsonAsync(route, 200, DraftJson());
                return true;
            case "/api/admin/policies":
                await JsonAsync(route, 200, PoliciesJson());
                return true;
            case $"/api/admin/catalogue/imports/{ImportId}":
                await JsonAsync(route, 200, ImportJson());
                return true;
            default:
                return false;
        }
    }

    private static string Path(IRoute route) =>
        new Uri(route.Request.Url).AbsolutePath;

    private static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new RouteFulfillOptions
        {
            Status = status,
            ContentType = "application/json",
            Body = body
        });

    private static string ProgramsJson() => """
        {
          "items": [
            {
              "id": "00000000-0000-0000-0000-000000000906",
              "code": "AI-DS",
              "displayName": "Data Science",
              "active": true,
              "provenance": {
                "sourceReference": "https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655",
                "accessedOn": "2026-07-13",
                "sourceKind": "official-source",
                "syntheticFields": ["Active"]
              }
            }
          ],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "code,id"
        }
        """;

    private static string VersionsJson() => $$"""
        {
          "items": [
            {
              "id": "{{VersionId}}",
              "scope": "AI-DS",
              "version": "DEMO-CATALOGUE-2026.1",
              "state": "published",
              "source": "AASTMT public programme snapshot",
              "publishedAtUtc": "2026-07-13T12:00:00Z"
            }
          ],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "publishedAtUtc-desc,id"
        }
        """;

    private static string DraftJson()
    {
        var codes = new[]
        {
            "BA101", "BA113", "GN111", "GN112", "BA102", "GN121", "GN123",
            "BA203", "GN211", "IN211", "DS221", "GN223", "IN221", "IN311",
            "DS312", "DS322", "DS324", "DS413", "DS421"
        };
        var courses = codes.Select((code, index) => new
        {
            id = Guid.Parse($"00000000-0000-0000-0000-{index + 910:000000000000}"),
            code,
            title = code == "DS413" ? "Project I" : $"Course {code}",
            credits = 3,
            active = true,
            provenance = new
            {
                sourceReference =
                    "https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655",
                accessedOn = "2026-07-13",
                sourceKind = "official-source",
                syntheticFields = new[] { "Credits", "IsActive" }
            },
            rowVersion = $"course-rv-{index + 1}"
        });
        return JsonSerializer.Serialize(new
        {
            id = Guid.Parse(DraftId),
            scope = "AI-DS",
            basedOnVersionId = Guid.Parse(VersionId),
            state = "editing",
            canonicalContentHash = "sha256:CATALOGUE-009",
            rowVersion = "draft-rv-1",
            programs = JsonDocument.Parse(ProgramsJson()).RootElement
                .GetProperty("items")
                .Clone(),
            courses,
            curricula = Array.Empty<object>()
        });
    }

    private static string PoliciesJson() => $$"""
        {
          "items": [
            {
              "id": "{{PolicyId}}",
              "scope": "AI-DS",
              "version": "DEMO-POC-2026.1",
              "state": "validated",
              "rowVersion": "policy-rv-1",
              "rules": [
                {
                  "id": "00000000-0000-0000-0000-000000000940",
                  "code": "NORMAL_MAX_CREDITS",
                  "valueType": "number",
                  "value": 18,
                  "effectiveFromUtc": "2026-07-13T00:00:00Z",
                  "effectiveToUtc": null,
                  "sourceReference": "SRC-GENERAL-2016",
                  "sourceKind": "official-source"
                },
                {
                  "id": "00000000-0000-0000-0000-000000000941",
                  "code": "PROBATION_MAX_CREDITS",
                  "valueType": "number",
                  "value": 12,
                  "effectiveFromUtc": "2026-07-13T00:00:00Z",
                  "effectiveToUtc": null,
                  "sourceReference": "SRC-GENERAL-2016",
                  "sourceKind": "official-source"
                }
              ]
            }
          ],
          "page": 1,
          "pageSize": 20,
          "totalCount": 1,
          "sort": "effectiveFromUtc-desc,id"
        }
        """;

    private static string ImportJson() => $$"""
        {
          "id": "{{ImportId}}",
          "draftId": "{{DraftId}}",
          "state": "uploaded",
          "source": "AASTMT public programme snapshot",
          "accessedOn": "2026-07-13",
          "contentHash": "sha256:IMPORT-009",
          "syntheticFieldCount": 2,
          "rowVersion": "import-rv-1",
          "errors": [],
          "totalErrorCount": 0,
          "hasMoreErrors": false,
          "publishedVersionId": null
        }
        """;

    private static string SimulationJson() => """
        {
          "eligible": true,
          "policyVersion": "DEMO-POC-2026.1",
          "ruleResults": [
            {
              "ruleCode": "DS413_MIN_GPA",
              "passed": true,
              "requiredValue": "2.0",
              "currentValue": "2.50",
              "sourceReference": "SRC-DATA-SCIENCE",
              "sourceKind": "official-source",
              "explanation": "DS413_MIN_GPA passed."
            }
          ]
        }
        """;

    private static string ValidPreviewJson() => """
        {
          "valid": true,
          "errors": [],
          "previewToken": "preview-token-009",
          "expectedDraftRowVersion": "draft-rv-1",
          "dependencyVersions": {
            "policy": "DEMO-POC-2026.1"
          }
        }
        """;

    private static string PublishedVersionJson() => $$"""
        {
          "id": "{{VersionId}}",
          "scope": "AI-DS",
          "version": "DEMO-CATALOGUE-2026.2",
          "state": "published",
          "source": "AASTMT public programme snapshot",
          "publishedAtUtc": "2026-07-16T12:00:00Z"
        }
        """;
}
