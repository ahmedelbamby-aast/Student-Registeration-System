using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;

namespace StudentRegistration.IntegrationTests.Specs.Spec006.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public async Task Unsupported_media_type_returns_415_before_command_execution()
    {
        await using var application =
            await Spec006HttpTestApplication.CreateBodyBindingApplicationAsync();
        using var client = application.GetTestClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/admin/terms")
        {
            Content = new StringContent(
                "universityId=202600001&password=not-json",
                Encoding.UTF8,
                "text/plain")
        };
        await Spec006HttpTestApplication.AddAntiforgeryTokenAsync(client, request);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "UNSUPPORTED_MEDIA_TYPE",
            body.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            body.RootElement.GetProperty("correlationId").GetString()));
    }
}
