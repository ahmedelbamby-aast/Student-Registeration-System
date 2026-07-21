using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentAdministrationPageComponentTests
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor";

    [Fact]
    public void Adm_04_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        AdminPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("StudentAdministrationPage", "ADM-04");

    [Fact]
    public void Loading_state_has_a_stable_heading_polite_status_and_no_false_results()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(new HttpClient(new PendingHandler())
        {
            BaseAddress = new Uri("https://adm04.test")
        });
        RegisterAcademicApiClientWhenDelivered(context.Services);

        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, RequiredPageType()));

        Assert.NotNull(cut.Find("main#main-content"));
        var loading = cut.Find(".srs-app-shell__state--loading");
        Assert.Equal("status", loading.GetAttribute("role"));
        Assert.Equal("true", loading.GetAttribute("aria-busy"));
        Assert.Empty(cut.FindAll("[data-testid='student-search-results']"));
    }

    [Fact]
    public void All_nine_states_have_stable_ids_live_regions_and_safe_recovery_actions()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "ADM-04-COMP-STATE-LOADING",
            "ADM-04-COMP-STATE-EMPTY",
            "ADM-04-COMP-STATE-SUCCESS",
            "ADM-04-COMP-STATE-VALIDATION-ERROR",
            "ADM-04-COMP-STATE-SERVICE-ERROR",
            "ADM-04-COMP-STATE-UNAUTHORIZED",
            "ADM-04-COMP-STATE-SESSION-EXPIRED",
            "ADM-04-COMP-STATE-STALE",
            "ADM-04-COMP-STATE-OFFLINE",
            "aria-live=\"polite\"",
            "aria-live=\"assertive\"",
            "Clear Student search",
            "Refresh Student context",
            "Return to Admin home",
            "Sign in again");
        Assert.DoesNotContain("queued success", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Success_read_is_noneditable_and_exposes_sourced_detail_before_correction()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "data-testid=\"student-search\"",
            "data-testid=\"student-search-results\"",
            "data-testid=\"academic-summary\"",
            "data-testid=\"academic-transcript\"",
            "data-testid=\"academic-holds\"",
            "data-testid=\"academic-provenance\"",
            "DataAsOfUtc",
            "StudentRowVersion",
            "StudentTermStateRowVersion",
            "data-testid=\"open-academic-correction\"");
        Assert.DoesNotContain("contenteditable", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Correction_dialog_requires_reason_links_errors_and_guards_duplicate_submission()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "role=\"dialog\"",
            "aria-modal=\"true\"",
            "data-testid=\"correction-validation-summary\"",
            "id=\"academic-correction-reason\"",
            "for=\"academic-correction-reason\"",
            "Reason.Length < 10",
            "ExpectedStudentRowVersion",
            "ExpectedStudentTermStateRowVersion",
            "_isSubmittingCorrection",
            "Disabled=\"@_isSubmittingCorrection\"",
            "IsLoading=\"@_isSubmittingCorrection\"",
            "if (_isSubmittingCorrection)",
            "CorrectAdminStudentAcademicProfileAsync");
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Invalid_stale_and_restricted_results_never_commit_or_retain_protected_state()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "VALIDATION_ERROR",
            "STALE_VERSION",
            "currentVersion",
            "PurgeProtectedState",
            "_students = []",
            "_selectedStudent = null",
            "unauthorized",
            "session-expired",
            "FocusAsync");
        Assert.DoesNotContain("force overwrite", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IgnoreConcurrency", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Privacy_purge_clears_search_correction_and_pagination_metadata()
    {
        var source = RequiredPageSource();
        var purgeStart = source.IndexOf(
            "private void PurgeProtectedState()",
            StringComparison.Ordinal);
        Assert.True(purgeStart >= 0, "PurgeProtectedState must remain explicit and reviewable.");
        var purgeEnd = source.IndexOf(
            "private void ClearFailure()",
            purgeStart,
            StringComparison.Ordinal);

        Assert.True(purgeEnd > purgeStart, "The privacy-purge boundary could not be inspected.");
        var purge = source[purgeStart..purgeEnd];
        RepositoryFiles.ContainsAll(
            purge,
            "_students = []",
            "_selectedStudent = null",
            "_searchQuery = string.Empty",
            "_lastSubmittedQuery = string.Empty",
            "_correction = new CorrectionDraft()",
            "_currentPage = 1",
            "_pageSize = SearchPageSize",
            "_totalCount = 0");
    }

    private static Type RequiredPageType()
    {
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.StudentAdministrationPage");
        Assert.True(
            pageType is not null,
            "StudentAdministrationPage is intentionally absent until SPEC-008/T082; T204 remains red.");
        return pageType!;
    }

    private static string RequiredPageSource()
    {
        var path = RepositoryFiles.PathTo(PagePath);
        Assert.True(
            File.Exists(path),
            "StudentAdministrationPage.razor is intentionally absent until SPEC-008/T082; T204 remains red.");
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
