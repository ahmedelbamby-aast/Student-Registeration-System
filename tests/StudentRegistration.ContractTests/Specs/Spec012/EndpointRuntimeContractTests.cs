using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Registration.Endpoints;

namespace StudentRegistration.ContractTests.Specs.Spec012;

public sealed class EndpointRuntimeContractTests
{
    private static readonly Guid ApplicationUserId = Id(1);
    private static readonly Guid OtherApplicationUserId = Id(2);
    private static readonly Guid StudentId = Id(3);
    private static readonly Guid TermId = Id(4);
    private static readonly Guid OtherTermId = Id(5);

    [Fact]
    public async Task Get_returns_private_empty_and_current_fixed_load_views()
    {
        await using var fixture = await Fixture.StartAsync(StandardGroups());

        using var otherOwner = Request(
            HttpMethod.Get,
            Route(TermId),
            applicationUserId: OtherApplicationUserId);
        using var otherOwnerResponse = await fixture.Client.SendAsync(otherOwner);
        await AssertPrivateNotFound(otherOwnerResponse);

        using var otherTerm = Request(HttpMethod.Get, Route(OtherTermId));
        using var otherTermResponse = await fixture.Client.SendAsync(otherTerm);
        await AssertPrivateNotFound(otherTermResponse);

        using var emptyRequest = Request(HttpMethod.Get, Route(TermId));
        using var emptyResponse = await fixture.Client.SendAsync(emptyRequest);
        Assert.Equal(HttpStatusCode.OK, emptyResponse.StatusCode);
        var empty = await Read<RegistrationPlanDto>(emptyResponse);
        Assert.Empty(empty.SelectedGroups);
        Assert.Equal(0m, empty.TotalCredits);
        Assert.Equal(RegistrationPlanService.InitialEmptyVersion, empty.RowVersion);
        AssertFixedLoad(empty);
        Assert.Equal(0, fixture.Store.SuccessfulMutations);

        using var put = Request(
            HttpMethod.Put,
            Route(TermId),
            new RegistrationPlanMutationRequest(
                RegistrationPlanService.InitialEmptyVersion,
                [Id(20), Id(21)]));
        using var putResponse = await fixture.Client.SendAsync(put);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        using var currentRequest = Request(HttpMethod.Get, Route(TermId));
        using var currentResponse = await fixture.Client.SendAsync(currentRequest);
        Assert.Equal(HttpStatusCode.OK, currentResponse.StatusCode);
        var current = await Read<RegistrationPlanDto>(currentResponse);
        Assert.Equal([Id(20), Id(21)], current.SelectedGroups.Select(group => group.GroupId));
        Assert.Equal(6m, current.TotalCredits);
        AssertFixedLoad(current);
        Assert.Equal(1, fixture.Store.SuccessfulMutations);
        Assert.Equal(0, fixture.Context.SeatMutationCalls);
    }

    [Fact]
    public async Task Put_is_complete_and_rejects_duplicate_overload_and_stale_without_mutation()
    {
        var groups = StandardGroups()
            .Concat(Enumerable.Range(30, 7).Select(value =>
                Group(value, value, $"C{value}", 3m)))
            .Append(Group(20, 22, "CS101", 3m))
            .ToArray();
        await using var fixture = await Fixture.StartAsync(groups);

        var first = await PutPlan(
            fixture,
            RegistrationPlanService.InitialEmptyVersion,
            [Id(20)]);
        Assert.Equal([Id(20)], first.SelectedGroups.Select(group => group.GroupId));
        Assert.Equal(1, fixture.Store.SuccessfulMutations);

        var second = await PutPlan(fixture, first.RowVersion, [Id(21)]);
        Assert.Equal([Id(21)], second.SelectedGroups.Select(group => group.GroupId));
        Assert.DoesNotContain(second.SelectedGroups, group => group.GroupId == Id(20));
        Assert.Equal(2, fixture.Store.SuccessfulMutations);
        AssertFixedLoad(second);

        var writesBeforeRejections = fixture.Store.SuccessfulMutations;
        using var duplicateRequest = Request(
            HttpMethod.Put,
            Route(TermId),
            new RegistrationPlanMutationRequest(second.RowVersion, [Id(20), Id(22)]));
        using var duplicateResponse = await fixture.Client.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        var duplicateError = await Read<ApiError>(duplicateResponse);
        Assert.Equal("DUPLICATE_OFFERING_SELECTION", duplicateError.Code);
        AssertSafe(duplicateError);
        Assert.Equal(writesBeforeRejections, fixture.Store.SuccessfulMutations);

        using var overloadRequest = Request(
            HttpMethod.Put,
            Route(TermId),
            new RegistrationPlanMutationRequest(
                second.RowVersion,
                Enumerable.Range(30, 7).Select(Id).ToArray()));
        using var overloadResponse = await fixture.Client.SendAsync(overloadRequest);
        Assert.Equal(HttpStatusCode.Conflict, overloadResponse.StatusCode);
        var overloadError = await Read<ApiError>(overloadResponse);
        Assert.Equal("LOAD_ABOVE_MAXIMUM", overloadError.Code);
        AssertSafe(overloadError);
        Assert.Equal(writesBeforeRejections, fixture.Store.SuccessfulMutations);

        using var staleRequest = Request(
            HttpMethod.Put,
            Route(TermId),
            new RegistrationPlanMutationRequest(first.RowVersion, [Id(20)]));
        using var staleResponse = await fixture.Client.SendAsync(staleRequest);
        Assert.Equal(HttpStatusCode.Conflict, staleResponse.StatusCode);
        var stale = await Read<StaleRegistrationPlanResponse>(staleResponse);
        Assert.Equal("STALE_VERSION", stale.Error.Code);
        Assert.Equal(second.RowVersion, stale.Error.CurrentVersion);
        Assert.Equal(second.RowVersion, stale.CurrentPlan.RowVersion);
        Assert.Equal([Id(21)], stale.CurrentPlan.SelectedGroups.Select(group => group.GroupId));
        AssertFixedLoad(stale.CurrentPlan);
        Assert.Equal(writesBeforeRejections, fixture.Store.SuccessfulMutations);
        Assert.Equal(0, fixture.Context.SeatMutationCalls);
    }

    [Fact]
    public async Task Bodyless_validate_returns_current_snapshot_without_mutation_or_seat_effect()
    {
        await using var fixture = await Fixture.StartAsync(
            [Group(20, 20, "CS101", 3m, capacity: 1, enrolled: 1)]);
        var current = await PutPlan(
            fixture,
            RegistrationPlanService.InitialEmptyVersion,
            [Id(20)]);
        var writesBeforeValidation = fixture.Store.SuccessfulMutations;

        using var validateRequest = Request(
            HttpMethod.Post,
            $"{Route(TermId)}/validate");
        Assert.Null(validateRequest.Content);
        using var validateResponse = await fixture.Client.SendAsync(validateRequest);
        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);
        var validated = await Read<RegistrationPlanDto>(validateResponse);

        Assert.Equal(current.RowVersion, validated.RowVersion);
        Assert.Equal([Id(20)], validated.SelectedGroups.Select(group => group.GroupId));
        Assert.Contains(validated.SelectionIssues, issue => issue.Code == "GROUP_FULL");
        Assert.True(validated.ReviewBlocked);
        AssertFixedLoad(validated);
        Assert.Equal(writesBeforeValidation, fixture.Store.SuccessfulMutations);
        Assert.Equal(0, fixture.Context.SeatMutationCalls);
    }

    [Fact]
    public async Task Put_dependency_failure_is_safe_and_does_not_claim_a_mutation()
    {
        await using var fixture = await Fixture.StartAsync(StandardGroups());
        fixture.Store.FailNextReplace = true;

        using var request = Request(
            HttpMethod.Put,
            Route(TermId),
            new RegistrationPlanMutationRequest(
                RegistrationPlanService.InitialEmptyVersion,
                [Id(20)]));
        using var response = await fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var error = await Read<ApiError>(response);
        Assert.Equal("REGISTRATION_PLAN_UNAVAILABLE", error.Code);
        AssertSafe(error);
        Assert.Equal(0, fixture.Store.SuccessfulMutations);
        Assert.Equal(0, fixture.Context.SeatMutationCalls);
    }

    private static async Task<RegistrationPlanDto> PutPlan(
        Fixture fixture,
        string expectedVersion,
        IReadOnlyList<Guid> groupIds)
    {
        using var request = Request(
            HttpMethod.Put,
            Route(TermId),
            new RegistrationPlanMutationRequest(expectedVersion, groupIds));
        using var response = await fixture.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await Read<RegistrationPlanDto>(response);
    }

    private static void AssertFixedLoad(RegistrationPlanDto plan)
    {
        Assert.Equal(18m, plan.DefaultTargetCredits);
        Assert.Equal(18m, plan.MaximumAllowedCredits);
        var reason = Assert.Single(plan.LoadReasons);
        Assert.Equal(FakeContext.PolicySetId, reason.PolicySetId);
        Assert.Equal("DEMO-POC-2026.1", reason.PolicyVersion);
        Assert.Equal("DEMO-APPROVAL-2026.1", reason.SourceReference);
        Assert.False(string.IsNullOrWhiteSpace(reason.Message));
    }

    private static async Task AssertPrivateNotFound(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("REGISTRATION_CONTEXT_NOT_FOUND", error.Code);
        AssertSafe(error);
        Assert.DoesNotContain(StudentId.ToString("D"), body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(ApplicationUserId.ToString("D"), body, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertSafe(ApiError error)
    {
        Assert.False(string.IsNullOrWhiteSpace(error.CorrelationId));
        Assert.DoesNotContain("sql", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exception", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("student", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>();
        Assert.NotNull(payload);
        return payload!;
    }

    private static HttpRequestMessage Request(
        HttpMethod method,
        string route,
        Guid? applicationUserId = null)
    {
        var request = new HttpRequestMessage(method, route);
        request.Headers.TryAddWithoutValidation(
            TestAuthenticationHandler.UserHeader,
            (applicationUserId ?? ApplicationUserId).ToString("D"));
        return request;
    }

    private static HttpRequestMessage Request<T>(
        HttpMethod method,
        string route,
        T body,
        Guid? applicationUserId = null)
    {
        var request = Request(method, route, applicationUserId);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private static string Route(Guid termId) =>
        $"/api/student/terms/{termId:D}/registration-plan";

    private static IReadOnlyList<RegistrationPlanGroupSnapshot> StandardGroups() =>
        [
            Group(20, 20, "CS101", 3m),
            Group(21, 21, "DS201", 3m)
        ];

    private static RegistrationPlanGroupSnapshot Group(
        int offering,
        int group,
        string courseCode,
        decimal credits,
        int capacity = 30,
        int enrolled = 10) =>
        new(
            Id(offering),
            Id(group),
            $"G{group}",
            courseCode,
            $"{courseCode} title",
            credits,
            "published",
            false,
            capacity,
            enrolled,
            $"offering/{offering}",
            $"group/{group}",
            [
                new(
                    Id(500 + group),
                    DayOfWeek.Monday,
                    new TimeOnly(8 + group % 4, 0),
                    new TimeOnly(8 + group % 4, 50),
                    "Lecture",
                    "A-101",
                    "Smart Village",
                    "Dr. Salma",
                    [],
                    "Africa/Cairo")
            ]);

    private static Guid Id(int value) =>
        Guid.Parse($"01241000-0000-0000-0000-{value:000000000000}");

    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(
            WebApplication application,
            HttpClient client,
            FakeStore store,
            FakeContext context)
        {
            Application = application;
            Client = client;
            Store = store;
            Context = context;
        }

        public WebApplication Application { get; }
        public HttpClient Client { get; }
        public FakeStore Store { get; }
        public FakeContext Context { get; }

        public static async Task<Fixture> StartAsync(
            IReadOnlyList<RegistrationPlanGroupSnapshot> groups)
        {
            var store = new FakeStore();
            var context = new FakeContext(groups);
            var builder = WebApplication.CreateBuilder(
                new WebApplicationOptions { EnvironmentName = "Testing" });
            builder.WebHost.UseTestServer();
            builder.Services.AddRouting();
            builder.Services
                .AddAuthentication(TestAuthenticationHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.AuthenticationScheme,
                    _ => { });
            builder.Services.AddAuthorization(options =>
                options.AddPolicy("Student", policy =>
                    policy.RequireAuthenticatedUser().RequireRole("Student")));
            builder.Services.AddSingleton<IRegistrationPlanOwnerReader>(new FakeOwner());
            builder.Services.AddSingleton<IRegistrationPlanStore>(store);
            builder.Services.AddSingleton<IRegistrationPlanContextReader>(context);
            builder.Services.AddSingleton<ScheduleConflictDetector>();
            builder.Services.AddSingleton<TimeProvider>(new FixedTimeProvider());
            builder.Services.AddScoped<RegistrationPlanService>();

            var application = builder.Build();
            application.UseAuthentication();
            application.UseAuthorization();
            application.MapSpec012Endpoints();
            await application.StartAsync();
            return new(application, application.GetTestClient(), store, context);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Application.DisposeAsync();
        }
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "Spec012Test";
        public const string UserHeader = "X-SPEC012-USER";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserHeader, out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, userId.ToString()),
                    new(ClaimTypes.Role, "Student")
                ],
                AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(identity),
                    AuthenticationScheme)));
        }
    }

    private sealed class FakeOwner : IRegistrationPlanOwnerReader
    {
        public Task<Guid?> ResolveStudentIdAsync(
            Guid applicationUserId,
            Guid termId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(
                applicationUserId == ApplicationUserId && termId == TermId
                    ? StudentId
                    : null);
    }

    private sealed class FakeContext(
        IReadOnlyList<RegistrationPlanGroupSnapshot> groups)
        : IRegistrationPlanContextReader
    {
        public static Guid PolicySetId { get; } = Id(600);
        public int SeatMutationCalls => 0;

        public Task<RegistrationPlanContextSnapshot?> ReadAsync(
            Guid studentId,
            Guid termId,
            IReadOnlyCollection<Guid> groupIds,
            DateTime evaluatedAtUtc,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<RegistrationPlanContextSnapshot?>(
                studentId == StudentId && termId == TermId
                    ? new(
                        "academic/1",
                        PolicySetId,
                        "DEMO-POC-2026.1",
                        "DEMO-APPROVAL-2026.1",
                        "catalogue/1",
                        groups.Where(group => groupIds.Contains(group.GroupId)).ToArray())
                    : null);
    }

    private sealed class FakeStore : IRegistrationPlanStore
    {
        private RegistrationPlan? _plan;
        private int _version;

        public bool FailNextReplace { get; set; }
        public int SuccessfulMutations { get; private set; }

        public Task<RegistrationPlan?> ReadAsync(
            Guid studentId,
            Guid termId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _plan is not null &&
                _plan.StudentId == studentId &&
                _plan.TermId == termId
                    ? _plan
                    : null);

        public Task<RegistrationPlanStoreResult> ReplaceAsync(
            RegistrationPlanStoreCommand command,
            CancellationToken cancellationToken = default)
        {
            if (FailNextReplace)
            {
                FailNextReplace = false;
                return Task.FromResult(new RegistrationPlanStoreResult(
                    RegistrationPlanStoreOutcome.StorageUnavailable));
            }

            var currentVersion = _plan is null
                ? RegistrationPlanService.InitialEmptyVersion
                : Convert.ToBase64String(_plan.Version);
            if (!string.Equals(command.ExpectedRowVersion, currentVersion, StringComparison.Ordinal))
            {
                return Task.FromResult(new RegistrationPlanStoreResult(
                    RegistrationPlanStoreOutcome.StaleVersion,
                    _plan));
            }

            _plan ??= new(
                Id(900),
                command.StudentId,
                command.TermId,
                0m,
                RegistrationPlanState.Draft);
            _plan.ReplaceSelections(
                command.Selections,
                command.TotalCredits,
                command.State,
                command.Conflicts,
                command.Validation);
            _version++;
            typeof(RegistrationPlan)
                .GetProperty(nameof(RegistrationPlan.Version), BindingFlags.Instance | BindingFlags.Public)!
                .SetValue(_plan, BitConverter.GetBytes(_version));
            SuccessfulMutations++;
            return Task.FromResult(new RegistrationPlanStoreResult(
                RegistrationPlanStoreOutcome.Updated,
                _plan));
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 16, 12, 0, 0, TimeSpan.Zero);
    }
}
