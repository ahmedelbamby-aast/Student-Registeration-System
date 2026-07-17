using System.Security.Claims;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.ApplicationTests.Infrastructure;
using StudentRegistration.Registration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec014;

public sealed class Endpoint01BehaviorTests
{
    private const string EndpointPath =
        "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs";

    [Fact]
    public void Post_handler_uses_authenticated_identity_exact_permission_and_antiforgery()
    {
        var endpoint = FutureEndpointSource();

        Spec014BehaviorFiles.ContainsAll(
            endpoint,
            "/api/student/terms/{termId}/registrations",
            "SubmitRegistrationRequest",
            "ClaimTypes.NameIdentifier",
            "Student",
            "Registration.SubmitOwn",
            "RequireAuthorization",
            "RequireAntiforgeryTokenAttribute");
        Assert.DoesNotContain("request.StudentId", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("request.TermId", endpoint, StringComparison.Ordinal);
    }

    [Fact]
    public void Post_handler_maps_new_replay_processing_and_conflict_outcomes()
    {
        var endpoint = FutureEndpointSource();

        Spec014BehaviorFiles.ContainsAll(
            endpoint,
            "StatusCodes.Status201Created",
            "StatusCodes.Status200OK",
            "StatusCodes.Status202Accepted",
            "StatusCodes.Status400BadRequest",
            "StatusCodes.Status401Unauthorized",
            "StatusCodes.Status403Forbidden",
            "StatusCodes.Status409Conflict",
            "ANTIFORGERY_INVALID",
            "IDEMPOTENCY_KEY_REUSED",
            "GROUP_FULL",
            "PLAN_CHANGED",
            "POLICY_CHANGED");
    }

    [Fact]
    public void Post_handler_delegates_without_implementing_registration_in_the_endpoint()
    {
        var endpoint = FutureEndpointSource();

        Spec014BehaviorFiles.ContainsAll(
            endpoint,
            "RegistrationTransactionCoordinator",
            "CancellationToken");
        Assert.DoesNotContain("EnrolledCount++", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChangesAsync", endpoint, StringComparison.Ordinal);
        Assert.DoesNotContain("BeginTransaction", endpoint, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Authenticated_ingress_time_is_captured_before_context_resolution_io()
    {
        var close = new DateTimeOffset(2026, 7, 17, 10, 0, 0, TimeSpan.Zero);
        var received = close.AddMilliseconds(-1);
        var time = new MutableTimeProvider(received);
        var applicationUserId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var endpointStore = new CrossingCutoffEndpointStore(
            time,
            new RegistrationCommandContext(
                new ResolvedRegistrationContext(
                    applicationUserId,
                    Guid.NewGuid(),
                    termId,
                    "WINDOW-V1",
                    close.AddHours(-1).UtcDateTime,
                    close.UtcDateTime,
                    EmergencyClosed: false),
                "ACADEMIC-V1"));
        var service = new RegistrationEndpointService(
            new RegistrationCommandFactory(time),
            endpointStore,
            time);
        var coordinator = new RegistrationTransactionCoordinator(
            new StudentAcademicProfileService(new NeverAcademicStore(), time));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, applicationUserId.ToString())],
            "SPEC014-Test"));

        var result = await service.SubmitAsync(
            principal,
            termId,
            new SubmitRegistrationRequest(Guid.NewGuid(), "AQ==", Guid.NewGuid()),
            coordinator);

        Assert.Equal(RegistrationEndpointOutcome.Conflict, result.Outcome);
        Assert.NotNull(endpointStore.Submitted);
        Assert.Equal(received.UtcDateTime, endpointStore.Submitted!.ReceivedAtUtc);
        Assert.True(time.UtcNow > close);
    }

    private static string FutureEndpointSource()
    {
        Assert.True(
            Spec014BehaviorFiles.Exists(EndpointPath),
            "Expected red for T026: Spec014Endpoints/POST handler is intentionally absent until T110.");
        return Spec014BehaviorFiles.Read(EndpointPath);
    }

    private sealed class CrossingCutoffEndpointStore(
        MutableTimeProvider time,
        RegistrationCommandContext context) : IRegistrationEndpointStore
    {
        public RegistrationCommand? Submitted { get; private set; }

        public Task<RegistrationCommandContext?> ResolveCommandContextAsync(
            Guid applicationUserId,
            Guid termId,
            CancellationToken cancellationToken = default)
        {
            time.UtcNow = time.UtcNow.AddMinutes(1);
            return Task.FromResult<RegistrationCommandContext?>(context);
        }

        public Task<RegistrationEndpointResult> SubmitAsync(
            RegistrationCommand command,
            RegistrationTransactionCoordinator coordinator,
            CancellationToken cancellationToken = default)
        {
            Submitted = command;
            return Task.FromResult(new RegistrationEndpointResult(
                RegistrationEndpointOutcome.Conflict,
                ErrorCode: "GROUP_FULL"));
        }

        public Task<RegistrationEndpointResult> LookupAsync(
            Guid applicationUserId,
            Guid termId,
            Guid clientRequestId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class NeverAcademicStore : IStudentAcademicProfileStore
    {
        public Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
            Guid applicationUserId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) => Never();

        public Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
            Guid studentId,
            Guid termId,
            AcademicProfileReadRequest request,
            CancellationToken cancellationToken = default) => Never();

        public Task<AcademicProfileStoreResult> CorrectAsync(
            CorrectAcademicProfileStoreCommand command,
            CancellationToken cancellationToken = default) => Never();

        public Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
            RegistrationBoundaryStoreCommand command,
            Func<CancellationToken, Task> commitCallback,
            CancellationToken cancellationToken = default) => Never();

        private static Task<AcademicProfileStoreResult> Never() =>
            throw new InvalidOperationException("The endpoint-store double owns this test.");
    }
}

internal static class Spec014BehaviorFiles
{
    private static readonly string Root = FindRoot();

    public static bool Exists(string relativePath) =>
        File.Exists(Path.Combine(Root, Normalize(relativePath)));

    public static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Root, Normalize(relativePath)));

    public static void ContainsAll(string value, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            Assert.Contains(fragment, value, StringComparison.Ordinal);
        }
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "StudentRegistration.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);
}
