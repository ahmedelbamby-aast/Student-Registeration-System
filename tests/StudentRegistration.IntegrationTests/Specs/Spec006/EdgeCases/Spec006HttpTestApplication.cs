using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentRegistration.Academics.Endpoints;
using StudentRegistration.Api.Composition;

namespace StudentRegistration.IntegrationTests.Specs.Spec006.EdgeCases;

internal static class Spec006HttpTestApplication
{
    private const string AntiforgeryHeaderName = "X-SPEC006-CSRF";

    internal static async Task<WebApplication> CreateBodyBindingApplicationAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddSafeApiErrors();
        AddTestAuthentication(
            builder.Services,
            authenticated: true,
            permitted: true);
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(
                "AcademicTerms.Manage",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("permission", "AcademicTerms.Manage");
                }));
        builder.Services.AddAntiforgery(options =>
            options.HeaderName = AntiforgeryHeaderName);

        var application = builder.Build();
        application.UseSafeApiErrors();
        application.UseRouting();
        application.UseAuthentication();
        application.UseAuthorization();
        application.UseAntiforgery();
        application.MapGet(
                "/test/spec006/antiforgery",
                (HttpContext context, IAntiforgery antiforgery) =>
                    Results.Ok(antiforgery.GetAndStoreTokens(context).RequestToken))
            .AllowAnonymous();
        application.MapSpec008Endpoints();
        await application.StartAsync();
        return application;
    }

    internal static async Task AddAntiforgeryTokenAsync(
        HttpClient client,
        HttpRequestMessage request)
    {
        using var tokenResponse =
            await client.GetAsync("/test/spec006/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<string>();
        var cookie = tokenResponse.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0])
            .Single();

        request.Headers.Add(AntiforgeryHeaderName, token);
        request.Headers.Add("Cookie", cookie);
    }

    internal static async Task<WebApplication> CreateAuthorizationApplicationAsync(
        bool authenticated,
        bool permitted)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.UseTestServer();
        AddTestAuthentication(builder.Services, authenticated, permitted);
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(
                "Context.Read",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("permission", "Context.Read");
                }));

        var application = builder.Build();
        application.UseRouting();
        application.UseAuthentication();
        application.UseAuthorization();
        application.MapSpec008Endpoints();
        await application.StartAsync();
        return application;
    }

    private static void AddTestAuthentication(
        IServiceCollection services,
        bool authenticated,
        bool permitted)
    {
        services
            .AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                options =>
                {
                    options.ClaimsIssuer = authenticated ? "authenticated" : "anonymous";
                    options.TimeProvider = TimeProvider.System;
                });
        services.AddSingleton(new TestAuthorizationState(permitted));
    }

    internal sealed record TestAuthorizationState(bool Permitted);

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestAuthorizationState state)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        internal const string SchemeName = "SPEC006-Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!string.Equals(
                    Options.ClaimsIssuer,
                    "authenticated",
                    StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString("D"))
            };
            if (state.Permitted)
            {
                claims.Add(new Claim("permission", "Context.Read"));
                claims.Add(new Claim("permission", "AcademicTerms.Manage"));
            }

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, SchemeName));
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(principal, SchemeName)));
        }
    }
}
