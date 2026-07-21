using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentDashboardPageComponentTests
{
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor";

    [Fact]
    public void Stu_01_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        StudentPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("StudentDashboardPage", "STU-01");

    [Fact]
    public void Loading_state_keeps_stable_landmarks_named_progress_and_heading_focus_target()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(new HttpClient(new PendingHandler())
        {
            BaseAddress = new Uri("https://stu01.test")
        });
        RegisterAcademicApiClient(context.Services);

        var pageType = RequiredPageType();
        var cut = context.Render<DynamicComponent>(parameters =>
            parameters.Add(component => component.Type, pageType));

        Assert.NotNull(cut.Find("main"));
        Assert.NotNull(cut.Find("[data-testid='student-dashboard-page']"));
        var heading = cut.Find("h1#student-dashboard-heading");
        Assert.Equal("Student dashboard", heading.TextContent.Trim());
        var loading = cut.Find("[data-testid='student-dashboard-loading']");
        Assert.Equal("status", loading.GetAttribute("role"));
        Assert.Equal("polite", loading.GetAttribute("aria-live"));
        Assert.Empty(cut.FindAll("[data-testid='registration-primary-action']"));
    }

    [Fact]
    public void Page_declares_all_nine_governed_state_ids_and_route_test_ids()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "STU-01-COMP-T149",
            "STU-01-COMP-STATE-LOADING",
            "STU-01-COMP-STATE-EMPTY",
            "STU-01-COMP-STATE-SUCCESS",
            "STU-01-COMP-STATE-VALIDATION-ERROR",
            "STU-01-COMP-STATE-SERVICE-ERROR",
            "STU-01-COMP-STATE-UNAUTHORIZED",
            "STU-01-COMP-STATE-SESSION-EXPIRED",
            "STU-01-COMP-STATE-STALE",
            "STU-01-COMP-STATE-OFFLINE",
            "STU-01-E2E-PRIMARY",
            "STU-01-E2E-FAILURE",
            "STU-01-A11Y-T150",
            "STU-01-VIS-T151");
    }

    [Fact]
    public void Open_upcoming_closed_and_no_term_states_use_server_window_data_and_named_navigation()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "RegistrationWindowState.Open",
            "RegistrationWindowState.Upcoming",
            "RegistrationWindowState.Closed",
            "RegistrationWindowState.None",
            "data-testid=\"registration-primary-action\"",
            "Start registration",
            "Resume registration",
            "href=\"/student/subjects\"",
            "href=\"/student/schedule\"",
            "href=\"/student/registrations\"",
            "href=\"/student/account\"",
            "WINDOW_CLOSED",
            "No current registration term");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("setInterval", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Hold_and_incomplete_profile_blockers_precede_and_describe_the_disabled_action()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "data-testid=\"registration-blockers\"",
            "REGISTRATION_HOLD",
            "PROFILE_NOT_READY",
            "DescribedBy",
            "Disabled",
            "BlocksRegistration");
        Assert.True(
            source.IndexOf(
                "data-testid=\"registration-blockers\"",
                StringComparison.Ordinal) <
            source.IndexOf(
                "data-testid=\"registration-primary-action\"",
                StringComparison.Ordinal),
            "STU-01-COMP-T149 requires every blocker before its disabled action.");
        Assert.DoesNotContain("Dismiss hold", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Override hold", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Focus_live_regions_refresh_pending_and_duplicate_action_guards_are_explicit()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "id=\"student-dashboard-heading\"",
            "tabindex=\"-1\"",
            "aria-live=\"polite\"",
            "aria-live=\"assertive\"",
            "data-testid=\"student-dashboard-refresh\"",
            "_isRefreshing",
            "Disabled=\"@_isRefreshing\"",
            "if (_isRefreshing)",
            "GetAppContextAsync",
            "GetStudentAcademicContextAsync");
        Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
        Assert.DoesNotContain("queued success", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Current_timetable_links_to_the_authoritative_registration_record()
    {
        var source = RequiredPageSource();

        RepositoryFiles.ContainsAll(
            source,
            "Current timetable",
            "Open current timetable",
            "/student/registrations#current-registration",
            "data-contributor-state=\"available\"");
        Assert.DoesNotContain("GetCurrentTimetableAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("sample timetable", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mock timetable", source, StringComparison.OrdinalIgnoreCase);
    }

    private static Type RequiredPageType()
    {
        var pageType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Pages.StudentDashboardPage");
        Assert.True(
            pageType is not null,
            "StudentDashboardPage is intentionally absent until SPEC-008/T078; T149 remains red.");
        return pageType!;
    }

    private static string RequiredPageSource()
    {
        var path = RepositoryFiles.PathTo(PagePath);
        Assert.True(
            File.Exists(path),
            "StudentDashboardPage.razor is intentionally absent until SPEC-008/T078; T149 remains red.");
        return File.ReadAllText(path);
    }

    private static void RegisterAcademicApiClient(IServiceCollection services)
    {
        var apiClientType = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Features.Academics.AcademicApiClient");
        Assert.NotNull(apiClientType);

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
