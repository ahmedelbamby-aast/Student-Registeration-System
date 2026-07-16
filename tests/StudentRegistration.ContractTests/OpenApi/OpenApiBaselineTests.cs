using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Academics.Endpoints;
using StudentRegistration.Api.Composition;
using StudentRegistration.Api.Endpoints;
using StudentRegistration.IdentityAccess.Endpoints;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.OpenApi;

public sealed class OpenApiBaselineTests
{
    private const string BaselinePath =
        "specs/006-domain-class-api-contracts/contracts/openapi/student-registration-v1.json";

    [Fact]
    public async Task Real_application_surface_is_generated_deterministically()
    {
        var first = await GenerateAsync();
        var second = await GenerateAsync();

        Assert.Equal(Canonicalize(first), Canonicalize(second));
        using var document = JsonDocument.Parse(first);
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.EnumerateObject().Count() >= 20);
        Assert.True(paths.TryGetProperty("/api/auth/student/login", out _));
        Assert.True(paths.TryGetProperty("/api/context", out _));
        Assert.True(paths.TryGetProperty("/api/admin/terms", out _));
        Assert.True(paths.TryGetProperty("/api/health", out _));
        Assert.False(paths.GetProperty("/api/public/context")
            .GetProperty("get").TryGetProperty("security", out _));
        Assert.True(paths.GetProperty("/api/context")
            .GetProperty("get").TryGetProperty("security", out _));
    }

    [Fact]
    public async Task Generated_surface_matches_the_approved_semantic_baseline()
    {
        var generated = await GenerateAsync();
        if (string.Equals(
                Environment.GetEnvironmentVariable("SRS_UPDATE_OPENAPI_BASELINE"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            var baselineFile = RepositoryFiles.PathTo(BaselinePath);
            Directory.CreateDirectory(Path.GetDirectoryName(baselineFile)!);
            await File.WriteAllTextAsync(
                baselineFile,
                PrettyPrint(generated) + Environment.NewLine);
        }

        Assert.True(
            RepositoryFiles.Exists(BaselinePath),
            $"Generate and approve the real OpenAPI baseline at {BaselinePath}.");
        var baseline = RepositoryFiles.Read(BaselinePath);

        Assert.Equal(Canonicalize(baseline), Canonicalize(generated));
    }

    [Fact]
    public void Semantic_comparison_ignores_formatting_and_property_order_but_rejects_drift()
    {
        const string left =
            """{"paths":{"/api/context":{"get":{"responses":{"200":{},"403":{}}}}},"openapi":"3.1.0"}""";
        const string reordered =
            """
            {
              "openapi": "3.1.0",
              "paths": {
                "/api/context": {
                  "get": {
                    "responses": {
                      "403": {},
                      "200": {}
                    }
                  }
                }
              }
            }
            """;
        const string drifted =
            """{"openapi":"3.1.0","paths":{"/api/context":{"get":{"responses":{"200":{},"401":{}}}}}}""";

        Assert.Equal(Canonicalize(left), Canonicalize(reordered));
        Assert.NotEqual(Canonicalize(left), Canonicalize(drifted));
    }

    internal static string Canonicalize(string json)
    {
        var node = JsonNode.Parse(json)
            ?? throw new JsonException("The OpenAPI document is empty.");
        return Sort(node).ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private static string PrettyPrint(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(
            document.RootElement,
            new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonNode Sort(JsonNode node) =>
        node switch
        {
            JsonObject value => new JsonObject(
                value.OrderBy(property => property.Key, StringComparer.Ordinal)
                    .Select(property =>
                        KeyValuePair.Create(
                            property.Key,
                            property.Value is null ? null : Sort(property.Value)))),
            JsonArray value => new JsonArray(
                value.Select(item => item is null ? null : Sort(item)).ToArray()),
            _ => node.DeepClone()
        };

    private static async Task<string> GenerateAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddStudentRegistrationJsonContracts();
        builder.Services.AddStudentRegistrationOpenApi();
        builder.Services.AddAuthorization(options =>
        {
            foreach (var policyName in new[]
            {
                "Context.Read",
                "AcademicProfile.ReadOwn",
                "AcademicTerms.Manage",
                "AcademicProfiles.Manage",
                "StaffContext",
                "IdentityManagement"
            })
            {
                options.AddPolicy(
                    policyName,
                    policy => policy.RequireAuthenticatedUser());
            }
        });

        await using var application = builder.Build();
        application.MapSpec018Endpoints();
        application.MapSpec007Endpoints();
        application.MapSpec008Endpoints();
        application.MapOpenApi().AllowAnonymous();
        await application.StartAsync();

        using var response = await application.GetTestClient()
            .GetAsync($"/openapi/{OpenApiRegistration.DocumentName}.json");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
