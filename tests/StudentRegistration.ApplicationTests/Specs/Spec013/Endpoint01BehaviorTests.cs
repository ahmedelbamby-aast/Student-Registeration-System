using System.Reflection;

namespace StudentRegistration.ApplicationTests.Specs.Spec013;

public sealed class Endpoint01BehaviorTests
{
    private const string EndpointPath =
        "src/StudentRegistration.Registration/Endpoints/Spec013Endpoints.cs";
    private const string CoordinatorPath =
        "src/StudentRegistration.Registration/Application/OptimizationCoordinator.cs";

    [Fact]
    public void Recommendation_endpoint_delegates_one_authenticated_self_scoped_request()
    {
        var endpoint = FutureSource(
            EndpointPath,
            "T056 must deliver the future Spec 013 handlers.");

        Spec013BehaviorFiles.ContainsAll(
            endpoint,
            "/api/student/terms/{termId}/registration-plan/recommendations",
            "RecommendScheduleRequest",
            "OptimizationCoordinator",
            "ClaimTypes.NameIdentifier",
            "RequireAuthorization",
            "Student",
            "CancellationToken");
        var parameterNames = EndpointHandlerParameters();
        Assert.False(parameterNames.Contains("studentId", StringComparison.OrdinalIgnoreCase));
        Assert.False(parameterNames.Contains("planId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Recommendation_endpoint_maps_budget_diagnostics_and_safe_failures()
    {
        var endpoint = FutureSource(
            EndpointPath,
            "T056 must map the future recommendation outcomes.");

        Spec013BehaviorFiles.ContainsAll(
            endpoint,
            "VALIDATION_ERROR",
            "REGISTRATION_CONTEXT_NOT_FOUND",
            "PLAN_CHANGED",
            "STALE_INPUT",
            "RATE_LIMITED",
            "RECOMMENDATIONS_UNAVAILABLE",
            "INTERNAL_ERROR",
            "StatusCodes.Status200OK",
            "StatusCodes.Status400BadRequest",
            "StatusCodes.Status404NotFound",
            "StatusCodes.Status409Conflict",
            "StatusCodes.Status429TooManyRequests",
            "StatusCodes.Status503ServiceUnavailable");
    }

    [Fact]
    public void Coordinator_is_bounded_cancellable_and_returns_only_complete_options()
    {
        var coordinator = FutureSource(
            CoordinatorPath,
            "T045/T047 must deliver the future bounded coordinator.");

        Spec013BehaviorFiles.ContainsAll(
            coordinator,
            "CancellationToken",
            "TimeProvider",
            "time-budget",
            "no-solution",
            "inclusion-minimal",
            "change-group",
            "remove-course",
            "ScheduleOption",
            "OptimizationDiagnostic");
        Assert.DoesNotContain("Task.Run(", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("CapacityReservation", coordinator, StringComparison.Ordinal);
    }

    private static string FutureSource(string path, string message)
    {
        Assert.True(Spec013BehaviorFiles.Exists(path), message);
        return Spec013BehaviorFiles.Read(path);
    }

    private static string EndpointHandlerParameters()
    {
        var endpointType = Assembly.Load("StudentRegistration.Registration")
            .GetType("StudentRegistration.Registration.Endpoints.Spec013Endpoints");
        Assert.NotNull(endpointType);
        var methods = endpointType!
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(method => method.GetParameters().Any(parameter =>
                parameter.ParameterType.Name == "RecommendScheduleRequest"))
            .ToArray();
        var handler = Assert.Single(methods);
        return string.Join(
            ",",
            handler.GetParameters().Select(parameter => parameter.Name));
    }
}

internal static class Spec013BehaviorFiles
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
