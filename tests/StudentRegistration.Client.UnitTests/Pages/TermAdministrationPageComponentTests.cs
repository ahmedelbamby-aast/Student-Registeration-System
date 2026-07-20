using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class TermAdministrationPageComponentTests
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor";

    [Fact]
    public void Loading_state_has_the_stable_main_heading_and_polite_status_contract()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(new HttpClient(new PendingHandler())
        {
            BaseAddress = new Uri("https://adm02.test")
        });
        RegisterAcademicApiClientWhenDelivered(context.Services);

        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, RequiredPageType()));

        Assert.NotNull(cut.Find("main#main-content"));
        var loading = cut.Find(".srs-app-shell__state--loading");
        Assert.Equal("status", loading.GetAttribute("role"));
        Assert.Equal("true", loading.GetAttribute("aria-busy"));
        Assert.Empty(cut.FindAll("[data-testid='create-term-submit']"));
    }

    [Theory]
    [InlineData("ADM-02-COMP-STATE-LOADING")]
    [InlineData("ADM-02-COMP-STATE-EMPTY")]
    [InlineData("ADM-02-COMP-STATE-SUCCESS")]
    [InlineData("ADM-02-COMP-STATE-VALIDATION-ERROR")]
    [InlineData("ADM-02-COMP-STATE-SERVICE-ERROR")]
    [InlineData("ADM-02-COMP-STATE-UNAUTHORIZED")]
    [InlineData("ADM-02-COMP-STATE-SESSION-EXPIRED")]
    [InlineData("ADM-02-COMP-STATE-STALE")]
    [InlineData("ADM-02-COMP-STATE-OFFLINE")]
    public void All_nine_approved_complete_state_fixture_ids_are_bound(string stateFixtureId)
    {
        Assert.Contains(stateFixtureId, RequiredPageSource(), StringComparison.Ordinal);
    }

    [Fact]
    public void Success_and_empty_states_bind_term_list_editor_windows_and_server_owned_values()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "data-testid=\"term-administration-empty\"",
            "data-testid=\"term-list\"",
            "data-testid=\"term-editor\"",
            "data-testid=\"registration-window-editor\"",
            "Term code",
            "Display name",
            "Institutional time zone",
            "Teaching start date",
            "Teaching end date",
            "Registration opens",
            "Registration closes",
            "Reason",
            "Source",
            "Create term",
            "Save changes");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validation_overlap_and_stale_states_focus_reviewable_details_without_false_success()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "data-testid=\"term-validation-summary\"",
            "aria-live=\"assertive\"",
            "tabindex=\"-1\"",
            "VALIDATION_ERROR",
            "WINDOW_OVERLAP",
            "STALE_VERSION",
            "data-testid=\"conflicting-registration-windows\"",
            "data-testid=\"stale-changed-fields\"",
            "data-testid=\"refresh-stale-term\"");
        Assert.DoesNotContain("overwrite anyway", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("queued success", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Publish_confirmation_is_explicit_and_restores_focus_to_its_trigger()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "data-testid=\"publish-registration-window\"",
            "data-testid=\"publish-registration-window-dialog\"",
            "Publish registration window",
            "Cancel",
            "Confirm publish",
            "aria-modal=\"true\"",
            "FocusAsync");
    }

    [Fact]
    public void Create_save_and_publish_each_have_one_pending_guard_and_cannot_duplicate_commands()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "_isCreating",
            "_isSaving",
            "_isPublishing",
            "if (_isCreating)",
            "if (_isSaving)",
            "if (_isPublishing)",
            "disabled=\"@_isCreating\"",
            "disabled=\"@_isSaving\"",
            "disabled=\"@_isPublishing\"",
            "CreateAdminTermAsync",
            "UpdateAdminTermAsync",
            "PublishAdminRegistrationWindowAsync");
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
    }

    private static Type RequiredPageType()
    {
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.TermAdministrationPage");
        Assert.True(
            pageType is not null,
            "TermAdministrationPage is intentionally absent until SPEC-008/T080; ADM-02-COMP-T194 remains red.");
        return pageType!;
    }

    private static string RequiredPageSource()
    {
        var path = RepositoryFiles.PathTo(PagePath);
        Assert.True(
            File.Exists(path),
            "TermAdministrationPage.razor is intentionally absent until SPEC-008/T080; ADM-02-COMP-T194 remains red.");
        return File.ReadAllText(path);
    }

    private static void RegisterAcademicApiClientWhenDelivered(IServiceCollection services)
    {
        var apiClientType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Features.Academics.AcademicApiClient");
        if (apiClientType is not null)
        {
            services.AddScoped(apiClientType);
        }
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
