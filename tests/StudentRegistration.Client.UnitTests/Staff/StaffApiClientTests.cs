using System.Net;
using System.Text;
using Microsoft.JSInterop;
using StudentRegistration.Client.Features.Staff;
using StudentRegistration.Contracts.Staff;

namespace StudentRegistration.Client.UnitTests.Staff;

public sealed class StaffApiClientTests
{
    [Fact]
    public async Task Roster_request_is_bounded_and_never_sends_staff_or_role_scope()
    {
        HttpRequestMessage? captured = null;
        using var http = Client(request =>
        {
            captured = request;
            return Task.FromResult(Json(HttpStatusCode.OK, """
                {"items":[],"page":2,"pageSize":20,"totalCount":0,"sort":"displayName:asc,universityId:asc"}
                """));
        });
        var client = new StaffApiClient(http);

        var result = await client.GetRosterAsync(
            Guid.Parse("00000000-0000-0000-0000-000000016001"), page: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal("/api/staff/groups/00000000-0000-0000-0000-000000016001/roster?page=2&pageSize=20", captured!.RequestUri!.PathAndQuery);
        Assert.DoesNotContain("staffId", captured.RequestUri.PathAndQuery, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("role", captured.RequestUri.PathAndQuery, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Availability_put_sends_complete_contract_and_antiforgery_token()
    {
        string? body = null;
        HttpRequestMessage? captured = null;
        using var http = Client(async request =>
        {
            captured = request;
            body = await request.Content!.ReadAsStringAsync();
            return Json(HttpStatusCode.OK, """
                {"availability":{"id":"00000000-0000-0000-0000-000000016010","staffId":"00000000-0000-0000-0000-000000016030","termId":"00000000-0000-0000-0000-000000016050","deadlineUtc":"2026-07-21T18:00:00Z","rowVersion":"AV-RV-2","ranges":[]},"impactAlertIds":[]}
                """);
        });
        var client = new StaffApiClient(http, new TokenJavascriptRuntime());
        var request = new ReplaceAvailabilityRequest(
            "AV-RV-1",
            [new(Guid.Parse("00000000-0000-0000-0000-000000016011"), DayOfWeek.Monday, new(9, 0), new(12, 0), "available")]);

        var result = await client.ReplaceAvailabilityAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpMethod.Put, captured!.Method);
        Assert.Equal("TOKEN-016", captured.Headers.GetValues("X-XSRF-TOKEN").Single());
        Assert.Contains("expectedStaffTermRowVersion", body, StringComparison.Ordinal);
        Assert.Contains("\"kind\":\"available\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Safe_error_preserves_status_code_and_correlation_only()
    {
        using var http = Client(_ => Task.FromResult(Json(HttpStatusCode.NotFound,
            "{\"code\":\"STAFF_GROUP_NOT_FOUND\",\"message\":\"Not found.\",\"correlationId\":\"SAFE-016\"}")));
        var client = new StaffApiClient(http);

        var result = await client.GetRosterAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("STAFF_GROUP_NOT_FOUND", result.Error?.Code);
        Assert.Equal("SAFE-016", result.Error?.CorrelationId);
    }

    [Fact]
    public async Task Availability_conflict_returns_the_authorized_current_aggregate()
    {
        using var http = Client(_ => Task.FromResult(Json(HttpStatusCode.Conflict, """
            {"error":{"code":"STALE_VERSION","message":"Changed.","correlationId":"SAFE-016","currentVersion":"AV-RV-2"},
            "currentAvailability":{"id":"00000000-0000-0000-0000-000000016010","staffId":"00000000-0000-0000-0000-000000016030","termId":"00000000-0000-0000-0000-000000016050","deadlineUtc":"2026-07-21T18:00:00Z","rowVersion":"AV-RV-2","ranges":[]},
            "serverTimeUtc":"2026-07-20T07:15:00Z","deadlineUtc":"2026-07-21T18:00:00Z"}
            """)));
        var client = new StaffApiClient(http, new TokenJavascriptRuntime());

        var result = await client.ReplaceAvailabilityAsync(
            new ReplaceAvailabilityRequest("AV-RV-1", []));

        Assert.False(result.IsSuccess);
        Assert.Equal("STALE_VERSION", result.Error?.Code);
        Assert.Equal("AV-RV-2", result.Conflict?.CurrentAvailability.RowVersion);
    }

    private static HttpClient Client(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) =>
        new(new StubHandler(send)) { BaseAddress = new Uri("https://student-registration.test/") };

    private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }

    private sealed class TokenJavascriptRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult((TValue)(object)"TOKEN-016");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromResult((TValue)(object)"TOKEN-016");
    }
}
