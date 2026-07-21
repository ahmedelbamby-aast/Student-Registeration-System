using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.Client.Features.Operations;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class AuditAdministrationPageComponentTests
{
    [Fact]
    public void Adm_09_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        AdminPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("AuditAdministrationPage", "ADM-09");

    [Fact]
    public void Adm_09_renders_bounded_filters_and_keeps_export_actions_disabled_until_server_binding()
    {
        using var context = new BunitContext();
        var httpClient = new HttpClient(new PendingHandler()) { BaseAddress = new Uri("https://localhost") };
        context.Services.AddSingleton(new AdminOperationsApiClient(httpClient));
        context.Services.AddSingleton(new AcademicApiClient(httpClient));
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.AuditAdministrationPage");
        Assert.NotNull(pageType);

        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main#main-content"));
        Assert.NotNull(cut.Find(".srs-app-shell__state--loading"));
    }

    [Fact]
    public void Adm_09_preserves_bounded_filters_paging_immutable_detail_and_export_states()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "aria-label=\"@(LocalizedUiText.Get(\"Scoped audit filters\"))\"",
            "name=\"pageSize\"",
            "max=\"100\"",
            "data-testid=\"audit-event-table\"",
            "data-testid=\"audit-event-card-list\"",
            "Immutable event detail",
            "TotalPages",
            "SearchAsync",
            "queued",
            "ready",
            "failed",
            "expired",
            "restricted");
        Assert.DoesNotContain("Edit event", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Delete event", page, StringComparison.OrdinalIgnoreCase);
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
