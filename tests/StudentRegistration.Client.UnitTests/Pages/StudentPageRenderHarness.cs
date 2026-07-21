using System.Net;
using System.Text;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.Client.Components.Layout;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.Contracts;

namespace StudentRegistration.Client.UnitTests.Pages;

internal static class StudentPageRenderHarness
{
    internal static void AssertAuthenticatedShellAndMenuOperate(string pageTypeName, string routeId)
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var httpClient = new HttpClient(new StudentContextThenFailureHandler())
        {
            BaseAddress = new Uri("https://student-render.test")
        };

        context.Services.AddSingleton(httpClient);
        context.Services.AddSingleton(new AcademicApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new CatalogueApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new IdentityApiClient(httpClient, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new RegistrationApiClient(httpClient, context.JSInterop.JSRuntime));

        var pageType = typeof(App).Assembly.GetType($"StudentRegistration.Client.Pages.{pageTypeName}");
        Assert.NotNull(pageType);
        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main"));

        var shell = context.Render<AuthenticatedPage>(parameters => parameters
            .Add(component => component.Context, StudentContext)
            .Add(component => component.Workspace, WorkspaceKind.Student)
            .Add(component => component.CurrentRouteId, routeId)
            .Add(component => component.Density, UiDensity.Comfortable)
            .Add(component => component.Heading, "Deterministic Student route"));

        Assert.NotNull(shell.Find("main#main-content"));
        var menu = shell.Find(".srs-app-shell__menu-button");
        Assert.Equal("false", menu.GetAttribute("aria-expanded"));
        menu.Click();
        shell.WaitForAssertion(() =>
            Assert.Equal("true", shell.Find(".srs-app-shell__menu-button").GetAttribute("aria-expanded")));
        Assert.NotNull(shell.Find(".srs-app-shell__scrim"));
    }

    private static readonly FrontendAppContextView StudentContext = new(
        new DateTimeOffset(2026, 7, 21, 9, 30, 0, TimeSpan.Zero),
        "Africa/Cairo", null, null, RegistrationWindowState.None, null,
        "Student Demo", ["Student"], "Student", "active",
        new DateTimeOffset(2026, 7, 21, 11, 30, 0, TimeSpan.Zero),
        "available", "/status/support");

    private sealed class StudentContextThenFailureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return new TaskCompletionSource<HttpResponseMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously).Task;
        }
    }
}
