using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Pages;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;

namespace StudentRegistration.E2ETests.Specs.Spec007;

/// <summary>
/// Executable client-to-HTTP journeys. These render the real Razor pages,
/// interact with their DOM, execute IdentityApiClient, and verify navigation
/// and the antiforgery-bearing request rather than inspecting source text.
/// </summary>
public sealed class ExecutableIdentityJourneys
{
    [Fact]
    public async Task Student_can_submit_real_login_page_and_reach_student_workspace()
    {
        var handler = new RecordingHandler(request =>
            Json(
                new SessionDto(
                    "Synthetic Student",
                    ["Student"],
                    "Student",
                    "active",
                    DateTime.UtcNow.AddHours(1))));
        using var context = CreateContext(handler);
        var page = context.Render<StudentLoginPage>();

        page.Find("#student-university-id").Input("202600001");
        page.Find("#student-password").Input(CreateSecret());
        page.Find("form[data-testid=student-login-form]").Submit();

        page.WaitForAssertion(() => Assert.EndsWith(
            "/student",
            context.Services.GetRequiredService<NavigationManager>().Uri,
            StringComparison.Ordinal));
        var request = Assert.Single(handler.Requests);
        Assert.Equal("/api/auth/student/login", request.Uri.AbsolutePath);
        Assert.Equal("test-xsrf", Assert.Single(request.AntiforgeryValues));
        var body = JsonSerializer.Deserialize<StudentLoginRequest>(
            request.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal("202600001", body?.UniversityId);
        Assert.NotEmpty(body?.Password ?? string.Empty);
    }

    [Fact]
    public void Student_login_validation_executes_without_sending_a_request()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException());
        using var context = CreateContext(handler);
        var page = context.Render<StudentLoginPage>();

        page.Find("form[data-testid=student-login-form]").Submit();

        page.WaitForAssertion(() =>
        {
            Assert.Contains("Enter your University ID", page.Markup, StringComparison.Ordinal);
            Assert.Contains("Enter your password", page.Markup, StringComparison.Ordinal);
        });
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public void Student_activation_posts_three_secrets_then_clears_and_navigates()
    {
        var handler = new RecordingHandler(_ =>
            Json(
                new SessionDto(
                    "Synthetic Student",
                    ["Student"],
                    "Student",
                    "activated",
                    DateTime.UtcNow.AddHours(1))));
        using var context = CreateContext(handler);
        var page = context.Render<StudentActivationPage>();
        var newPassword = CreateSecret();

        page.Find("#activation-university-id").Input("202600002");
        page.Find("#activation-initial-password").Input(CreateSecret());
        page.Find("#activation-new-password").Input(newPassword);
        page.Find("#activation-confirm-password").Input(newPassword);
        page.Find("form[data-testid=student-activation-form]").Submit();

        page.WaitForAssertion(() => Assert.EndsWith(
            "/student/login",
            context.Services.GetRequiredService<NavigationManager>().Uri,
            StringComparison.Ordinal));
        Assert.All(
            page.FindAll("input[type=password]"),
            input => Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty));
        Assert.Equal("/api/auth/student/activate", Assert.Single(handler.Requests).Uri.AbsolutePath);
    }

    [Fact]
    public void Student_activation_blocks_invalid_confirmation_and_renders_a_race_loser_generically()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new ApiError(
                "ACTIVATION_FAILED",
                "The activation request could not be completed.",
                "spec007-activation-race"))
        });
        using var context = CreateContext(handler);
        var page = context.Render<StudentActivationPage>();

        page.Find("#activation-university-id").Input("202600002");
        page.Find("#activation-initial-password").Input(CreateSecret());
        page.Find("#activation-new-password").Input(CreateSecret());
        page.Find("#activation-confirm-password").Input(CreateSecret());
        page.Find("form[data-testid=student-activation-form]").Submit();

        page.WaitForAssertion(() => Assert.Contains(
            "The new passwords must match",
            page.Markup,
            StringComparison.Ordinal));
        Assert.Empty(handler.Requests);

        var matchingPassword = CreateSecret();
        page.Find("#activation-initial-password").Input(CreateSecret());
        page.Find("#activation-new-password").Input(matchingPassword);
        page.Find("#activation-confirm-password").Input(matchingPassword);
        page.Find("form[data-testid=student-activation-form]").Submit();

        page.WaitForAssertion(() =>
        {
            var feedback = page.Find("[data-code=ACTIVATION_FAILED]");
            Assert.Equal("alert", feedback.GetAttribute("role"));
            Assert.Contains("Activation not accepted", feedback.TextContent, StringComparison.Ordinal);
        });
        Assert.All(
            page.FindAll("input[type=password]"),
            input => Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty));
        Assert.False(context.Services
            .GetRequiredService<NavigationManager>()
            .Uri
            .EndsWith("/student/login", StringComparison.Ordinal));
        Assert.Equal("/api/auth/student/activate", Assert.Single(handler.Requests).Uri.AbsolutePath);
    }

    [Fact]
    public void Multiple_role_staff_configuration_is_rejected_without_a_context_picker()
    {
        var handler = new RecordingHandler(_ => Json(new SessionDto(
            "Invalid multi-role staff",
            ["Lecturer", "TeachingAssistant"],
            null,
            "role-selection-required",
            DateTime.UtcNow.AddHours(1))));
        using var context = CreateContext(handler);
        var page = context.Render<StaffLoginPage>();

        page.Find("#staff-user-name").Input("LEC-0001");
        page.Find("#staff-password").Input(CreateSecret());
        page.Find("form[data-testid=staff-login-form]").Submit();

        page.WaitForAssertion(() => Assert.NotNull(
            page.Find("[data-code=INVALID_ROLE_CONFIGURATION]")));
        Assert.Empty(page.FindAll("[data-state=role-selection-required]"));
        Assert.False(context.Services.GetRequiredService<NavigationManager>().Uri.EndsWith(
            "/staff",
            StringComparison.Ordinal));
        Assert.Equal("/api/auth/staff/login", Assert.Single(handler.Requests).Uri.AbsolutePath);
    }

    [Fact]
    public void Recovery_request_stays_generic_and_completion_clears_secrets()
    {
        var handler = new RecordingHandler(request => new HttpResponseMessage(
            request.RequestUri!.AbsolutePath.EndsWith("/request", StringComparison.Ordinal)
                ? HttpStatusCode.Accepted
                : HttpStatusCode.NoContent));
        using var context = CreateContext(handler);
        var page = context.Render<AccountRecoveryPage>();

        page.Find("#recovery-identifier").Input("unknown-or-known");
        page.Find("form[data-testid=recovery-request-form]").Submit();
        page.WaitForAssertion(() => Assert.Contains(
            "If an eligible account matches",
            page.Markup,
            StringComparison.Ordinal));

        var newPassword = CreateSecret();
        page.Find("#recovery-code").Input(CreateSecret());
        page.Find("#recovery-new-password").Input(newPassword);
        page.Find("#recovery-confirm-password").Input(newPassword);
        page.Find("form[data-testid=recovery-complete-form]").Submit();

        page.WaitForAssertion(() => Assert.Contains(
            "Recovery complete",
            page.Markup,
            StringComparison.Ordinal));
        Assert.All(
            page.FindAll("input[type=password]"),
            input => Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty));
        Assert.Equal(
            ["/api/auth/recovery/request", "/api/auth/recovery/complete"],
            handler.Requests.Select(request => request.Uri.AbsolutePath).ToArray());
    }

    [Fact]
    public void Student_account_loads_server_session_and_changes_password()
    {
        var handler = new RecordingHandler(request =>
            request.Method == HttpMethod.Get
                ? Json(new SessionDto(
                    "Synthetic Student",
                    ["Student"],
                    "Student",
                    "active",
                    DateTime.UtcNow.AddHours(1)))
                : new HttpResponseMessage(HttpStatusCode.NoContent));
        using var context = CreateContext(handler);
        var page = context.Render<StudentAccountPage>();
        page.WaitForElement("form[data-testid=password-change-form]");
        var newPassword = CreateSecret();

        page.Find("#account-current-password").Input(CreateSecret());
        page.Find("#account-new-password").Input(newPassword);
        page.Find("#account-confirm-password").Input(newPassword);
        page.Find("form[data-testid=password-change-form]").Submit();

        page.WaitForAssertion(() => Assert.Contains(
            "Password changed",
            page.Markup,
            StringComparison.Ordinal));
        Assert.All(
            page.FindAll("input[type=password]"),
            input => Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty));
        Assert.Equal(
            ["/api/auth/session", "/api/auth/password/change"],
            handler.Requests.Select(request => request.Uri.AbsolutePath).ToArray());
    }

    [Fact]
    public void Student_account_confirms_and_revokes_every_session_before_signing_out()
    {
        var handler = new RecordingHandler(request =>
            request.Method == HttpMethod.Get
                ? Json(new SessionDto(
                    "Synthetic Student",
                    ["Student"],
                    "Student",
                    "active",
                    DateTime.UtcNow.AddHours(1)))
                : new HttpResponseMessage(HttpStatusCode.NoContent));
        using var context = CreateContext(handler);
        var page = context.Render<StudentAccountPage>();
        page.WaitForElement("#revoke-all-sessions").Click();

        var dialog = page.WaitForElement("#revoke-all-dialog");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        dialog.QuerySelector("button[data-action=confirm]")!.Click();

        page.WaitForAssertion(() => Assert.EndsWith(
            "/student/login",
            context.Services.GetRequiredService<NavigationManager>().Uri,
            StringComparison.Ordinal));
        Assert.Equal(
            ["/api/auth/session", "/api/auth/sessions/revoke-all"],
            handler.Requests.Select(request => request.Uri.AbsolutePath).ToArray());
        Assert.Equal("test-xsrf", Assert.Single(handler.Requests[1].AntiforgeryValues));
    }

    [Fact]
    public void Admin_user_page_executes_bounded_list_query_and_renders_server_data()
    {
        var user = new IdentityUserSummaryDto(
            Guid.NewGuid(),
            "Demo Lecturer",
            "LEC-0001",
            true,
            ["Lecturer"],
            Convert.ToBase64String([1, 2, 3, 4]));
        var handler = new RecordingHandler(_ =>
            Json(new Page<IdentityUserSummaryDto>(
                [user],
                pageNumber: 1,
                pageSize: 20,
                totalCount: 1,
                sort: "displayName,id")));
        using var context = CreateContext(handler);

        var page = context.Render<UserAdministrationPage>();

        page.WaitForAssertion(() => Assert.Contains(
            "Demo Lecturer",
            page.Markup,
            StringComparison.Ordinal));
        var request = Assert.Single(handler.Requests);
        Assert.Equal("/api/admin/users", request.Uri.AbsolutePath);
        Assert.Contains("pageSize=20", request.Uri.Query, StringComparison.Ordinal);
        Assert.Contains("displayName%2Cid", request.Uri.Query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Admin_user_page_executes_status_role_and_atomic_import_commands()
    {
        var userId = Guid.NewGuid();
        var importId = Guid.NewGuid();
        var current = new IdentityUserSummaryDto(
            userId,
            "Demo Lecturer",
            "LEC-0001",
            true,
            ["Lecturer"],
            Convert.ToBase64String([1, 2, 3, 4]));
        var import = new IdentityImportBatchDto(
            importId,
            "demo-identity-import.json",
            new string('A', 64),
            "validated",
            Convert.ToBase64String([5, 6, 7, 8]),
            []);
        var handler = new RecordingHandler(request =>
        {
            var path = request.RequestUri!.AbsolutePath;
            if (request.Method == HttpMethod.Get && path == "/api/admin/users")
            {
                return Json(new Page<IdentityUserSummaryDto>(
                    [current],
                    pageNumber: 1,
                    pageSize: 20,
                    totalCount: 1,
                    sort: "displayName,id"));
            }

            if (request.Method == HttpMethod.Patch && path.EndsWith("/status", StringComparison.Ordinal))
            {
                current = current with
                {
                    Enabled = false,
                    RowVersion = Convert.ToBase64String([9, 10, 11, 12])
                };
                return Json(current);
            }

            if (request.Method == HttpMethod.Put && path.EndsWith("/roles", StringComparison.Ordinal))
            {
                current = current with
                {
                    Roles = ["Admin", "Lecturer"],
                    RowVersion = Convert.ToBase64String([13, 14, 15, 16])
                };
                return Json(current);
            }

            if (request.Method == HttpMethod.Post && path == "/api/admin/users/imports")
            {
                return Json(import);
            }

            if (request.Method == HttpMethod.Post && path.EndsWith("/publish", StringComparison.Ordinal))
            {
                import = import with
                {
                    State = "published",
                    RowVersion = Convert.ToBase64String([17, 18, 19, 20])
                };
                return Json(import);
            }

            throw new InvalidOperationException($"Unexpected identity request: {request.Method} {path}");
        });
        using var context = CreateContext(handler);
        var page = context.Render<UserAdministrationPage>();
        page.WaitForAssertion(() => Assert.Contains("Demo Lecturer", page.Markup, StringComparison.Ordinal));

        page.FindAll("button")
            .First(button => button.TextContent.Trim() == "Review")
            .Click();
        page.Find("#status-change-reason").Input("Demo account lifecycle review");
        page.FindAll("button")
            .Single(button => button.TextContent.Contains("Disable account", StringComparison.Ordinal))
            .Click();
        page.WaitForElement("#status-change-dialog button[data-action=confirm]").Click();
        page.WaitForAssertion(() => Assert.Contains("Account status updated", page.Markup, StringComparison.Ordinal));

        var adminCheckbox = page.FindAll("input[type=checkbox]")
            .Single(input => input.ParentElement!.TextContent.Contains("Admin", StringComparison.Ordinal));
        adminCheckbox.Change(true);
        page.Find("#role-change-reason").Input("Add demo administration responsibility");
        page.FindAll("form.srs-admin-users__editor").Single().Submit();
        page.WaitForAssertion(() => Assert.Contains("Roles updated", page.Markup, StringComparison.Ordinal));

        page.FindAll("button")
            .Single(button => button.TextContent.Contains("Preview rows", StringComparison.Ordinal))
            .Click();
        page.WaitForElement(".srs-admin-users__import-preview");
        page.FindAll("button")
            .Single(button => button.TextContent.Contains("Submit import for validation", StringComparison.Ordinal))
            .Click();
        page.WaitForAssertion(() => Assert.Contains("Import status: validated", page.Markup, StringComparison.Ordinal));
        page.FindAll("button")
            .Single(button => button.TextContent.Contains("Publish all rows", StringComparison.Ordinal))
            .Click();
        page.WaitForAssertion(() => Assert.Contains("Import published", page.Markup, StringComparison.Ordinal));

        var commands = handler.Requests
            .Where(request => request.Method != HttpMethod.Get)
            .ToArray();
        Assert.Equal(
            [
                HttpMethod.Patch,
                HttpMethod.Put,
                HttpMethod.Post,
                HttpMethod.Post
            ],
            commands.Select(request => request.Method).ToArray());
        Assert.Equal(
            [
                $"/api/admin/users/{userId}/status",
                $"/api/admin/users/{userId}/roles",
                "/api/admin/users/imports",
                $"/api/admin/users/imports/{importId}/publish"
            ],
            commands.Select(request => request.Uri.AbsolutePath).ToArray());
        Assert.All(commands, request => Assert.Equal("test-xsrf", Assert.Single(request.AntiforgeryValues)));
        Assert.Contains("Demo account lifecycle review", commands[0].Body, StringComparison.Ordinal);
        Assert.Contains("Add demo administration responsibility", commands[1].Body, StringComparison.Ordinal);
        Assert.DoesNotContain("password", commands[2].Body, StringComparison.OrdinalIgnoreCase);
    }

    private static BunitContext CreateContext(RecordingHandler handler)
    {
        var context = new BunitContext();
        context.JSInterop
            .Setup<string>("StudentRegistration.antiforgery.getRequestToken", _ => true)
            .SetResult("test-xsrf");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://student-registration.test/")
        };
        context.Services.AddSingleton(httpClient);
        context.Services.AddSingleton<IdentityApiClient>();
        return context;
    }

    private static HttpResponseMessage Json<T>(T value) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    private static string CreateSecret() =>
        string.Concat("Demo!", Guid.NewGuid().ToString("N"));

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.TryGetValues("X-XSRF-TOKEN", out var values)
                    ? values.ToArray()
                    : [],
                body));
            return responder(request);
        }
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri Uri,
        IReadOnlyList<string> AntiforgeryValues,
        string Body);
}
