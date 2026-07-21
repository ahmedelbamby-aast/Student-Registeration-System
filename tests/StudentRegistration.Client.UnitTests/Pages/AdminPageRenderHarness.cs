using System.Net;
using System.Text;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Features.Operations;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.Client.Features.Scheduling;
using StudentRegistration.Client.Components.Layout;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Contracts;

namespace StudentRegistration.Client.UnitTests.Pages;

internal static class AdminPageRenderHarness
{
    internal static void AssertAuthenticatedShellAndMenuOperate(string pageTypeName, string routeId)
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(new ContextThenPendingHandler())
        {
            BaseAddress = new Uri("https://admin-render.test")
        };

        context.Services.AddSingleton(httpClient);
        context.Services.AddSingleton(new AcademicApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new AdminOperationsApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new CatalogueApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new IdentityApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new RegistrationApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new RegistrationApprovalApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new SchedulingApiClient(httpClient, context.JSInterop.JSRuntime));

        var pageType = typeof(App).Assembly.GetType($"StudentRegistration.Client.Pages.{pageTypeName}");
        Assert.NotNull(pageType);
        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main#main-content"));
        Assert.NotNull(cut.Find(".srs-app-shell"));

        var shell = context.Render<AuthenticatedPage>(parameters => parameters
            .Add(component => component.Context, AdminContext)
            .Add(component => component.Workspace, WorkspaceKind.Admin)
            .Add(component => component.CurrentRouteId, routeId)
            .Add(component => component.Density, UiDensity.Compact)
            .Add(component => component.Heading, "Deterministic Admin route"));

        Assert.NotNull(shell.Find("main#main-content"));
        Assert.NotNull(shell.Find(".srs-app-shell__sidebar[aria-label='Workspace navigation']"));
        var menu = shell.Find(".srs-app-shell__menu-button");
        Assert.Equal("false", menu.GetAttribute("aria-expanded"));
        menu.Click();
        shell.WaitForAssertion(() =>
            Assert.Equal("true", shell.Find(".srs-app-shell__menu-button").GetAttribute("aria-expanded")));
        Assert.NotNull(shell.Find(".srs-app-shell__scrim"));
    }

    private static readonly FrontendAppContextView AdminContext = new(
        new DateTimeOffset(2026, 7, 21, 9, 30, 0, TimeSpan.Zero),
        "Africa/Cairo", null, null, RegistrationWindowState.None, null,
        "Admin Demo", ["Admin"], "Admin", "active",
        new DateTimeOffset(2026, 7, 21, 11, 30, 0, TimeSpan.Zero),
        "available", "/status/support");

    private sealed class ContextThenPendingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/api/context")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ContextJson, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent(ErrorJson, Encoding.UTF8, "application/json")
            });
        }

        private const string ContextJson = """
            {"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo",
             "teachingTerm":null,"registrationTerm":null,"registrationWindowState":"open",
             "registrationWindow":null,"serviceState":"available","displayName":"Admin Demo",
             "authorizedRoles":["Admin"],"activeRole":"Admin","sessionState":"active",
             "expiresAtUtc":"2026-07-21T11:30:00Z","supportReferencePath":"/status/support"}
            """;

        private const string ErrorJson = """
            {"code":"SERVICE_UNAVAILABLE","message":"The deterministic owner service is unavailable.",
             "correlationId":"ADM-BUNIT-SAFE-REF"}
            """;
    }
}
