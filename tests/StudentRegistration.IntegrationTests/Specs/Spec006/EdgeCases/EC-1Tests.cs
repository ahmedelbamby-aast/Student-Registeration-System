using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;

namespace StudentRegistration.IntegrationTests.Specs.Spec006.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public async Task Malformed_json_returns_400_validation_error_before_command_execution()
    {
        await using var application =
            await Spec006HttpTestApplication.CreateBodyBindingApplicationAsync();
        using var client = application.GetTestClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/admin/terms")
        {
            Content = new StringContent(
                """{"universityId":"202600001","password":""",
                Encoding.UTF8,
                "application/json")
        };
        await Spec006HttpTestApplication.AddAntiforgeryTokenAsync(client, request);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("VALIDATION_ERROR", body.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            body.RootElement.GetProperty("correlationId").GetString()));
        Assert.DoesNotContain(
            "AdminAcademicManagementService",
            await response.Content.ReadAsStringAsync());
    }
}
