using System.Net;
using Microsoft.AspNetCore.TestHost;

namespace StudentRegistration.IntegrationTests.Specs.Spec006.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public async Task Unauthenticated_protected_request_returns_401_without_resource_disclosure()
    {
        await using var application =
            await Spec006HttpTestApplication.CreateAuthorizationApplicationAsync(
                authenticated: false,
                permitted: false);
        using var response = await application.GetTestClient()
            .GetAsync("/api/context");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.False(response.Headers.Contains("ETag"));
    }

    [Fact]
    public async Task Authenticated_but_unpermitted_request_returns_403_without_resource_disclosure()
    {
        await using var application =
            await Spec006HttpTestApplication.CreateAuthorizationApplicationAsync(
                authenticated: true,
                permitted: false);
        using var response = await application.GetTestClient()
            .GetAsync("/api/context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.False(response.Headers.Contains("ETag"));
    }
}
