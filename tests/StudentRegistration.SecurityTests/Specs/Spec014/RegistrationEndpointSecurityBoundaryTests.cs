using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Api.Composition;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Endpoints;

namespace StudentRegistration.SecurityTests.Specs.Spec014;

public sealed class RegistrationEndpointSecurityBoundaryTests
{
    private const string AuthenticationScheme = "SPEC014-Test";
    private const string AntiforgeryHeader = "X-SPEC014-XSRF";

    [Fact]
    public async Task Post_requires_exact_student_permission_and_antiforgery_before_handler()
    {
        var store = new TrackingEndpointStore();
        await using var application = await CreateApplicationAsync(store);
        using var client = application.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
        var userId = Guid.NewGuid();
        var antiforgery = await GetAntiforgeryAsync(client, userId);

        using var missingPermission = Post(
            RolePolicies.Student,
            permission: null,
            userId: userId);
        AddAntiforgery(missingPermission, antiforgery);
        using var missingPermissionResponse = await client.SendAsync(missingPermission);
        Assert.Equal(HttpStatusCode.Forbidden, missingPermissionResponse.StatusCode);
        Assert.Equal(0, store.ResolveCalls);

        using var wrongRole = Post(
            RolePolicies.Admin,
            RolePolicies.RegistrationSubmitOwn,
            userId);
        AddAntiforgery(wrongRole, antiforgery);
        using var wrongRoleResponse = await client.SendAsync(wrongRole);
        Assert.Equal(HttpStatusCode.Forbidden, wrongRoleResponse.StatusCode);
        Assert.Equal(0, store.ResolveCalls);

        using var missingAntiforgery = Post(
            RolePolicies.Student,
            RolePolicies.RegistrationSubmitOwn,
            userId);
        using var missingAntiforgeryResponse = await client.SendAsync(missingAntiforgery);
        Assert.Equal(HttpStatusCode.BadRequest, missingAntiforgeryResponse.StatusCode);
        var antiforgeryError = await missingAntiforgeryResponse.Content
            .ReadFromJsonAsync<StudentRegistration.Contracts.ApiError>();
        Assert.Equal("ANTIFORGERY_INVALID", antiforgeryError?.Code);
        Assert.Equal(0, store.ResolveCalls);

        using var allowed = Post(
            RolePolicies.Student,
            RolePolicies.RegistrationSubmitOwn,
            userId);
        AddAntiforgery(allowed, antiforgery);
        using var allowedResponse = await client.SendAsync(allowed);
        Assert.Equal(HttpStatusCode.Conflict, allowedResponse.StatusCode);
        Assert.Equal(1, store.ResolveCalls);
    }

    [Fact]
    public async Task Private_lookup_authorizes_before_owner_term_request_lookup()
    {
        var store = new TrackingEndpointStore();
        await using var application = await CreateApplicationAsync(store);
        using var client = application.GetTestClient();
        var termId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var path = $"/api/student/terms/{termId:D}/registrations/by-request/{requestId:D}";

        using var anonymousResponse = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        Assert.Equal(0, store.LookupCalls);

        using var wrongRole = Get(path, RolePolicies.Admin, RolePolicies.RegistrationSubmitOwn);
        using var wrongRoleResponse = await client.SendAsync(wrongRole);
        Assert.Equal(HttpStatusCode.Forbidden, wrongRoleResponse.StatusCode);
        Assert.Equal(0, store.LookupCalls);

        using var allowed = Get(
            path,
            RolePolicies.Student,
            RolePolicies.RegistrationSubmitOwn);
        using var allowedResponse = await client.SendAsync(allowed);
        Assert.Equal(HttpStatusCode.NotFound, allowedResponse.StatusCode);
        Assert.Equal(1, store.LookupCalls);
    }

    private static async Task<WebApplication> CreateApplicationAsync(
        TrackingEndpointStore store)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IRegistrationEndpointStore>(store);
        builder.Services.AddSingleton<IStudentAcademicProfileStore, UnusedAcademicStore>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<RegistrationCommandFactory>();
        builder.Services.AddScoped<RegistrationEndpointService>();
        builder.Services.AddScoped<StudentAcademicProfileService>();
        builder.Services.AddScoped<RegistrationTransactionCoordinator>();
        builder.Services
            .AddAuthentication(AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(
                AuthenticationScheme,
                _ => { });
        builder.Services.AddIdentityAuthorization();
        builder.Services.AddAntiforgery(options => options.HeaderName = AntiforgeryHeader);
        builder.Services.AddRateLimiter(_ => { });

        var application = builder.Build();
        application.UseRouting();
        application.UseStudentRegistrationIdentitySecurity();
        application.MapGet(
            "/test/antiforgery",
            (HttpContext context, IAntiforgery antiforgery) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(context);
                return Results.Json(new AntiforgeryToken(tokens.RequestToken!));
            }).AllowAnonymous();
        application.MapSpec014Endpoints();
        await application.StartAsync();
        return application;
    }

    private static HttpRequestMessage Post(
        string? role,
        string? permission,
        Guid userId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/student/terms/{Guid.NewGuid():D}/registrations")
        {
            Content = JsonContent.Create(new SubmitRegistrationRequest(
                Guid.NewGuid(),
                "AQ==",
                Guid.NewGuid()))
        };
        Authenticate(request, role, permission, userId);
        return request;
    }

    private static HttpRequestMessage Get(
        string path,
        string? role,
        string? permission)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        Authenticate(request, role, permission);
        return request;
    }

    private static void Authenticate(
        HttpRequestMessage request,
        string? role,
        string? permission,
        Guid? userId = null)
    {
        request.Headers.TryAddWithoutValidation(
            "X-SPEC014-User",
            (userId ?? Guid.NewGuid()).ToString());
        if (role is not null)
        {
            request.Headers.TryAddWithoutValidation("X-SPEC014-Role", role);
        }

        if (permission is not null)
        {
            request.Headers.TryAddWithoutValidation("X-SPEC014-Permission", permission);
        }
    }

    private static void AddAntiforgery(
        HttpRequestMessage request,
        AntiforgeryClientState state)
    {
        request.Headers.TryAddWithoutValidation(AntiforgeryHeader, state.Token);
        request.Headers.TryAddWithoutValidation("Cookie", state.CookieHeader);
    }

    private static async Task<AntiforgeryClientState> GetAntiforgeryAsync(
        HttpClient client,
        Guid userId)
    {
        using var request = Get(
            "/test/antiforgery",
            RolePolicies.Student,
            RolePolicies.RegistrationSubmitOwn);
        request.Headers.Remove("X-SPEC014-User");
        request.Headers.TryAddWithoutValidation("X-SPEC014-User", userId.ToString());
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AntiforgeryToken>();
        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0]);
        return new(payload!.Token, string.Join("; ", cookies));
    }

    private sealed record AntiforgeryToken(string Token);

    private sealed record AntiforgeryClientState(string Token, string CookieHeader);

    private sealed class HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-SPEC014-User", out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };
            if (Request.Headers.TryGetValue("X-SPEC014-Role", out var role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            if (Request.Headers.TryGetValue("X-SPEC014-Permission", out var permission))
            {
                claims.Add(new Claim(
                    RolePolicies.PermissionClaimType,
                    permission.ToString()));
            }

            var identity = new ClaimsIdentity(
                claims,
                Scheme.Name,
                ClaimTypes.Name,
                ClaimTypes.Role);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }

    private sealed class TrackingEndpointStore : IRegistrationEndpointStore
    {
        private int _resolveCalls;
        private int _lookupCalls;

        public int ResolveCalls => Volatile.Read(ref _resolveCalls);

        public int LookupCalls => Volatile.Read(ref _lookupCalls);

        public Task<RegistrationCommandContext?> ResolveCommandContextAsync(
            Guid applicationUserId,
            Guid termId,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _resolveCalls);
            return Task.FromResult<RegistrationCommandContext?>(null);
        }

        public Task<RegistrationEndpointResult> SubmitAsync(
            RegistrationCommand command,
            RegistrationTransactionCoordinator coordinator,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The context-not-found probe must not submit.");

        public Task<RegistrationEndpointResult> LookupAsync(
            Guid applicationUserId,
            Guid termId,
            Guid clientRequestId,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _lookupCalls);
            return Task.FromResult(new RegistrationEndpointResult(
                RegistrationEndpointOutcome.NotFound,
                ErrorCode: "REQUEST_NOT_FOUND"));
        }
    }

    private sealed class UnusedAcademicStore : IStudentAcademicProfileStore
    {
        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) => Unused();

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) => Unused();

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default) => Unused();

        public Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default) => Unused();

        private static Task<AcademicProfileStoreResult> Unused() =>
            throw new InvalidOperationException("The security boundary must reject before this store.");
    }
}
