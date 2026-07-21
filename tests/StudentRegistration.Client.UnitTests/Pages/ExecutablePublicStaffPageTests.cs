using System.Net;
using System.Text;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Client;
using StudentRegistration.Client.Components.Approvals;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Features.Operations;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.Client.Features.Staff;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class ExecutablePublicPageRenderTests
{
    [Theory]
    [InlineData("RoleGatewayPage", "main#main-content")]
    [InlineData("StudentLoginPage", "form[data-testid='student-login-form']")]
    [InlineData("StudentActivationPage", "form[data-testid='student-activation-form']")]
    [InlineData("StaffLoginPage", "form[data-testid='staff-login-form']")]
    [InlineData("AccountRecoveryPage", "form[data-testid='recovery-request-form']")]
    [InlineData("SystemStatusPage", "[data-route-id='SYS-01']")]
    public void Auth_and_system_routes_render_the_actual_page_with_stable_landmarks(
        string pageName,
        string landmark)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Pending);
        var cut = PublicStaffPageHarness.RenderPage(context, pageName);
        Assert.NotNull(cut.Find(landmark));
    }

    [Theory]
    [InlineData("StudentLoginPage", "form[data-testid='student-login-form']", "Review the sign-in details")]
    [InlineData("StudentActivationPage", "form[data-testid='student-activation-form']", "Review the activation details")]
    [InlineData("StaffLoginPage", "form[data-testid='staff-login-form']", "Review the sign-in details")]
    [InlineData("AccountRecoveryPage", "form[data-testid='recovery-request-form']", "Review the recovery request")]
    public void Auth_forms_execute_real_local_validation(
        string pageName,
        string formSelector,
        string expectedSummary)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Failure);
        var cut = PublicStaffPageHarness.RenderPage(context, pageName);
        cut.Find(formSelector).Submit();
        cut.WaitForAssertion(() => Assert.Contains(expectedSummary, cut.Markup, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("StudentLoginPage", "#student-university-id", "S260001", "#student-password", "ValidPass!1", "/student")]
    [InlineData("StaffLoginPage", "#staff-user-name", "LEC-0001", "#staff-password", "ValidPass!1", "/staff")]
    public void Login_success_uses_the_server_single_role_and_navigates(
        string pageName,
        string identitySelector,
        string identity,
        string passwordSelector,
        string password,
        string expectedPath)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Success);
        var cut = PublicStaffPageHarness.RenderPage(context, pageName);
        cut.Find(identitySelector).Input(identity);
        cut.Find(passwordSelector).Input(password);
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.EndsWith(expectedPath, context.Services.GetRequiredService<NavigationManager>().Uri, StringComparison.Ordinal));
    }

    [Fact]
    public void Activation_success_operates_the_real_form_and_returns_to_student_login()
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Success);
        var cut = PublicStaffPageHarness.RenderPage(context, "StudentActivationPage");
        cut.Find("#activation-university-id").Input("S260001");
        cut.Find("#activation-initial-password").Input("Initial!1");
        cut.Find("#activation-new-password").Input("NewValidPass!123");
        cut.Find("#activation-confirm-password").Input("NewValidPass!123");
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => Assert.EndsWith("/student/login", context.Services.GetRequiredService<NavigationManager>().Uri, StringComparison.Ordinal));
    }

    [Fact]
    public void Recovery_success_operates_the_real_request_form_without_disclosing_identity()
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Success);
        var cut = PublicStaffPageHarness.RenderPage(context, "AccountRecoveryPage");
        cut.Find("#recovery-identifier").Input("S260001");
        cut.Find("form[data-testid='recovery-request-form']").Submit();
        cut.WaitForAssertion(() => Assert.Contains("Check your recovery channel", cut.Markup, StringComparison.Ordinal));
    }

    [Fact]
    public void Public_gateway_renders_success_failure_and_operable_destination()
    {
        using var success = PublicStaffPageHarness.Create(ResponseMode.Success);
        var available = PublicStaffPageHarness.RenderPage(success, "RoleGatewayPage");
        available.WaitForAssertion(() => Assert.NotNull(available.Find("[data-testid='student-login-link']")));
        available.Find("[data-testid='student-login-link']").Click();

        using var failure = PublicStaffPageHarness.Create(ResponseMode.Failure);
        var unavailable = PublicStaffPageHarness.RenderPage(failure, "RoleGatewayPage");
        unavailable.WaitForAssertion(() => Assert.Contains("SERVICE_UNAVAILABLE", unavailable.Markup, StringComparison.Ordinal));
    }

    [Fact]
    public void System_status_renders_success_failure_and_operable_retry()
    {
        using var success = PublicStaffPageHarness.Create(ResponseMode.Success);
        var healthy = PublicStaffPageHarness.RenderPage(success, "SystemStatusPage",
            new Dictionary<string, object?> { ["Code"] = "healthy" });
        healthy.WaitForAssertion(() => Assert.Contains("Systems available", healthy.Markup, StringComparison.Ordinal));

        using var failure = PublicStaffPageHarness.Create(ResponseMode.Failure);
        var unavailable = PublicStaffPageHarness.RenderPage(failure, "SystemStatusPage",
            new Dictionary<string, object?> { ["Code"] = "healthy" });
        unavailable.WaitForAssertion(() => Assert.Contains("Service unavailable", unavailable.Markup, StringComparison.Ordinal));
        Assert.Equal("/status/healthy", unavailable.Find("a[data-action-id='retry']").GetAttribute("href"));
        var pageType = typeof(App).Assembly.GetType("StudentRegistration.Client.Pages.SystemStatusPage")!;
        unavailable.Render(parameters => parameters
            .Add(component => component.Type, pageType)
            .Add(component => component.Parameters,
                new Dictionary<string, object> { ["Code"] = "404" }));
        Assert.Contains("Page not found", unavailable.Markup, StringComparison.Ordinal);
    }
}

public sealed class ExecutableStaffPageRenderTests
{
    [Theory]
    [InlineData("StaffDashboardPage", "Loading authorized assignments")]
    [InlineData("StaffTimetablePage", "Loading authorized timetable")]
    [InlineData("StaffRosterPage", "Loading the authorized roster")]
    [InlineData("StaffAvailabilityPage", "Loading owned availability")]
    public void Staff_routes_render_deterministic_pending_state(string pageName, string pendingText)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Pending);
        var parameters = pageName == "StaffRosterPage"
            ? new Dictionary<string, object?> { ["GroupId"] = PublicStaffPageHarness.GroupId }
            : null;
        var cut = PublicStaffPageHarness.RenderPage(context, pageName, parameters);
        Assert.Contains(pendingText, cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("StaffDashboardPage", "STF-01")]
    [InlineData("StaffTimetablePage", "STF-02")]
    [InlineData("StaffRosterPage", "STF-03")]
    [InlineData("StaffAvailabilityPage", "STF-04")]
    public void Staff_routes_render_success_with_single_lecturer_context_and_operable_navigation(
        string pageName,
        string routeId)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Success);
        var parameters = pageName == "StaffRosterPage"
            ? new Dictionary<string, object?> { ["GroupId"] = PublicStaffPageHarness.GroupId }
            : null;
        var cut = PublicStaffPageHarness.RenderPage(context, pageName, parameters);
        cut.WaitForAssertion(() => Assert.NotNull(cut.Find($"[data-route-id='{routeId}']")));
        cut.FindAll("a[href='/staff']").First().Click();
    }

    [Theory]
    [InlineData("StaffDashboardPage")]
    [InlineData("StaffTimetablePage")]
    [InlineData("StaffRosterPage")]
    [InlineData("StaffAvailabilityPage")]
    public void Staff_routes_render_authoritative_failure_without_role_selection(string pageName)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Failure);
        var parameters = pageName == "StaffRosterPage"
            ? new Dictionary<string, object?> { ["GroupId"] = PublicStaffPageHarness.GroupId }
            : null;
        var cut = PublicStaffPageHarness.RenderPage(context, pageName, parameters);
        cut.WaitForAssertion(() => Assert.Contains("SERVICE_UNAVAILABLE", cut.Markup, StringComparison.Ordinal));
        Assert.DoesNotContain("Switch context", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, "ADM-10", "Admin")]
    [InlineData(false, "STF-05", "TeachingAssistant")]
    public void Approval_workspace_renders_empty_queue_and_operates_refresh(
        bool admin,
        string routeId,
        string role)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Success, role);
        var cut = context.Render<ApprovalWorkspace>(parameters => parameters.Add(component => component.IsAdmin, admin));
        cut.WaitForAssertion(() => Assert.NotNull(cut.Find($"[data-route-id='{routeId}'][data-state='empty']")));
        cut.Find("button").Click();
        cut.WaitForAssertion(() => Assert.NotNull(cut.Find($"[data-route-id='{routeId}'][data-state='empty']")));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Approval_workspace_renders_deterministic_pending_state(bool admin)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Pending);
        var cut = context.Render<ApprovalWorkspace>(parameters => parameters.Add(component => component.IsAdmin, admin));
        Assert.Contains("Loading the authorized approval queue", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, "Admin")]
    [InlineData(false, "Lecturer")]
    [InlineData(false, "TeachingAssistant")]
    public void Approval_workspace_failure_preserves_separate_single_role_scope(bool admin, string role)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.Failure, role);
        var cut = context.Render<ApprovalWorkspace>(parameters => parameters.Add(component => component.IsAdmin, admin));
        cut.WaitForAssertion(() => Assert.Contains("SERVICE_UNAVAILABLE", cut.Markup, StringComparison.Ordinal));
        Assert.DoesNotContain("LecturerTeachingAssistant", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, "Admin", "ADM-10")]
    [InlineData(false, "Lecturer", "STF-05")]
    [InlineData(false, "TeachingAssistant", "STF-05")]
    public void Approval_workspace_projects_pending_overload_and_private_held_capacity(
        bool admin,
        string role,
        string routeId)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.ApprovalQueue, role);
        var cut = context.Render<ApprovalWorkspace>(parameters => parameters.Add(component => component.IsAdmin, admin));

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find($"[data-route-id='{routeId}'][data-state='success']")));
        Assert.Contains("Overload request: 21 credits of maximum 21", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Approval is required and is not implied", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("3.00", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("21", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Total", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Enrolled", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Held", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Available", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("seat holder", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(true, "Lecturer")]
    [InlineData(false, "Student")]
    public void Approval_workspace_denies_a_role_mismatch_before_loading_the_queue(bool admin, string role)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.ApprovalQueue, role);
        var cut = context.Render<ApprovalWorkspace>(parameters => parameters.Add(component => component.IsAdmin, admin));

        cut.WaitForAssertion(() => Assert.Contains("Approval queue unavailable", cut.Markup, StringComparison.Ordinal));
        Assert.DoesNotContain("AI401", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, "Admin")]
    [InlineData(false, "Lecturer")]
    [InlineData(false, "TeachingAssistant")]
    public void Approval_workspace_keeps_stale_decision_as_an_error_and_never_claims_success(bool admin, string role)
    {
        using var context = PublicStaffPageHarness.Create(ResponseMode.StaleDecision, role);
        var cut = context.Render<ApprovalWorkspace>(parameters => parameters.Add(component => component.IsAdmin, admin));
        cut.WaitForAssertion(() => Assert.Contains("AI401", cut.Markup, StringComparison.Ordinal));

        cut.Find("textarea").Input("Reviewed against the current assignment and capacity.");
        cut.FindAll("button").Single(button => button.TextContent.Contains("Approve subject", StringComparison.Ordinal)).Click();

        cut.WaitForAssertion(() => Assert.Contains("APPROVAL_STALE", cut.Markup, StringComparison.Ordinal));
        Assert.Contains("Refresh the queue", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Registration approved", cut.Markup, StringComparison.Ordinal);
    }
}

internal enum ResponseMode { Pending, Success, Failure, ApprovalQueue, StaleDecision }

internal static class PublicStaffPageHarness
{
    internal static readonly Guid GroupId = Guid.Parse("00000000-0000-0000-0000-000000016001");

    internal static BunitContext Create(ResponseMode mode, string role = "Lecturer")
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var http = new HttpClient(new RouteHandler(mode, role)) { BaseAddress = new Uri("https://component.test/") };
        context.Services.AddSingleton(http);
        context.Services.AddSingleton(new AcademicApiClient(http, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new IdentityApiClient(http, context.JSInterop.JSRuntime));
        context.Services.AddSingleton(new OperationsApiClient(http));
        context.Services.AddSingleton(new StaffApiClient(http));
        context.Services.AddSingleton(new RegistrationApprovalApiClient(http, context.JSInterop.JSRuntime));
        return context;
    }

    internal static IRenderedComponent<DynamicComponent> RenderPage(
        BunitContext context,
        string pageName,
        IReadOnlyDictionary<string, object?>? pageParameters = null)
    {
        var type = typeof(App).Assembly.GetType($"StudentRegistration.Client.Pages.{pageName}");
        Assert.NotNull(type);
        IDictionary<string, object> dynamicParameters = pageParameters is null
            ? new Dictionary<string, object>()
            : pageParameters.ToDictionary(pair => pair.Key, pair => pair.Value!);
        return context.Render<DynamicComponent>(parameters => parameters
            .Add(component => component.Type, type!)
            .Add(component => component.Parameters, dynamicParameters));
    }

    private sealed class RouteHandler(ResponseMode mode, string role) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (mode == ResponseMode.Pending)
                return new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously).Task;

            var path = request.RequestUri!.AbsolutePath;
            if (mode == ResponseMode.Failure && path != "/api/context")
                return Task.FromResult(Json(HttpStatusCode.ServiceUnavailable, Error));

            var body = path switch
            {
                "/api/context" => Context(role),
                "/api/staff/assignments" => "[]",
                "/api/staff/timetable" => $$"""{"roleContext":"{{role}}","assignments":[]}""",
                var value when value.Contains("/roster", StringComparison.Ordinal) => """{"items":[],"page":1,"pageSize":20,"totalCount":0,"sort":"displayName:asc,universityId:asc"}""",
                "/api/staff/availability" => Availability,
                "/api/admin/registration-approvals" or "/api/staff/registration-approvals" =>
                    mode is ResponseMode.ApprovalQueue or ResponseMode.StaleDecision ? ApprovalQueue : EmptyApprovalQueue,
                var value when value.Contains("/decision", StringComparison.Ordinal) => mode == ResponseMode.StaleDecision ? StaleDecision : "{}",
                "/api/health" => """{"status":"healthy","version":"unit-test","timestampUtc":"2026-07-21T09:30:00Z"}""",
                "/api/public/context" => PublicContext,
                "/api/auth/student/login" => Session("Student"),
                "/api/auth/student/activate" => Session("Student"),
                "/api/auth/staff/login" => Session(role),
                "/api/auth/recovery/request" => string.Empty,
                _ => Error
            };
            var status = path == "/api/auth/recovery/request"
                ? HttpStatusCode.Accepted
                : body == Error ? HttpStatusCode.ServiceUnavailable
                : body == StaleDecision ? HttpStatusCode.Conflict
                : HttpStatusCode.OK;
            return Task.FromResult(Json(status, body));
        }
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private static string Context(string role) => $$"""{"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,"registrationTerm":null,"registrationWindowState":"none","registrationWindow":null,"serviceState":"available","displayName":"{{role}} Demo","authorizedRoles":["{{role}}"],"activeRole":"{{role}}","sessionState":"active","expiresAtUtc":"2026-07-21T11:30:00Z","supportReferencePath":"/status/support"}""";
    private static string Session(string role) => $$"""{"displayName":"{{role}} Demo","roles":["{{role}}"],"activeRole":"{{role}}","sessionState":"active","expiresAtUtc":"2026-07-21T11:30:00Z"}""";
    private const string Availability = """{"id":"00000000-0000-0000-0000-000000016010","staffId":"00000000-0000-0000-0000-000000016030","termId":"00000000-0000-0000-0000-000000016050","deadlineUtc":"2026-07-22T18:00:00Z","rowVersion":"AV-RV-1","ranges":[]}""";
    private const string PublicContext = """{"serverTimeUtc":"2026-07-21T09:30:00Z","timeZoneId":"Africa/Cairo","teachingTermLabel":null,"registrationTermLabel":null,"registrationWindowState":"none","serviceState":"available"}""";
    private const string EmptyApprovalQueue = """{"items":[],"page":1,"pageSize":20,"totalCount":0}""";
    private const string ApprovalQueue = """
        {"items":[{"submissionId":"00000000-0000-0000-0000-000000014001",
        "line":{"lineId":"00000000-0000-0000-0000-000000014002","offeringId":"00000000-0000-0000-0000-000000014003","groupId":"00000000-0000-0000-0000-000000014004","courseCode":"AI401","subjectTitle":"Responsible AI","credits":3,"state":"pendingApproval","capacity":{"capacity":30,"enrolledCount":27,"heldSeatCount":2,"availableSeatCount":1},"rowVersion":"LINE-RV-1"},
        "studentUniversityId":"AI2600001","studentDisplayName":"Synthetic Student","requestedCredits":21,"currentCgpa":3.00,"overload":true,"submittedAtUtc":"2026-07-21T09:00:00Z","windowClosesAtUtc":"2026-07-22T18:00:00Z","submissionVersion":"SUB-RV-1"}],"page":1,"pageSize":20,"totalCount":1}
        """;
    private const string StaleDecision = """{"code":"APPROVAL_STALE","message":"Refresh the queue before deciding again.","correlationId":"UNIT-STALE"}""";
    private const string Error = """{"code":"SERVICE_UNAVAILABLE","message":"The service is unavailable.","correlationId":"UNIT-SAFE"}""";
}
