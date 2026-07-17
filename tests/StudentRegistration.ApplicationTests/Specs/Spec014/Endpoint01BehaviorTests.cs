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

    private static string FutureEndpointSource()
    {
        Assert.True(
            Spec014BehaviorFiles.Exists(EndpointPath),
            "Expected red for T026: Spec014Endpoints/POST handler is intentionally absent until T110.");
        return Spec014BehaviorFiles.Read(EndpointPath);
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
