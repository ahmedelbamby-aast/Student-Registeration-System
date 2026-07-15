using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class RoleGatewayPageComponentTests
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/RoleGatewayPage.razor";

    [Fact]
    public void Loading_state_renders_with_stable_landmarks_polite_status_and_heading_focus_target()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(new HttpClient(new PendingHandler())
        {
            BaseAddress = new Uri("https://auth01.test")
        });
        RegisterAcademicApiClientWhenDelivered(context.Services);

        var pageType = RequiredPageType();
        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main[data-testid='role-gateway-page']"));
        var heading = cut.Find("h1#role-gateway-heading");
        Assert.Equal("-1", heading.GetAttribute("tabindex"));
        var loading = cut.Find("[data-testid='public-context-loading']");
        Assert.Equal("status", loading.GetAttribute("role"));
        Assert.Equal("polite", loading.GetAttribute("aria-live"));
        Assert.Empty(cut.FindAll("[data-testid='student-login-link']"));
    }

    [Fact]
    public void Success_state_has_named_navigation_and_no_editable_form_or_client_time_authority()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "AUTH-01-COMP-STATE-SUCCESS",
            "data-testid=\"student-login-link\"",
            "href=\"/student/login\"",
            "data-testid=\"student-activation-link\"",
            "href=\"/student/activate\"",
            "data-testid=\"staff-login-link\"",
            "href=\"/staff/login\"");
        Assert.DoesNotContain("<EditForm", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<form", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Failure_stale_and_offline_states_preserve_safe_codes_focus_and_recovery_actions()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "AUTH-01-COMP-STATE-SERVICE-ERROR",
            "SERVICE_UNAVAILABLE",
            "MAINTENANCE",
            "WINDOW_CHANGED",
            "AUTH-01-COMP-STATE-STALE",
            "AUTH-01-COMP-STATE-OFFLINE",
            "Retry public context",
            "aria-live=\"polite\"",
            "tabindex=\"-1\"");
        Assert.DoesNotContain("stack trace", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sql", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("queued success", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Retry_has_one_pending_action_and_cannot_issue_duplicate_requests()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "data-testid=\"public-context-retry\"",
            "_isRetrying",
            "disabled=\"@_isRetrying\"",
            "if (_isRetrying)",
            "GetPublicContextAsync");
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
    }

    private static Type RequiredPageType()
    {
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.RoleGatewayPage");
        Assert.True(
            pageType is not null,
            "RoleGatewayPage is intentionally absent until SPEC-008/T076; T124 remains red.");
        return pageType!;
    }

    private static string RequiredPageSource()
    {
        var path = RepositoryFiles.PathTo(PagePath);
        Assert.True(
            File.Exists(path),
            "RoleGatewayPage.razor is intentionally absent until SPEC-008/T076; T124 remains red.");
        return File.ReadAllText(path);
    }

    private static void RegisterAcademicApiClientWhenDelivered(IServiceCollection services)
    {
        var apiClientType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Features.Academics.AcademicApiClient");
        if (apiClientType is null)
        {
            return;
        }

        var httpClient = services.BuildServiceProvider().GetRequiredService<HttpClient>();
        var apiClient = Activator.CreateInstance(apiClientType, httpClient);
        Assert.NotNull(apiClient);
        services.AddSingleton(apiClientType, apiClient);
    }

    private sealed class PendingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            new TaskCompletionSource<HttpResponseMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously).Task;
    }
}
