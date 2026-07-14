using System.Net;
using System.Text;
using Microsoft.JSInterop;
using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.UnitTests.Identity;

public sealed class IdentityApiClientTests
{
    [Theory]
    [InlineData("text/html", "<!doctype html><html></html>")]
    [InlineData("application/json", "{not-json}")]
    [InlineData("application/json", "{}")]
    [InlineData(
        "application/json",
        "{\"displayName\":null,\"roles\":null,\"activeRole\":null,\"sessionState\":null,\"expiresAtUtc\":\"2026-07-14T00:00:00Z\"}")]
    public async Task Successful_non_contract_session_response_fails_safely(
        string mediaType,
        string content)
    {
        using var httpClient = CreateClient(mediaType, content);
        var client = new IdentityApiClient(httpClient, new UnusedJavascriptRuntime());

        var result = await client.GetSessionAsync();

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Theory]
    [InlineData("text/html", "<!doctype html><html></html>")]
    [InlineData("application/json", "{}")]
    public async Task Successful_non_contract_admin_response_fails_safely(
        string mediaType,
        string content)
    {
        using var httpClient = CreateClient(mediaType, content);
        var client = new IdentityApiClient(httpClient, new UnusedJavascriptRuntime());

        var result = await client.ListAdminUsersAsync(null, page: 1);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    private static HttpClient CreateClient(string mediaType, string content) =>
        new(new StubHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, mediaType)
            }))
        {
            BaseAddress = new Uri("https://student-registration.test/")
        };

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(response);
    }

    private sealed class UnusedJavascriptRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            object?[]? args) =>
            throw new InvalidOperationException("GET requests must not invoke JavaScript.");

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args) =>
            throw new InvalidOperationException("GET requests must not invoke JavaScript.");
    }
}
