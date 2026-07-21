using Microsoft.Playwright;
using StudentRegistration.E2ETests.Infrastructure;

namespace StudentRegistration.E2ETests.Routes;

[Collection(Spec008BrowserCollection.CollectionName)]
public sealed class ApprovalWorkspaceStateJourneyTests(Spec008BrowserFixture fixture)
{
    [Theory]
    [InlineData("/admin/approvals", "ADM-10", "Admin", "/api/admin/registration-approvals")]
    [InlineData("/staff/approvals", "STF-05", "Lecturer", "/api/staff/registration-approvals")]
    [InlineData("/staff/approvals", "STF-05", "TeachingAssistant", "/api/staff/registration-approvals")]
    public async Task Paged_pending_overload_projects_exact_boundary_and_held_capacity(
        string route, string routeId, string role, string apiPath)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", request => RequestedUnifiedRouteJourneyTests.Path(request) switch
        {
            "/api/context" => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, RequestedUnifiedRouteJourneyTests.Context(role)),
            var path when path == apiPath => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, PagedQueue),
            _ => request.AbortAsync()
        });

        await page.GotoAsync(route);
        var workspace = page.Locator($"[data-route-id='{routeId}'][data-state='success']");
        await workspace.WaitForAsync();
        await Assertions.Expect(workspace.GetByText("Overload request:", new() { Exact = false })).ToBeVisibleAsync();
        await Assertions.Expect(workspace).ToContainTextAsync("Overload request: 21 credits of maximum 21");
        await Assertions.Expect(workspace).ToContainTextAsync("Current CGPA: 3.00; required CGPA: 3.00");
        var capacity = workspace.GetByRole(AriaRole.Status, new() { Name = "Capacity for AI401" });
        await Assertions.Expect(capacity).ToContainTextAsync("Total");
        await Assertions.Expect(capacity).ToContainTextAsync("Enrolled");
        await Assertions.Expect(capacity).ToContainTextAsync("Held");
        await Assertions.Expect(capacity).ToContainTextAsync("Available");
        await Assertions.Expect(workspace.GetByText("21 pending subject decisions", new() { Exact = false })).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("/admin/approvals", "Admin", "/api/admin/registration-approvals", "Approve subject")]
    [InlineData("/staff/approvals", "Lecturer", "/api/staff/registration-approvals", "Reject plan")]
    [InlineData("/staff/approvals", "TeachingAssistant", "/api/staff/registration-approvals", "Approve subject")]
    public async Task Terminal_decision_refreshes_to_empty_without_false_pending_or_success(
        string route, string role, string apiPath, string action)
    {
        var decided = false;
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", request => RequestedUnifiedRouteJourneyTests.Path(request) switch
        {
            "/api/context" => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, RequestedUnifiedRouteJourneyTests.Context(role)),
            var path when path == apiPath => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, decided ? EmptyQueue : PagedQueue),
            var path when path.Contains("/decision", StringComparison.Ordinal) => DecideAsync(request),
            _ => request.AbortAsync()
        });

        await page.GotoAsync(route);
        await page.GetByTestId($"approval-reason-{LineId:D}").FillAsync("Reviewed against current capacity and scope.");
        await page.GetByRole(AriaRole.Button, new() { Name = action, Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByText("No pending decisions", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("Pending approval", new() { Exact = true })).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByText("Registration approved", new() { Exact = false })).ToHaveCountAsync(0);
        return;

        Task DecideAsync(IRoute request)
        {
            decided = true;
            return RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, "{}");
        }
    }

    [Theory]
    [InlineData("/admin/approvals", "Admin", "/api/admin/registration-approvals")]
    [InlineData("/staff/approvals", "Lecturer", "/api/staff/registration-approvals")]
    [InlineData("/staff/approvals", "TeachingAssistant", "/api/staff/registration-approvals")]
    public async Task Stale_decision_preserves_operable_focus_and_never_claims_success(
        string route, string role, string apiPath)
    {
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", request => RequestedUnifiedRouteJourneyTests.Path(request) switch
        {
            "/api/context" => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, RequestedUnifiedRouteJourneyTests.Context(role)),
            var path when path == apiPath => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, PagedQueue),
            var path when path.Contains("/decision", StringComparison.Ordinal) => RequestedUnifiedRouteJourneyTests.JsonAsync(request, 409, Stale),
            _ => request.AbortAsync()
        });

        await page.GotoAsync(route);
        var reason = page.GetByTestId($"approval-reason-{LineId:D}");
        await reason.FillAsync("Reviewed current assignment.");
        var approve = page.GetByRole(AriaRole.Button, new() { Name = "Approve subject", Exact = true });
        await approve.ClickAsync();
        await Assertions.Expect(page.GetByText("APPROVAL_STALE", new() { Exact = false })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("Pending approval", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("Registration approved", new() { Exact = false })).ToHaveCountAsync(0);
        Assert.True(await reason.EvaluateAsync<bool>("element => element === document.activeElement"));
    }

    [Fact]
    public async Task Rapid_double_action_emits_one_decision_while_the_shared_command_is_busy()
    {
        var decisionRequests = 0;
        await using var context = await fixture.OpenContextAsync();
        var page = await context.NewPageAsync();
        await page.RouteAsync("**/api/**", async request =>
        {
            var path = RequestedUnifiedRouteJourneyTests.Path(request);
            if (path == "/api/context")
                await RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, RequestedUnifiedRouteJourneyTests.Context("Admin"));
            else if (path == "/api/admin/registration-approvals")
                await RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, PagedQueue);
            else if (path.Contains("/decision", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref decisionRequests);
                await Task.Delay(250);
                await RequestedUnifiedRouteJourneyTests.JsonAsync(request, 200, "{}");
            }
            else await request.AbortAsync();
        });

        await page.GotoAsync("/admin/approvals");
        await page.GetByTestId($"approval-reason-{LineId:D}").FillAsync("Reviewed once.");
        var approve = page.GetByRole(AriaRole.Button, new() { Name = "Approve subject", Exact = true });
        var first = approve.ClickAsync();
        await Assertions.Expect(approve).ToBeDisabledAsync();
        await approve.ClickAsync(new() { Force = true });
        await first;
        Assert.Equal(1, decisionRequests);
    }

    private static readonly Guid LineId = Guid.Parse("00000000-0000-0000-0000-000000014002");
    private const string EmptyQueue = """{"items":[],"page":1,"pageSize":20,"totalCount":0}""";
    private const string Stale = """{"code":"APPROVAL_STALE","message":"Refresh the queue before deciding again.","correlationId":"BROWSER-STALE"}""";
    private const string PagedQueue = """
        {"items":[{"submissionId":"00000000-0000-0000-0000-000000014001",
        "line":{"lineId":"00000000-0000-0000-0000-000000014002","offeringId":"00000000-0000-0000-0000-000000014003","groupId":"00000000-0000-0000-0000-000000014004","courseCode":"AI401","subjectTitle":"Responsible AI","credits":3,"state":"pendingApproval","capacity":{"capacity":30,"enrolledCount":27,"heldSeatCount":2,"availableSeatCount":1},"rowVersion":"LINE-RV-1"},
        "studentUniversityId":"AI2600001","studentDisplayName":"Synthetic Student","requestedCredits":21,"currentCgpa":3.00,"overload":true,"submittedAtUtc":"2026-07-21T09:00:00Z","windowClosesAtUtc":"2026-07-22T18:00:00Z","submissionVersion":"SUB-RV-1"}],"page":2,"pageSize":1,"totalCount":21}
        """;
}
