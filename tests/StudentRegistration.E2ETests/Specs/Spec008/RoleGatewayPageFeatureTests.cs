using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec008;

public sealed class RoleGatewayPageFrozenContractTests
{
    [Fact]
    public void Auth_01_owner_metadata_is_frozen_and_the_owner_page_exists()
    {
        var design = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/pages/AUTH-01.md");

        RepositoryFiles.ContainsAll(
            design,
            "\"routeTemplate\": \"/\"",
            "\"pageName\": \"RoleGatewayPage.razor\"",
            "GET /api/public/context",
            "AUTH-01-E2E-PRIMARY",
            "AUTH-01-E2E-FAILURE",
            "Student login -> AUTH-02",
            "Student activation -> AUTH-03",
            "Staff login -> AUTH-04");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Client/Pages/RoleGatewayPage.razor"),
            "AUTH-01-E2E-PRIMARY: SPEC-008/T076 must deliver RoleGatewayPage.razor.");
    }
}

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class RoleGatewayPageFeatureTests(Spec008BrowserFixture fixture)
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

    [Fact]
    public async Task Auth_01_e2e_primary_uses_server_context_and_named_destinations()
    {
        await using var context = await fixture.OpenContextAsync();
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
        var heading = page.GetByRole(
            AriaRole.Heading,
            new PageGetByRoleOptions { Name = "Role gateway", Exact = true });
        await heading.WaitForAsync();
        await page.Locator("[data-route-id='AUTH-01'][data-state='success']")
            .WaitForAsync();

        Assert.True(await heading.IsVisibleAsync());
        Assert.True(await page.GetByText("Africa/Cairo", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Summer 2026", new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("Fall 2026", new() { Exact = true }).IsVisibleAsync());
        Assert.Equal(
            "/student/login",
            await page.GetByRole(AriaRole.Link, new() { Name = "Student login", Exact = true })
                .GetAttributeAsync("href"));
        Assert.Equal(
            "/student/activate",
            await page.GetByRole(AriaRole.Link, new() { Name = "Student activation", Exact = true })
                .GetAttributeAsync("href"));
        Assert.Equal(
            "/staff/login",
            await page.GetByRole(AriaRole.Link, new() { Name = "Staff login", Exact = true })
                .GetAttributeAsync("href"));

        var content = await page.ContentAsync();
        Assert.DoesNotContain("localStorage", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("capacity", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internal health", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("MAINTENANCE", "Public registration status is temporarily unavailable.")]
    [InlineData("SERVICE_UNAVAILABLE", "Public context could not be loaded.")]
    public async Task Auth_01_e2e_failure_is_safe_and_retries_authoritative_context(
        string reasonCode,
        string safeMessage)
    {
        await using var context = await fixture.OpenContextAsync(375, 812);
        var page = await context.NewPageAsync();
        var attempts = 0;
        await page.RouteAsync("**/api/public/context", async route =>
        {
            attempts++;
            if (attempts == 1)
            {
                await route.FulfillAsync(new RouteFulfillOptions
                {
                    Status = 503,
                    ContentType = "application/json",
                    Body = $$"""
                        {
                          "code": "{{reasonCode}}",
                          "message": "{{safeMessage}}",
                          "correlationId": "AUTH-01-SAFE-REF"
                        }
                        """
                });
                return;
            }

            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = AvailableContext
            });
        });

        await page.GotoAsync("/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await page.GetByText(reasonCode, new() { Exact = true }).WaitForAsync();
        Assert.True(await page.GetByText(safeMessage, new() { Exact = true }).IsVisibleAsync());
        Assert.True(await page.GetByText("AUTH-01-SAFE-REF", new() { Exact = true }).IsVisibleAsync());

        await page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Retry public context", Exact = true })
            .ClickAsync();
        await page.GetByRole(
            AriaRole.Heading,
            new PageGetByRoleOptions { Name = "Role gateway", Exact = true })
            .WaitForAsync();
        await page.Locator("[data-route-id='AUTH-01'][data-state='success']")
            .WaitForAsync();
        Assert.Equal(2, attempts);
    }
}
