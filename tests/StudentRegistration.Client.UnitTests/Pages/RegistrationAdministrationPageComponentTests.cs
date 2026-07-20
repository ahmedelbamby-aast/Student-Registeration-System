using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Operations;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RegistrationAdministrationPageComponentTests
{
    [Fact]
    public void Adm_08_renders_the_honest_loading_shell_without_mutation_controls()
    {
        using var context = new BunitContext();
        var httpClient = new HttpClient(new PendingHandler())
        {
            BaseAddress = new Uri("https://localhost")
        };
        context.Services.AddSingleton(new AcademicApiClient(httpClient));
        context.Services.AddSingleton(new AdminOperationsApiClient(httpClient));
        context.Services.AddSingleton(new RegistrationApiClient(httpClient));
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.RegistrationAdministrationPage");
        Assert.NotNull(pageType);

        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main#main-content"));
        Assert.Equal("loading", cut.Find("[data-route-id='ADM-08']").GetAttribute("data-state"));
        Assert.Contains("No repair or correction actions are available", cut.Markup);
        Assert.Empty(cut.FindAll("button:not([disabled])[type='submit']"));
    }

    [Fact]
    public void Adm_08_preserves_timestamp_filters_semantic_results_and_read_only_alerts()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "data-testid=\"registration-monitor-observed-at\"",
            "aria-label=\"@(LocalizedUiText.Get(\"Registration monitoring filters\"))\"",
            "data-testid=\"registration-monitor-table\"",
            "data-testid=\"registration-monitor-card-list\"",
            "Selected receipt detail",
            "Reconciliation alerts",
            "supportReferencePath",
            "loading",
            "empty",
            "stale",
            "degraded",
            "unauthorized",
            "offline");
        Assert.DoesNotContain("Drop registration", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Withdraw", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Resume group", page, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class PendingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable.");
        }
    }
}
