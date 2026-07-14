using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentRegistration.Api.Development;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Api.Composition;

public static class IdentitySecurityRegistration
{
    public const string AuthenticationScheme = IdentityAuthenticationDefaults.AuthenticationScheme;
    public const string AuthenticationCookieName = "__Host-StudentRegistration.Session";
    public const string AntiforgeryRequestCookieName = "XSRF-TOKEN";
    public const string AntiforgeryRequestHeaderName = "X-XSRF-TOKEN";

    private const string FrameworkAntiforgeryCookieName =
        "__Host-StudentRegistration.Antiforgery";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(60);

    public static IServiceCollection AddStudentRegistrationIdentitySecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var security = configuration
            .GetSection("Identity:Security")
            .Get<IdentitySecurityOptions>() ?? new IdentitySecurityOptions();
        security.ValidateForEnvironment(environment.EnvironmentName);

        services.AddSingleton(security);
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = security.HasherCompatibilityMode;
            options.IterationCount = security.PasswordHashIterations;
        });
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = security.MinimumPasswordLength;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = security.MaximumFailures;
            options.Lockout.DefaultLockoutTimeSpan = security.LockoutDuration;
            options.Lockout.AllowedForNewUsers = true;
        });
        services.TryAddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
        services.TryAddSingleton<IIdentityPasswordValidator, IdentityPasswordValidator>();
        services.TryAddScoped<StudentAuthenticationService>();
        services.TryAddScoped<StudentActivationService>();
        services.TryAddScoped<StaffAuthenticationService>();
        services.TryAddScoped<SessionLifecycleService>();
        services.TryAddScoped<DemoIdentitySeedContributor>();
        services.TryAddScoped<AdminUserLifecycleService>();
        services.TryAddScoped<IdentityCookieAuthenticationEvents>();
        services.TryAddScoped<IdentityAbuseControl>();
        services.TryAddScoped<IIdentityAccountStore, IdentityAccountStore>();
        services.TryAddScoped<IIdentitySeedStore, IdentitySeedStore>();
        services.TryAddScoped<IAdminUserLifecycleStore, AdminUserLifecycleStore>();
        services.TryAddScoped<IIdentityAbuseStateStore, IdentityAbuseStateStore>();
        services.TryAddScoped<IAuditEventWriter, AuditTransactionWriter>();

        if (environment.IsDevelopment())
        {
            services.TryAddSingleton<IIdentityAbuseKeyProvider, DevelopmentIdentityAbuseKeyProvider>();
            services.TryAddSingleton<DemoCredentialSheetWriter>();
            services.TryAddSingleton<IAccountRecoveryProofDelivery, DevelopmentRecoveryProofDelivery>();
            services.TryAddSingleton<IProvisionedCredentialHandoff, DevelopmentProvisionedCredentialHandoff>();
        }
        else if (environment.IsEnvironment("Testing"))
        {
            services.TryAddSingleton<IIdentityAbuseKeyProvider, TestingIdentityAbuseKeyProvider>();
            services.TryAddSingleton<TestingRecoveryProofDelivery>();
            services.TryAddSingleton<IAccountRecoveryProofDelivery>(serviceProvider =>
                serviceProvider.GetRequiredService<TestingRecoveryProofDelivery>());
            services.TryAddSingleton<TestingProvisionedCredentialHandoff>();
            services.TryAddSingleton<IProvisionedCredentialHandoff>(serviceProvider =>
                serviceProvider.GetRequiredService<TestingProvisionedCredentialHandoff>());
        }
        else if (environment.IsProduction())
        {
            services.TryAddSingleton<IIdentityAbuseKeyProvider, ConfiguredIdentityAbuseKeyProvider>();
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationScheme;
                options.DefaultSignInScheme = AuthenticationScheme;
            })
            .AddCookie(AuthenticationScheme, options =>
            {
                options.Cookie.Name = AuthenticationCookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = "/";
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = SessionLifetime;
                options.SlidingExpiration = false;
                options.EventsType = typeof(IdentityCookieAuthenticationEvents);
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryRequestHeaderName;
            options.Cookie.Name = FrameworkAntiforgeryCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
        });

        services.AddIdentityAuthorization();
        AddCoarseRequestLimiters(services);
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, ProductionIdentitySecurityGuard>());
        return services;
    }

    public static IApplicationBuilder UseStudentRegistrationIdentitySecurity(
        this IApplicationBuilder application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.UseAuthentication();
        application.UseAuthorization();
        application.UseRateLimiter();
        application.UseAntiforgery();
        application.UseMiddleware<AntiforgeryValidationEnforcementMiddleware>();
        application.UseMiddleware<AntiforgeryRequestCookieMiddleware>();
        return application;
    }

    private static void AddCoarseRequestLimiters(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = static (context, _) =>
            {
                if (string.Equals(
                        context.HttpContext.Request.Path.Value,
                        "/api/auth/recovery/request",
                        StringComparison.OrdinalIgnoreCase))
                {
                    // Recovery existence and throttling are deliberately
                    // indistinguishable at the public boundary.
                    context.HttpContext.Response.StatusCode = StatusCodes.Status202Accepted;
                }
                else
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                }

                return ValueTask.CompletedTask;
            };
            foreach (var policyName in new[]
                     {
                         IdentityRateLimitPolicies.StudentLogin,
                         IdentityRateLimitPolicies.StaffLogin,
                         IdentityRateLimitPolicies.StudentActivation,
                         IdentityRateLimitPolicies.RecoveryRequest,
                         IdentityRateLimitPolicies.RecoveryCompletion,
                         IdentityRateLimitPolicies.PasswordChange
                     })
            {
                options.AddPolicy(
                    policyName,
                    _ => RateLimitPartition.GetFixedWindowLimiter(
                        policyName,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 25,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(1)
                        }));
            }
        });
    }
}

internal sealed class AntiforgeryValidationEnforcementMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var validation = context.Features.Get<IAntiforgeryValidationFeature>();
        if (validation is { IsValid: false })
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                new ApiError(
                    "ANTIFORGERY_INVALID",
                    "The request could not be completed.",
                    context.TraceIdentifier),
                context.RequestAborted);
            return;
        }

        await next(context);
    }
}

internal sealed class AntiforgeryRequestCookieMiddleware(
    RequestDelegate next,
    IAntiforgery antiforgery)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (HttpMethods.IsGet(context.Request.Method)
            && !string.Equals(
                context.Request.Headers["Sec-Fetch-Site"],
                "cross-site",
                StringComparison.OrdinalIgnoreCase))
        {
            var tokenSet = antiforgery.GetAndStoreTokens(context);
            if (!string.IsNullOrEmpty(tokenSet.RequestToken))
            {
                context.Response.Cookies.Append(
                    IdentitySecurityRegistration.AntiforgeryRequestCookieName,
                    tokenSet.RequestToken,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Path = "/",
                        IsEssential = true
                    });
            }
        }

        await next(context);
    }
}

internal sealed class ProductionIdentitySecurityGuard(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IHostEnvironment environment) : IHostedService
{
    private const string Testing = "Testing";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment()
            || environment.IsEnvironment(Testing))
        {
            return Task.CompletedTask;
        }

        if (!environment.IsProduction())
        {
            throw new InvalidOperationException(
                "IDENTITY_ENVIRONMENT_UNSUPPORTED: Only Development, Testing, and Production are recognized.");
        }

        using var scope = serviceProvider.CreateScope();
        var delivery = scope.ServiceProvider.GetService<IAccountRecoveryProofDelivery>();
        var credentialHandoff = scope.ServiceProvider.GetService<IProvisionedCredentialHandoff>();
        var abuseKey = scope.ServiceProvider.GetService<IIdentityAbuseKeyProvider>();
        var approvedProvider = configuration["Identity:Recovery:ApprovedProvider"];
        if (delivery is null || string.IsNullOrWhiteSpace(approvedProvider))
        {
            throw new InvalidOperationException(
                "IDENTITY_RECOVERY_PROVIDER_NOT_APPROVED: Production requires an approved institutional recovery provider.");
        }

        var approvedProvisioningProvider =
            configuration["Identity:Provisioning:ApprovedProvider"];
        if (credentialHandoff is null
            || string.IsNullOrWhiteSpace(approvedProvisioningProvider))
        {
            throw new InvalidOperationException(
                "IDENTITY_PROVISIONING_HANDOFF_NOT_APPROVED: Production requires an approved institutional credential handoff.");
        }

        if (abuseKey is null || abuseKey.GetKey().Length < 32)
        {
            throw new InvalidOperationException(
                "IDENTITY_ABUSE_KEY_REQUIRED: Production requires a shared external HMAC key.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
