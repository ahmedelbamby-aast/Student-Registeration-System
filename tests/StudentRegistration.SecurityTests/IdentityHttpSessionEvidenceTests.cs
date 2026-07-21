using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StudentRegistration.Api.Composition;
using StudentRegistration.Contracts.Identity;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.IdentityAccess.Endpoints;

namespace StudentRegistration.SecurityTests;

public sealed class IdentityHttpSessionEvidenceTests
{
    private const string CurrentPassword = "Correct horse battery staple";
    private const string ReplacementPassword = "Replacement credential value";

    [Theory]
    [InlineData(1, "anonymous")]
    [InlineData(2, "anonymous")]
    [InlineData(3, "anonymous")]
    [InlineData(4, "student")]
    [InlineData(5, "anonymous")]
    [InlineData(6, "anonymous")]
    [InlineData(7, "student")]
    [InlineData(8, "student")]
    [InlineData(12, "admin")]
    [InlineData(14, "admin")]
    [InlineData(15, "admin")]
    [InlineData(16, "admin")]
    public async Task Every_state_changing_identity_endpoint_rejects_missing_antiforgery_before_its_handler(
        int endpoint,
        string authenticationProfile)
    {
        await using var fixture = await RuntimeFixture.CreateAsync(replicaCount: 1);
        using var client = fixture.ReplicaOne.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");

        var authenticationCookie = string.Equals(
                authenticationProfile,
                "anonymous",
                StringComparison.Ordinal)
            ? null
            : await SignInAsync(client, authenticationProfile);
        fixture.HandlerSentinel.Reset();

        using var request = MutationRequest(endpoint, fixture.Store.User.Id);
        if (authenticationCookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", authenticationCookie);
        }

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ANTIFORGERY_INVALID", body, StringComparison.Ordinal);
        Assert.Equal(0, fixture.HandlerSentinel.ReachedCount);

        if (endpoint == 4)
        {
            Assert.DoesNotContain(
                response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : [],
                value => value.StartsWith(
                    IdentitySecurityRegistration.AuthenticationCookieName,
                    StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task Logout_endpoint_expires_the_actual_authentication_cookie()
    {
        await using var fixture = await RuntimeFixture.CreateAsync(replicaCount: 1);
        using var client = fixture.ReplicaOne.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
        var authenticationCookie = await SignInAsync(client, "student");
        var antiforgery = await AntiforgeryAsync(client, authenticationCookie);
        fixture.HandlerSentinel.Reset();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.TryAddWithoutValidation(
            IdentitySecurityRegistration.AntiforgeryRequestHeaderName,
            antiforgery.RequestToken);
        request.Headers.TryAddWithoutValidation(
            "Cookie",
            string.Join("; ", authenticationCookie, antiforgery.CookieHeader));

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, fixture.HandlerSentinel.ReachedCount);
        var expiredCookie = response.Headers.GetValues("Set-Cookie").Single(value =>
            value.StartsWith(
                $"{IdentitySecurityRegistration.AuthenticationCookieName}=",
                StringComparison.Ordinal));
        Assert.StartsWith(
            $"{IdentitySecurityRegistration.AuthenticationCookieName}=;",
            expiredCookie,
            StringComparison.Ordinal);
        Assert.True(
            expiredCookie.Contains(
                "expires=Thu, 01 Jan 1970",
                StringComparison.OrdinalIgnoreCase)
            || expiredCookie.Contains("max-age=0", StringComparison.OrdinalIgnoreCase),
            $"The authentication cookie was not expired: {expiredCookie}");
    }

    [Theory]
    [InlineData("recovery")]
    [InlineData("password-change")]
    [InlineData("revoke-all")]
    public async Task Shared_security_stamp_rejects_each_old_cookie_on_both_replicas(
        string mutation)
    {
        await using var fixture = await RuntimeFixture.CreateAsync(replicaCount: 2);
        using var replicaOneClient = fixture.ReplicaOne.GetTestClient();
        using var replicaTwoClient = fixture.ReplicaTwo!.GetTestClient();
        replicaOneClient.BaseAddress = new Uri("https://replica-one.local");
        replicaTwoClient.BaseAddress = new Uri("https://replica-two.local");
        var oldCookie = await SignInAsync(replicaOneClient, "student");

        await AssertSessionStatusAsync(replicaOneClient, oldCookie, HttpStatusCode.OK);
        await AssertSessionStatusAsync(replicaTwoClient, oldCookie, HttpStatusCode.OK);
        var originalStamp = fixture.Store.User.SecurityStamp;

        using (var scope = fixture.ReplicaOne.Services.CreateScope())
        {
            var lifecycle = scope.ServiceProvider.GetRequiredService<SessionLifecycleService>();
            SessionLifecycleResult result;
            switch (mutation)
            {
                case "recovery":
                    var request = await lifecycle.RequestRecoveryAsync(
                        fixture.Store.User.UserName);
                    Assert.Equal(SessionLifecycleOutcome.RecoveryAccepted, request.Outcome);
                    var proof = Assert.IsType<RecoveryProofDeliveryRequest>(
                        fixture.Delivery.LastRequest).Proof;
                    result = await lifecycle.CompleteRecoveryAsync(
                        proof,
                        ReplacementPassword);
                    Assert.Equal(1, fixture.Store.CompleteRecoveryCallCount);
                    break;

                case "password-change":
                    // This is deliberately the application service API itself, not a
                    // source inspection or indirect stamp mutation.
                    result = await lifecycle.ChangePasswordAsync(
                        fixture.Store.User.Id,
                        CurrentPassword,
                        ReplacementPassword);
                    Assert.Equal(1, fixture.Store.ChangePasswordCallCount);
                    break;

                case "revoke-all":
                    result = await lifecycle.RevokeAllSessionsAsync(fixture.Store.User.Id);
                    Assert.Equal(1, fixture.Store.RotateSecurityStampCallCount);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown mutation '{mutation}'.");
            }

            Assert.Equal(SessionLifecycleOutcome.Completed, result.Outcome);
        }

        Assert.NotEqual(originalStamp, fixture.Store.User.SecurityStamp);
        if (!string.Equals(mutation, "revoke-all", StringComparison.Ordinal))
        {
            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                fixture.Hasher.VerifyHashedPassword(
                    fixture.Store.User,
                    fixture.Store.User.PasswordHash,
                    ReplacementPassword));
        }

        await AssertSessionStatusAsync(
            replicaOneClient,
            oldCookie,
            HttpStatusCode.Unauthorized);
        await AssertSessionStatusAsync(
            replicaTwoClient,
            oldCookie,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Password_rotation_after_verification_cannot_issue_a_cookie_bound_to_the_new_stamp()
    {
        await using var fixture = await RuntimeFixture.CreateAsync(replicaCount: 1);
        using var client = fixture.ReplicaOne.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
        var antiforgery = await AntiforgeryAsync(client);
        var verifiedStamp = fixture.Store.User.SecurityStamp;
        var gate = fixture.Store.PauseNextAuthenticationReset();

        using var request = JsonRequest(
            HttpMethod.Post,
            "/api/auth/student/login",
            new StudentLoginRequest("AI2600001", CurrentPassword));
        request.Headers.TryAddWithoutValidation(
            IdentitySecurityRegistration.AntiforgeryRequestHeaderName,
            antiforgery.RequestToken);
        request.Headers.TryAddWithoutValidation("Cookie", antiforgery.CookieHeader);
        var responseTask = client.SendAsync(request);

        await gate.WaitUntilPausedAsync();
        fixture.Store.ReplacePasswordAndRotateStamp(
            fixture.Hasher,
            ReplacementPassword);
        gate.Release();

        using var response = await responseTask;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(verifiedStamp, fixture.Store.User.SecurityStamp);
        var issuedCookie = CookiePair(
            response,
            IdentitySecurityRegistration.AuthenticationCookieName);
        await AssertSessionStatusAsync(client, issuedCookie, HttpStatusCode.Unauthorized);
    }

    private static HttpRequestMessage MutationRequest(int endpoint, Guid userId)
    {
        var importId = Guid.Parse("10000000-0000-0000-0000-000000000014");
        var targetUserId = Guid.Parse("10000000-0000-0000-0000-000000000015");
        return endpoint switch
        {
            1 => JsonRequest(
                HttpMethod.Post,
                "/api/auth/student/login",
                new StudentLoginRequest("AI2600001", CurrentPassword)),
            2 => JsonRequest(
                HttpMethod.Post,
                "/api/auth/student/activate",
                new ActivateStudentRequest(
                    "AI2600001",
                    CurrentPassword,
                    ReplacementPassword)),
            3 => JsonRequest(
                HttpMethod.Post,
                "/api/auth/staff/login",
                new StaffLoginRequest("staff.session", CurrentPassword)),
            4 => new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout"),
            5 => JsonRequest(
                HttpMethod.Post,
                "/api/auth/recovery/request",
                new RecoveryRequest("student.session")),
            6 => JsonRequest(
                HttpMethod.Post,
                "/api/auth/recovery/complete",
                new RecoveryCompleteRequest("opaque-test-proof", ReplacementPassword)),
            7 => JsonRequest(
                HttpMethod.Post,
                "/api/auth/password/change",
                new ChangePasswordRequest(CurrentPassword, ReplacementPassword)),
            8 => new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/sessions/revoke-all"),
            12 => JsonRequest(
                HttpMethod.Post,
                "/api/admin/users/imports",
                new IdentityImportRequest(
                    "spec007-antiforgery.csv",
                    new string('0', 64),
                    "spec007-antiforgery-create",
                    [
                        new IdentityImportUserRequest(
                            "row-1",
                            "student",
                            "AI2600999",
                            null,
                            null,
                            "Synthetic Student",
                            [])
                    ])),
            14 => JsonRequest(
                HttpMethod.Post,
                $"/api/admin/users/imports/{importId}/publish",
                new IdentityImportPublishRequest("AQ==", "spec007-antiforgery-publish")),
            15 => JsonRequest(
                HttpMethod.Patch,
                $"/api/admin/users/{targetUserId}/status",
                new UserStatusRequest(false, "AQ==", "Antiforgery evidence")),
            16 => JsonRequest(
                HttpMethod.Put,
                $"/api/admin/users/{targetUserId}/roles",
                new UserRolesRequest(
                    [RolePolicies.Lecturer],
                    "AQ==",
                    "Antiforgery evidence")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(endpoint),
                endpoint,
                "The endpoint is not part of the SPEC-007 mutation matrix.")
        };
    }

    private static HttpRequestMessage JsonRequest<T>(
        HttpMethod method,
        string path,
        T value) =>
        new(method, path)
        {
            Content = JsonContent.Create(value)
        };

    private static async Task<string> SignInAsync(HttpClient client, string profile)
    {
        using var response = await client.GetAsync($"/test/sign-in/{profile}");
        response.EnsureSuccessStatusCode();
        return CookiePair(
            response,
            IdentitySecurityRegistration.AuthenticationCookieName);
    }

    private static async Task<AntiforgeryClientState> AntiforgeryAsync(
        HttpClient client,
        string? authenticationCookie = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test/antiforgery");
        if (!string.IsNullOrWhiteSpace(authenticationCookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", authenticationCookie);
        }

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0])
            .ToArray();
        var requestCookie = cookies.Single(value => value.StartsWith(
            $"{IdentitySecurityRegistration.AntiforgeryRequestCookieName}=",
            StringComparison.Ordinal));
        var requestToken = requestCookie[(requestCookie.IndexOf('=') + 1)..];
        return new AntiforgeryClientState(
            requestToken,
            string.Join("; ", cookies));
    }

    private static string CookiePair(HttpResponseMessage response, string cookieName) =>
        response.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0])
            .Single(value => value.StartsWith($"{cookieName}=", StringComparison.Ordinal));

    private static async Task AssertSessionStatusAsync(
        HttpClient client,
        string cookie,
        HttpStatusCode expectedStatus)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/session");
        request.Headers.TryAddWithoutValidation("Cookie", cookie);
        using var response = await client.SendAsync(request);
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    private sealed record AntiforgeryClientState(
        string RequestToken,
        string CookieHeader);

    private sealed class RuntimeFixture : IAsyncDisposable
    {
        private RuntimeFixture(
            RuntimeIdentityStore store,
            CapturingRecoveryProofDelivery delivery,
            IPasswordHasher<ApplicationUser> hasher,
            HandlerExecutionSentinel handlerSentinel,
            WebApplication replicaOne,
            WebApplication? replicaTwo)
        {
            Store = store;
            Delivery = delivery;
            Hasher = hasher;
            HandlerSentinel = handlerSentinel;
            ReplicaOne = replicaOne;
            ReplicaTwo = replicaTwo;
        }

        public RuntimeIdentityStore Store { get; }
        public CapturingRecoveryProofDelivery Delivery { get; }
        public IPasswordHasher<ApplicationUser> Hasher { get; }
        public HandlerExecutionSentinel HandlerSentinel { get; }
        public WebApplication ReplicaOne { get; }
        public WebApplication? ReplicaTwo { get; }

        public static async Task<RuntimeFixture> CreateAsync(int replicaCount)
        {
            if (replicaCount is < 1 or > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(replicaCount));
            }

            var security = new IdentitySecurityOptions();
            var hasher = new PasswordHasher<ApplicationUser>(
                Options.Create(new PasswordHasherOptions
                {
                    CompatibilityMode = security.HasherCompatibilityMode,
                    IterationCount = security.PasswordHashIterations
                }));
            var store = new RuntimeIdentityStore(hasher, CurrentPassword);
            var delivery = new CapturingRecoveryProofDelivery();
            var sentinel = new HandlerExecutionSentinel();
            var sharedProtection = new EphemeralDataProtectionProvider();
            var replicaOne = await CreateApplicationAsync(
                store,
                delivery,
                hasher,
                sentinel,
                sharedProtection);
            WebApplication? replicaTwo = null;
            try
            {
                if (replicaCount == 2)
                {
                    replicaTwo = await CreateApplicationAsync(
                        store,
                        delivery,
                        hasher,
                        sentinel,
                        sharedProtection);
                }

                return new RuntimeFixture(
                    store,
                    delivery,
                    hasher,
                    sentinel,
                    replicaOne,
                    replicaTwo);
            }
            catch
            {
                await replicaOne.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (ReplicaTwo is not null)
            {
                await ReplicaTwo.DisposeAsync();
            }

            await ReplicaOne.DisposeAsync();
        }

        private static async Task<WebApplication> CreateApplicationAsync(
            RuntimeIdentityStore store,
            CapturingRecoveryProofDelivery delivery,
            IPasswordHasher<ApplicationUser> hasher,
            HandlerExecutionSentinel sentinel,
            IDataProtectionProvider dataProtectionProvider)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Testing"
            });
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton<IDataProtectionProvider>(dataProtectionProvider);
            builder.Services.AddSingleton(store);
            builder.Services.AddSingleton<IIdentityAccountStore>(store);
            builder.Services.AddSingleton(delivery);
            builder.Services.AddSingleton<IAccountRecoveryProofDelivery>(delivery);
            builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>>(hasher);
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton(sentinel);
            builder.Services.AddScoped<StudentAuthenticationService>(serviceProvider =>
                new StudentAuthenticationService(
                    serviceProvider.GetRequiredService<IIdentityAccountStore>(),
                    serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(),
                    serviceProvider.GetRequiredService<TimeProvider>()));
            builder.Services.AddScoped<SessionLifecycleService>(serviceProvider =>
                new SessionLifecycleService(
                    serviceProvider.GetRequiredService<IIdentityAccountStore>(),
                    serviceProvider.GetRequiredService<IAccountRecoveryProofDelivery>(),
                    serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(),
                    serviceProvider.GetRequiredService<IIdentityPasswordValidator>(),
                    serviceProvider.GetRequiredService<IdentitySecurityOptions>(),
                    serviceProvider.GetRequiredService<TimeProvider>()));
            builder.Services.AddStudentRegistrationIdentitySecurity(
                new ConfigurationBuilder().Build(),
                builder.Environment);

            var application = builder.Build();
            application.UseRouting();
            application.UseStudentRegistrationIdentitySecurity();
            application.Use(async (context, next) =>
            {
                sentinel.Reached();
                await next(context);
            });
            application.MapGet("/test/sign-in/{profile}", TestSignInAsync)
                .AllowAnonymous();
            application.MapGet("/test/antiforgery", () => Results.NoContent())
                .AllowAnonymous();
            application.MapSpec007Endpoints();
            await application.StartAsync();
            return application;
        }

        private static async Task<IResult> TestSignInAsync(
            string profile,
            RuntimeIdentityStore store,
            HttpContext context)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, store.User.Id.ToString()),
                new(ClaimTypes.Name, store.User.UserName),
                new(
                    IdentityAuthenticationDefaults.SecurityStampClaim,
                    store.User.SecurityStamp)
            };
            switch (profile)
            {
                case "student":
                    claims.Add(new Claim(ClaimTypes.Role, RolePolicies.Student));
                    break;

                case "staff":
                    claims.Add(new Claim(ClaimTypes.Role, RolePolicies.Lecturer));
                    claims.Add(new Claim(
                        RolePolicies.AvailableRoleClaimType,
                        RolePolicies.Lecturer));
                    break;

                case "admin":
                    claims.Add(new Claim(ClaimTypes.Role, RolePolicies.Admin));
                    claims.Add(new Claim(
                        RolePolicies.AvailableRoleClaimType,
                        RolePolicies.Admin));
                    claims.Add(new Claim(
                        RolePolicies.PermissionClaimType,
                        RolePolicies.IdentityAccessManage));
                    break;

                default:
                    return Results.NotFound();
            }

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    claims,
                    IdentityAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role));
            await context.SignInAsync(
                IdentityAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    AllowRefresh = false,
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                });
            return Results.NoContent();
        }
    }

    private sealed class HandlerExecutionSentinel
    {
        private int _reachedCount;

        public int ReachedCount => Volatile.Read(ref _reachedCount);

        public void Reached() => Interlocked.Increment(ref _reachedCount);

        public void Reset() => Interlocked.Exchange(ref _reachedCount, 0);
    }

    private sealed class CapturingRecoveryProofDelivery : IAccountRecoveryProofDelivery
    {
        public RecoveryProofDeliveryRequest? LastRequest { get; private set; }

        public Task<RecoveryProofDeliveryResult> DeliverAsync(
            RecoveryProofDeliveryRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            return Task.FromResult(
                new RecoveryProofDeliveryResult(true, $"test:{Guid.NewGuid():N}"));
        }
    }

    private sealed class RuntimeIdentityStore : IIdentityAccountStore
    {
        private readonly ConcurrentDictionary<string, AccountRecoveryChallenge> _challenges =
            new(StringComparer.Ordinal);
        private readonly object _transitionLock = new();
        private IReadOnlyList<string> _effectiveRoles = [RolePolicies.Student];
        private DeterministicOperationGate? _nextAuthenticationResetGate;
        private DeterministicOperationGate? _nextRoleReadGate;

        public RuntimeIdentityStore(
            IPasswordHasher<ApplicationUser> hasher,
            string password)
        {
            User = new ApplicationUser(
                Guid.NewGuid(),
                "student.session",
                "STUDENT.SESSION",
                "AI2600001",
                "TRANSIENT",
                Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
            User.ReplacePasswordHash(hasher.HashPassword(User, password), User.SecurityStamp);
        }

        public ApplicationUser User { get; }
        public int CompleteRecoveryCallCount { get; private set; }
        public int ChangePasswordCallCount { get; private set; }
        public int RotateSecurityStampCallCount { get; private set; }

        public DeterministicOperationGate PauseNextAuthenticationReset()
        {
            var gate = new DeterministicOperationGate();
            Assert.Null(Interlocked.CompareExchange(
                ref _nextAuthenticationResetGate,
                gate,
                null));
            return gate;
        }

        public DeterministicOperationGate PauseNextRoleRead()
        {
            var gate = new DeterministicOperationGate();
            Assert.Null(Interlocked.CompareExchange(ref _nextRoleReadGate, gate, null));
            return gate;
        }

        public void ReplacePasswordAndRotateStamp(
            IPasswordHasher<ApplicationUser> hasher,
            string password)
        {
            lock (_transitionLock)
            {
                User.ReplacePasswordHash(
                    hasher.HashPassword(User, password),
                    Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
            }
        }

        public void SetEffectiveRoles(
            IReadOnlyList<string> roles,
            bool rotateSecurityStamp = false)
        {
            lock (_transitionLock)
            {
                _effectiveRoles = roles.ToArray();
                if (rotateSecurityStamp)
                {
                    User.RotateSecurityStamp(
                        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
                }
            }
        }

        public Task<ApplicationUser?> FindStudentByUniversityIdAsync(
            string normalizedUniversityId,
            CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(
                string.Equals(
                    normalizedUniversityId,
                    User.UniversityId,
                    StringComparison.Ordinal)
                    ? User
                    : null);

        public Task<ApplicationUser?> FindByNormalizedUserNameAsync(
            string normalizedUserName,
            CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(
                string.Equals(
                    normalizedUserName,
                    User.NormalizedUserName,
                    StringComparison.Ordinal)
                    ? User
                    : null);

        public Task<ApplicationUser?> FindByIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(applicationUserId == User.Id ? User : null);

        public Task<Staff?> FindStaffAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) =>
            Task.FromResult<Staff?>(null);

        public Task<bool> IsStudentActivatedAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) =>
            Task.FromResult(applicationUserId == User.Id);

        public async Task<IReadOnlyList<string>> GetEffectiveRolesAsync(
            Guid applicationUserId,
            DateTime utcNow,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<string> roles;
            lock (_transitionLock)
            {
                roles = applicationUserId == User.Id ? _effectiveRoles.ToArray() : [];
            }

            var gate = Interlocked.Exchange(ref _nextRoleReadGate, null);
            if (gate is not null)
            {
                await gate.PauseAsync(cancellationToken);
            }

            return roles;
        }

        public Task RecordAuthenticationFailureAsync(
            Guid? applicationUserId,
            string operation,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task ResetAuthenticationFailuresAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var gate = Interlocked.Exchange(ref _nextAuthenticationResetGate, null);
            if (gate is not null)
            {
                await gate.PauseAsync(cancellationToken);
            }
        }

        public Task RecordActivationFailureAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> TryActivateAsync(
            Guid applicationUserId,
            byte[] expectedVersion,
            string newPasswordHash,
            string newSecurityStamp,
            DateTime activatedAtUtc,
            CancellationToken cancellationToken) => Task.FromResult(false);

        public Task<bool> AddRecoveryChallengeAsync(
            AccountRecoveryChallenge challenge,
            CancellationToken cancellationToken) =>
            Task.FromResult(_challenges.TryAdd(challenge.TokenHash, challenge));

        public Task<AccountRecoveryChallenge?> FindRecoveryChallengeAsync(
            string tokenHash,
            CancellationToken cancellationToken) =>
            Task.FromResult(_challenges.GetValueOrDefault(tokenHash));

        public Task RecordRecoveryFailureAsync(
            Guid challengeId,
            int maximumAttempts,
            CancellationToken cancellationToken)
        {
            lock (_transitionLock)
            {
                _challenges.Values.Single(challenge => challenge.Id == challengeId)
                    .RecordFailedAttempt(maximumAttempts);
            }

            return Task.CompletedTask;
        }

        public Task<bool> TryCompleteRecoveryAsync(
            Guid challengeId,
            byte[] expectedChallengeVersion,
            Guid applicationUserId,
            string newPasswordHash,
            string newSecurityStamp,
            DateTime consumedAtUtc,
            int maximumAttempts,
            CancellationToken cancellationToken)
        {
            lock (_transitionLock)
            {
                var challenge = _challenges.Values.Single(candidate =>
                    candidate.Id == challengeId);
                if (applicationUserId != User.Id
                    || !challenge.TryConsume(consumedAtUtc, maximumAttempts))
                {
                    return Task.FromResult(false);
                }

                User.ReplacePasswordHash(newPasswordHash, newSecurityStamp);
                CompleteRecoveryCallCount++;
                return Task.FromResult(true);
            }
        }

        public Task<bool> ChangePasswordAsync(
            Guid applicationUserId,
            byte[] expectedVersion,
            string newPasswordHash,
            string newSecurityStamp,
            CancellationToken cancellationToken)
        {
            lock (_transitionLock)
            {
                if (applicationUserId != User.Id)
                {
                    return Task.FromResult(false);
                }

                User.ReplacePasswordHash(newPasswordHash, newSecurityStamp);
                ChangePasswordCallCount++;
                return Task.FromResult(true);
            }
        }

        public Task<bool> RotateSecurityStampAsync(
            Guid applicationUserId,
            byte[] expectedVersion,
            string newSecurityStamp,
            CancellationToken cancellationToken)
        {
            lock (_transitionLock)
            {
                if (applicationUserId != User.Id)
                {
                    return Task.FromResult(false);
                }

                User.RotateSecurityStamp(newSecurityStamp);
                RotateSecurityStampCallCount++;
                return Task.FromResult(true);
            }
        }
    }

    private sealed class DeterministicOperationGate
    {
        private readonly TaskCompletionSource _paused = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _released = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task WaitUntilPausedAsync() =>
            _paused.Task.WaitAsync(TimeSpan.FromSeconds(10));

        public void Release() => _released.TrySetResult();

        public async Task PauseAsync(CancellationToken cancellationToken)
        {
            _paused.TrySetResult();
            await _released.Task.WaitAsync(cancellationToken);
        }
    }
}
