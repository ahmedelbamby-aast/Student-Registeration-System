using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.IdentityAccess.Endpoints;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.SecurityTests;

public sealed class IdentityRuntimeCompositionTests
{
    private const string TestAuthenticationScheme = "SPEC007-Test";
    private const string AntiforgeryHeader = "X-SPEC007-XSRF";

    [Fact]
    public async Task Endpoint10_rejects_students_and_antiforgery_precedes_the_handler()
    {
        var store = new TrackingIdentityAccountStore();
        await using var application = await CreateIdentityEndpointApplicationAsync(store);
        using var client = application.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
        var antiforgery = await GetAntiforgeryTokenAsync(client);

        using var studentRequest = ContextRequest(
            [RolePolicies.Student],
            RolePolicies.Lecturer);
        studentRequest.Headers.TryAddWithoutValidation(AntiforgeryHeader, antiforgery.Token);
        studentRequest.Headers.TryAddWithoutValidation("Cookie", antiforgery.CookieHeader);
        using var studentResponse = await client.SendAsync(studentRequest);

        Assert.Equal(HttpStatusCode.Forbidden, studentResponse.StatusCode);
        Assert.Equal(0, store.CallCount);

        using var missingAntiforgeryRequest = ContextRequest(
            [RolePolicies.Lecturer],
            RolePolicies.Lecturer);
        using var missingAntiforgeryResponse = await client.SendAsync(missingAntiforgeryRequest);

        Assert.Equal(HttpStatusCode.BadRequest, missingAntiforgeryResponse.StatusCode);
        Assert.Equal(0, store.CallCount);
    }

    [Fact]
    public async Task Endpoint10_issues_only_the_selected_role_as_authorizing_claims()
    {
        var user = new ApplicationUser(
            Guid.NewGuid(),
            "dual.admin.lecturer",
            "DUAL.ADMIN.LECTURER",
            universityId: null,
            passwordHash: "HASHED-CREDENTIAL-NOT-USED-BY-CONTEXT-TEST",
            securityStamp: Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
        var store = new TrackingIdentityAccountStore(
            user,
            [RolePolicies.Admin, RolePolicies.Lecturer]);
        await using var application = await CreateIdentityEndpointApplicationAsync(store);
        using var client = application.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
        var availableRoles = new[] { RolePolicies.Admin, RolePolicies.Lecturer };
        var antiforgery = await GetAntiforgeryTokenAsync(
            client,
            availableRoles,
            user.Id);
        var authorization = application.Services.GetRequiredService<IAuthorizationService>();
        var capture = application.Services.GetRequiredService<SignInCapture>();

        using var lecturerRequest = ContextRequest(
            availableRoles,
            RolePolicies.Lecturer,
            user.Id);
        lecturerRequest.Headers.TryAddWithoutValidation(AntiforgeryHeader, antiforgery.Token);
        lecturerRequest.Headers.TryAddWithoutValidation("Cookie", antiforgery.CookieHeader);
        using var lecturerResponse = await client.SendAsync(lecturerRequest);

        Assert.Equal(HttpStatusCode.OK, lecturerResponse.StatusCode);
        Assert.NotNull(capture.Principal);
        var lecturerPrincipal = capture.Principal!;
        Assert.Equal(
            [RolePolicies.Lecturer],
            lecturerPrincipal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray());
        Assert.Equal(
            availableRoles,
            lecturerPrincipal.FindAll(RolePolicies.AvailableRoleClaimType)
                .Select(claim => claim.Value)
                .Order(StringComparer.Ordinal)
                .ToArray());
        Assert.False((await authorization.AuthorizeAsync(
            lecturerPrincipal,
            resource: null,
            RolePolicies.Admin)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            lecturerPrincipal,
            resource: null,
            RolePolicies.IdentityManagement)).Succeeded);
        Assert.Equal(
            [RolePolicies.ContextRead],
            lecturerPrincipal.FindAll(RolePolicies.PermissionClaimType)
                .Select(claim => claim.Value)
                .Order(StringComparer.Ordinal)
                .ToArray());

        capture.Principal = null;
        using var adminRequest = ContextRequest(
            availableRoles,
            RolePolicies.Admin,
            user.Id);
        adminRequest.Headers.TryAddWithoutValidation(AntiforgeryHeader, antiforgery.Token);
        adminRequest.Headers.TryAddWithoutValidation("Cookie", antiforgery.CookieHeader);
        using var adminResponse = await client.SendAsync(adminRequest);

        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.NotNull(capture.Principal);
        var adminPrincipal = capture.Principal!;
        Assert.Equal(
            [RolePolicies.Admin],
            adminPrincipal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray());
        Assert.False((await authorization.AuthorizeAsync(
            adminPrincipal,
            resource: null,
            RolePolicies.Lecturer)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(
            adminPrincipal,
            resource: null,
            RolePolicies.IdentityManagement)).Succeeded);
        Assert.Equal(
            new[]
            {
                RolePolicies.AcademicProfilesManage,
                RolePolicies.AcademicTermsManage,
                RolePolicies.ContextRead,
                RolePolicies.IdentityAccessManage
            },
            adminPrincipal.FindAll(RolePolicies.PermissionClaimType)
                .Select(claim => claim.Value)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void Sql_composition_requires_configuration_and_builds_the_complete_identity_graph()
    {
        var missing = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddStudentRegistrationSqlServer(
                new ConfigurationBuilder().Build()));
        Assert.StartsWith("DATABASE_CONNECTION_REQUIRED:", missing.Message, StringComparison.Ordinal);

        var configuration = Configuration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:StudentRegistration"] =
                "Server=(localdb)\\MSSQLLocalDB;Database=StudentRegistrationComposition;Integrated Security=true;TrustServerCertificate=true"
        });
        var environment = new TestHostEnvironment("Testing", Path.GetTempPath());
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddAuthoritativeTime();
        services.AddStudentRegistrationSqlServer(configuration);
        services.AddStudentRegistrationIdentitySecurity(configuration, environment);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;

        var context = scoped.GetRequiredService<StudentRegistrationDbContext>();
        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
        Assert.Contains(
            "RetryingExecutionStrategy",
            context.Database.CreateExecutionStrategy().GetType().Name,
            StringComparison.Ordinal);
        Assert.IsType<IdentityAccountStore>(scoped.GetRequiredService<IIdentityAccountStore>());
        Assert.IsType<IdentitySeedStore>(scoped.GetRequiredService<IIdentitySeedStore>());
        Assert.IsType<IdentityAbuseStateStore>(scoped.GetRequiredService<IIdentityAbuseStateStore>());
        Assert.IsType<AdminUserLifecycleStore>(scoped.GetRequiredService<IAdminUserLifecycleStore>());
        Assert.IsType<AuditTransactionWriter>(scoped.GetRequiredService<IAuditEventWriter>());
        Assert.NotNull(scoped.GetRequiredService<AdminUserLifecycleService>());
        Assert.NotNull(scoped.GetRequiredService<IProvisionedCredentialHandoff>());
    }

    [Fact]
    public async Task Production_identity_startup_requires_an_approved_provisioning_handoff()
    {
        var withoutAdapter = CreateProductionIdentityProvider(
            includeHandoff: false,
            includeApproval: true);
        await using (withoutAdapter)
        {
            var guard = withoutAdapter.GetServices<IHostedService>()
                .Single(service => service.GetType().Name == "ProductionIdentitySecurityGuard");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                guard.StartAsync(CancellationToken.None));
            Assert.StartsWith(
                "IDENTITY_PROVISIONING_HANDOFF_NOT_APPROVED:",
                exception.Message,
                StringComparison.Ordinal);
        }

        var withoutApproval = CreateProductionIdentityProvider(
            includeHandoff: true,
            includeApproval: false);
        await using (withoutApproval)
        {
            var guard = withoutApproval.GetServices<IHostedService>()
                .Single(service => service.GetType().Name == "ProductionIdentitySecurityGuard");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                guard.StartAsync(CancellationToken.None));
            Assert.StartsWith(
                "IDENTITY_PROVISIONING_HANDOFF_NOT_APPROVED:",
                exception.Message,
                StringComparison.Ordinal);
        }

        var approved = CreateProductionIdentityProvider(
            includeHandoff: true,
            includeApproval: true);
        await using (approved)
        {
            var guard = approved.GetServices<IHostedService>()
                .Single(service => service.GetType().Name == "ProductionIdentitySecurityGuard");
            await guard.StartAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Hosted_web_app_serves_static_assets_and_falls_back_without_shadowing_api_routes()
    {
        var contentRoot = Path.Combine(
            Path.GetTempPath(),
            "StudentRegistration-SPEC007-hosting",
            Guid.NewGuid().ToString("N"));
        var webRoot = Path.Combine(contentRoot, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "index.html"),
            "<!doctype html><title>Hosted Student Registration</title>");
        await File.WriteAllTextAsync(Path.Combine(webRoot, "site.css"), "body { color: navy; }");

        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = typeof(IdentitySecurityRegistration).Assembly.FullName,
                EnvironmentName = "Testing",
                ContentRootPath = contentRoot,
                WebRootPath = webRoot
            });
            builder.WebHost.UseTestServer();
            await using var application = builder.Build();
            application.UseStudentRegistrationWebApp();
            application.MapGet("/api/probe", () => Results.Text("api-ok"));
            application.MapStudentRegistrationWebAppFallback();
            await application.StartAsync();

            using var client = application.GetTestClient();
            Assert.Equal("api-ok", await client.GetStringAsync("/api/probe"));
            Assert.Equal(
                HttpStatusCode.NotFound,
                (await client.GetAsync("/api/missing")).StatusCode);
            Assert.Contains(
                "Hosted Student Registration",
                await client.GetStringAsync("/student/login"),
                StringComparison.Ordinal);
            Assert.Contains(
                "color: navy",
                await client.GetStringAsync("/site.css"),
                StringComparison.Ordinal);
        }
        finally
        {
            var resolved = Path.GetFullPath(contentRoot);
            var expectedRoot = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                "StudentRegistration-SPEC007-hosting"));
            Assert.StartsWith(expectedRoot, resolved, StringComparison.OrdinalIgnoreCase);
            if (Directory.Exists(resolved))
            {
                Directory.Delete(resolved, recursive: true);
            }
        }
    }

    private static async Task<WebApplication> CreateIdentityEndpointApplicationAsync(
        TrackingIdentityAccountStore store)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IIdentityAccountStore>(store);
        builder.Services.AddSingleton<SignInCapture>();
        builder.Services.AddSingleton<IAccountRecoveryProofDelivery, RejectingRecoveryDelivery>();
        builder.Services.AddSingleton(new IdentitySecurityOptions());
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IIdentityPasswordValidator, IdentityPasswordValidator>();
        builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
        builder.Services.AddScoped<SessionLifecycleService>();
        builder.Services
            .AddAuthentication(TestAuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(
                TestAuthenticationScheme,
                _ => { })
            .AddCookie(
                IdentityAuthenticationDefaults.AuthenticationScheme,
                options => options.Events.OnSigningIn = context =>
                {
                    context.HttpContext.RequestServices
                        .GetRequiredService<SignInCapture>()
                        .Principal = context.Principal;
                    return Task.CompletedTask;
                });
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
                return Results.Json(new AntiforgeryTokenResponse(tokens.RequestToken!));
            }).AllowAnonymous();
        application.MapSpec007Endpoints();
        await application.StartAsync();
        return application;
    }

    private static HttpRequestMessage ContextRequest(
        IReadOnlyList<string> availableRoles,
        string requestedRole,
        Guid? userId = null,
        string? activeRole = null)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Put,
            "/api/auth/session/context")
        {
            Content = JsonContent.Create(new { role = requestedRole })
        };
        request.Headers.TryAddWithoutValidation(
            "X-SPEC007-Available-Roles",
            string.Join(',', availableRoles));
        request.Headers.TryAddWithoutValidation(
            "X-SPEC007-User",
            (userId ?? Guid.NewGuid()).ToString());
        if (activeRole is not null)
        {
            request.Headers.TryAddWithoutValidation("X-SPEC007-Active-Role", activeRole);
        }

        return request;
    }

    private static async Task<AntiforgeryClientToken> GetAntiforgeryTokenAsync(
        HttpClient client,
        IReadOnlyList<string>? availableRoles = null,
        Guid? userId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test/antiforgery");
        if (availableRoles is not null && userId is not null)
        {
            request.Headers.TryAddWithoutValidation(
                "X-SPEC007-Available-Roles",
                string.Join(',', availableRoles));
            request.Headers.TryAddWithoutValidation("X-SPEC007-User", userId.Value.ToString());
        }

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0]);
        return new AntiforgeryClientToken(payload!.Token, string.Join("; ", cookies));
    }

    private static ServiceProvider CreateProductionIdentityProvider(
        bool includeHandoff,
        bool includeApproval)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Identity:Security:UseDemoCredentialPolicy"] = "false",
            ["Identity:Security:ApprovedInstitutionalPolicyVersion"] = "TEST-APPROVED-1",
            ["Identity:Security:AbuseSubjectHmacKey"] =
                Convert.ToBase64String(Enumerable.Repeat((byte)0x5A, 32).ToArray()),
            ["Identity:Recovery:ApprovedProvider"] = "testing-approved-recovery"
        };
        if (includeApproval)
        {
            settings["Identity:Provisioning:ApprovedProvider"] =
                "testing-approved-provisioning";
        }

        var configuration = Configuration(settings);
        var environment = new TestHostEnvironment(Environments.Production, Path.GetTempPath());
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddSingleton<IAccountRecoveryProofDelivery, RejectingRecoveryDelivery>();
        if (includeHandoff)
        {
            services.AddSingleton<IProvisionedCredentialHandoff, NoOpProvisionedCredentialHandoff>();
        }

        services.AddStudentRegistrationIdentitySecurity(configuration, environment);
        return services.BuildServiceProvider();
    }

    private static IConfiguration Configuration(
        IDictionary<string, string?> settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

    private sealed record AntiforgeryTokenResponse(string Token);

    private sealed record AntiforgeryClientToken(string Token, string CookieHeader);

    private sealed class HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-SPEC007-Available-Roles", out var roles)
                || !Request.Headers.TryGetValue("X-SPEC007-User", out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };
            claims.AddRange(
                roles.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(role => new Claim(RolePolicies.AvailableRoleClaimType, role)));
            if (Request.Headers.TryGetValue("X-SPEC007-Active-Role", out var activeRole))
            {
                claims.Add(new Claim(ClaimTypes.Role, activeRole.ToString()));
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

    private sealed class TrackingIdentityAccountStore : IIdentityAccountStore
    {
        private int _callCount;
        private readonly ApplicationUser? _user;
        private readonly IReadOnlyList<string> _roles;

        public TrackingIdentityAccountStore(
            ApplicationUser? user = null,
            IReadOnlyList<string>? roles = null)
        {
            _user = user;
            _roles = roles ?? [];
        }

        public int CallCount => Volatile.Read(ref _callCount);

        public Task<ApplicationUser?> FindStudentByUniversityIdAsync(
            string normalizedUniversityId,
            CancellationToken cancellationToken) => Invoked<ApplicationUser?>();

        public Task<ApplicationUser?> FindByNormalizedUserNameAsync(
            string normalizedUserName,
            CancellationToken cancellationToken) => Invoked<ApplicationUser?>();

        public Task<ApplicationUser?> FindByIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) =>
            Invoked(_user?.Id == applicationUserId ? _user : null);

        public Task<Staff?> FindStaffAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Invoked<Staff?>();

        public Task<bool> IsStudentActivatedAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Invoked<bool>();

        public Task<IReadOnlyList<string>> GetEffectiveRolesAsync(
            Guid applicationUserId,
            DateTime utcNow,
            CancellationToken cancellationToken) => Invoked(_roles);

        public Task RecordAuthenticationFailureAsync(
            Guid? applicationUserId,
            string operation,
            CancellationToken cancellationToken) => Invoked();

        public Task ResetAuthenticationFailuresAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Invoked();

        public Task RecordActivationFailureAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Invoked();

        public Task<bool> TryActivateAsync(
            Guid applicationUserId,
            byte[] expectedVersion,
            string newPasswordHash,
            string newSecurityStamp,
            DateTime activatedAtUtc,
            CancellationToken cancellationToken) => Invoked<bool>();

        public Task<bool> AddRecoveryChallengeAsync(
            AccountRecoveryChallenge challenge,
            CancellationToken cancellationToken) => Invoked<bool>();

        public Task<AccountRecoveryChallenge?> FindRecoveryChallengeAsync(
            string tokenHash,
            CancellationToken cancellationToken) => Invoked<AccountRecoveryChallenge?>();

        public Task RecordRecoveryFailureAsync(
            Guid challengeId,
            int maximumAttempts,
            CancellationToken cancellationToken) => Invoked();

        public Task<bool> TryCompleteRecoveryAsync(
            Guid challengeId,
            byte[] expectedChallengeVersion,
            Guid applicationUserId,
            string newPasswordHash,
            string newSecurityStamp,
            DateTime consumedAtUtc,
            int maximumAttempts,
            CancellationToken cancellationToken) => Invoked<bool>();

        public Task<bool> ChangePasswordAsync(
            Guid applicationUserId,
            byte[] expectedVersion,
            string newPasswordHash,
            string newSecurityStamp,
            CancellationToken cancellationToken) => Invoked<bool>();

        public Task<bool> RotateSecurityStampAsync(
            Guid applicationUserId,
            byte[] expectedVersion,
            string newSecurityStamp,
            CancellationToken cancellationToken) => Invoked<bool>();

        private Task Invoked()
        {
            Interlocked.Increment(ref _callCount);
            return Task.CompletedTask;
        }

        private Task<T> Invoked<T>(T value = default!)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(value);
        }
    }

    private sealed class SignInCapture
    {
        public ClaimsPrincipal? Principal { get; set; }
    }

    private sealed class RejectingRecoveryDelivery : IAccountRecoveryProofDelivery
    {
        public Task<RecoveryProofDeliveryResult> DeliverAsync(
            RecoveryProofDeliveryRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new RecoveryProofDeliveryResult(false, "test-rejected"));
    }

    private sealed class NoOpProvisionedCredentialHandoff : IProvisionedCredentialHandoff
    {
        public Task PrepareAsync(
            Guid importId,
            IReadOnlyCollection<DemoCredential> credentials,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task CompleteAsync(Guid importId, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task AbortAsync(Guid importId, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class TestHostEnvironment(
        string environmentName,
        string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "StudentRegistration.SecurityTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
