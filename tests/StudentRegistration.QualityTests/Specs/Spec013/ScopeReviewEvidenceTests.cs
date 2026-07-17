using System.Reflection;
using System.Text.Json;
using StudentRegistration.Infrastructure.SqlServer.Registration;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec013;

public sealed class ScopeReviewEvidenceTests
{
    [Fact]
    public void Optimizer_is_bounded_deterministic_code_without_ml_or_ortools()
    {
        var configuration = new OptimizerConfiguration(
            "1.0.0",
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            "SPEC-013 Gate A");

        Assert.Equal(
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ],
            configuration.FactorOrder);

        var optimizer = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Domain/ScheduleOptimizer.cs");
        var scorer = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Domain/ScheduleScorer.cs");
        RepositoryFiles.ContainsAll(
            optimizer,
            "MaximumVisitedNodes = 100_000",
            ".Take(3)",
            "StringComparison.Ordinal",
            "Comparer<ScheduleCandidateGroup[]>.Create");
        RepositoryFiles.ContainsAll(
            scorer,
            "PreferenceViolations",
            "IdleMinutes",
            "TeachingDays",
            "StableGroupTuple");

        var projectFiles = Directory.GetFiles(
                RepositoryFiles.Root,
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(File.ReadAllText);
        var dependencyDeclarations = string.Join('\n', projectFiles);
        DoesNotContainAny(
            dependencyDeclarations,
            "Google.OrTools",
            "Microsoft.ML",
            "ML.NET",
            "TensorFlow",
            "TorchSharp",
            "OnnxRuntime");

        var referencedAssemblies = typeof(ScheduleOptimizer).Assembly
            .GetReferencedAssemblies()
            .Concat(typeof(ScheduleRecommendationSqlServerAdapter).Assembly
                .GetReferencedAssemblies())
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(
            referencedAssemblies,
            name => name.Contains("OrTools", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Microsoft.ML", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Onnx", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Tensor", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Torch", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Delivered_surface_is_student_plan_only_with_no_resource_writer()
    {
        using var endpointManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/endpoint-manifest.json"));
        var endpoints = endpointManifest.RootElement
            .GetProperty("endpoints")
            .EnumerateArray()
            .Where(endpoint => endpoint.GetProperty("owner").GetString() == "013")
            .Select(endpoint =>
                $"{endpoint.GetProperty("method").GetString()} " +
                endpoint.GetProperty("path").GetString())
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            [
                "POST /api/student/terms/{termId}/registration-plan/recommendations",
                "PUT /api/student/terms/{termId}/registration-plan/recommended-option"
            ],
            endpoints);

        using var routeManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/route-manifest.json"));
        var routes = routeManifest.RootElement
            .GetProperty("routes")
            .EnumerateArray()
            .Where(route => route.GetProperty("owners")
                .EnumerateArray()
                .Any(owner => owner.GetString() == "013"))
            .ToArray();
        var route = Assert.Single(routes);
        Assert.Equal("STU-04", route.GetProperty("id").GetString());
        Assert.Equal("/student/schedule", route.GetProperty("template").GetString());
        Assert.Equal("012", route.GetProperty("implementationOwner").GetString());

        using var persistenceManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/persistence-manifest.json"));
        Assert.False(
            persistenceManifest.RootElement
                .GetProperty("contributions")
                .TryGetProperty("013", out _));
        Assert.DoesNotContain(
            persistenceManifest.RootElement
                .GetProperty("migrations")
                .EnumerateArray(),
            migration =>
                migration.GetProperty("owner").GetString() == "013" ||
                migration.GetProperty("prerequisiteSpecs")
                    .EnumerateArray()
                    .Any(prerequisite => prerequisite.GetString() == "013"));

        Assert.Equal(
            ["ReadAsync"],
            DeclaredPublicMethodNames<IRecommendationSnapshotReader>());
        Assert.Equal(
            ["ReplaceAsync"],
            DeclaredPublicMethodNames<IRecommendationPlanWriter>());

        var adapter = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/ScheduleRecommendationSqlServerAdapter.cs");
        RepositoryFiles.ContainsAll(
            adapter,
            "IRecommendationSnapshotReader",
            "IRecommendationPlanWriter",
            "AsNoTracking",
            "plan.ReplaceSelections");
        DoesNotContainAny(
            adapter,
            "OfferingPublicationService",
            "ResourceAvailabilityService",
            "UpdateGroupRequest",
            "ExecuteUpdate",
            "ExecuteDelete");
    }

    [Fact]
    public void Applying_an_option_changes_the_plan_without_reserving_a_seat()
    {
        Assert.Contains(
            typeof(RecommendationApplicationService).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            method => method.Name == "ApplyAsync");
        Assert.DoesNotContain(
            typeof(RecommendationApplicationService).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            method => method.Name.Contains("reserv", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(RecommendationPlanReplacement).GetProperties(),
            property => ContainsAny(
                property.Name,
                "Seat",
                "Reservation",
                "Enrollment",
                "Capacity",
                "Room",
                "Staff",
                "Resource"));

        var adapter = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/ScheduleRecommendationSqlServerAdapter.cs");
        RepositoryFiles.ContainsAll(
            adapter,
            "group.EnrolledCount < group.Capacity",
            "plan.ReplaceSelections",
            "SaveChangesAsync");
        DoesNotContainAny(
            adapter,
            "EnrolledCount++",
            "RemainingSeats--",
            "new Enrollment",
            "SeatReservation",
            "ReserveAsync");
    }

    [Fact]
    public void Evidence_document_binds_each_exclusion_to_delivered_artifacts()
    {
        var evidence = RepositoryFiles.Read(
            "docs/release-evidence/SPEC-013-scope-review.md");

        RepositoryFiles.ContainsAll(
            evidence,
            "T061",
            "T062",
            "T063",
            "T064",
            "OS-1",
            "OS-2",
            "OS-3",
            "OS-4",
            "ScheduleOptimizer",
            "ScheduleScorer",
            "ScheduleRecommendationSqlServerAdapter",
            "IRecommendationSnapshotReader",
            "IRecommendationPlanWriter",
            "no seat reservation",
            "no SPEC-013 persistence contribution",
            "No packages were found",
            "ScopeReviewEvidenceTests");
    }

    private static string[] DeclaredPublicMethodNames<T>() =>
        typeof(T).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(candidate =>
            value.Contains(candidate, StringComparison.OrdinalIgnoreCase));

    private static void DoesNotContainAny(
        string value,
        params string[] excludedValues)
    {
        foreach (var excluded in excludedValues)
        {
            Assert.DoesNotContain(
                excluded,
                value,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
