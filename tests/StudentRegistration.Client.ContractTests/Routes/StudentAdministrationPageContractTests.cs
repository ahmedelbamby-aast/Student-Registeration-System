using System.Reflection;
using System.Text.Json;
using StudentRegistration.Client;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StudentAdministrationPageContractTests
{
    private const string FixturePath =
        "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec008/ADM-04/route-contract.json";
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor";
    private const string ApiClientPath =
        "src/StudentRegistration.Client/Features/Academics/AcademicApiClient.cs";

    [Fact]
    public void Adm04_fixture_freezes_routes_bounds_states_journeys_and_privacy_purge()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(FixturePath));
        var root = document.RootElement;

        Assert.Equal("frontend-fixture/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("ADM-04", root.GetProperty("routeId").GetString());
        Assert.Equal("/admin/students", root.GetProperty("routeTemplate").GetString());
        Assert.Equal("AcademicProfiles.Manage", root.GetProperty("permission").GetString());

        var api = root.GetProperty("api");
        AssertEndpoint(api, "search", "GET", "/api/admin/students");
        AssertEndpoint(
            api,
            "detail",
            "GET",
            "/api/admin/students/{studentId}/academic-context");
        AssertEndpoint(
            api,
            "correction",
            "PATCH",
            "/api/admin/students/{studentId}/academic-profile");
        Assert.Equal(
            "X-XSRF-TOKEN",
            api.GetProperty("correction").GetProperty("antiforgeryHeader").GetString());
        Assert.Equal(
            ["expectedStudentRowVersion", "expectedStudentTermStateRowVersion"],
            Strings(api.GetProperty("correction"), "expectedVersions"));

        var bounds = root.GetProperty("bounds");
        Assert.Equal((3, 50), (
            bounds.GetProperty("searchMinimum").GetInt32(),
            bounds.GetProperty("searchMaximum").GetInt32()));
        Assert.Equal((1, 20, 100), (
            bounds.GetProperty("defaultPage").GetInt32(),
            bounds.GetProperty("defaultPageSize").GetInt32(),
            bounds.GetProperty("maximumPageSize").GetInt32()));
        Assert.Equal((10, 500), (
            bounds.GetProperty("reasonMinimum").GetInt32(),
            bounds.GetProperty("reasonMaximum").GetInt32()));
        Assert.Equal((1, 20), (
            bounds.GetProperty("operationMinimum").GetInt32(),
            bounds.GetProperty("operationMaximum").GetInt32()));

        Assert.Equal(
            [
                "loading", "empty", "success", "validation-error", "service-error",
                "unauthorized", "session-expired", "stale", "offline"
            ],
            root.GetProperty("states").EnumerateArray()
                .Select(state => state.GetProperty("state").GetString()!)
                .ToArray());
        Assert.Equal(
            [
                "ADM-04-COMP-STATE-LOADING",
                "ADM-04-COMP-STATE-EMPTY",
                "ADM-04-COMP-STATE-SUCCESS",
                "ADM-04-COMP-STATE-VALIDATION-ERROR",
                "ADM-04-COMP-STATE-SERVICE-ERROR",
                "ADM-04-COMP-STATE-UNAUTHORIZED",
                "ADM-04-COMP-STATE-SESSION-EXPIRED",
                "ADM-04-COMP-STATE-STALE",
                "ADM-04-COMP-STATE-OFFLINE"
            ],
            root.GetProperty("states").EnumerateArray()
                .Select(state => state.GetProperty("testId").GetString()!)
                .ToArray());
        Assert.Equal(
            [
                "ADM-04-read-v1", "ADM-04-no-results-v1",
                "ADM-04-invalid-correction-v1", "ADM-04-stale-v1",
                "ADM-04-reason-required-v1", "ADM-04-restricted-v1"
            ],
            root.GetProperty("journeys").EnumerateArray()
                .Select(journey => journey.GetProperty("fixture").GetString()!)
                .ToArray());
        Assert.Equal(
            ["unauthorized", "session-expired"],
            Strings(root.GetProperty("privacy"), "purgeStates"));
        Assert.Equal([320, 375, 768, 1024, 1280, 1920], Integers(root, "responsiveWidths"));
    }

    [Fact]
    public void Admin_locator_and_correction_are_the_bounded_named_contracts()
    {
        Assert.Equal(
            ["Cohort", "DataVersion", "ProgramCode", "Standing", "StudentId", "UniversityId"],
            PublicProperties<AdminStudentLocatorDto>());
        Assert.Equal(
            [
                "ExpectedStudentRowVersion", "ExpectedStudentTermStateRowVersion",
                "Operations", "Reason", "Source", "TermId"
            ],
            PublicProperties<AcademicProfileCorrectionRequest>());

        Assert.Throws<ArgumentOutOfRangeException>(() => new AcademicProfileCorrectionRequest(
            "term-2026-fall",
            "student-rv-1",
            "term-state-rv-1",
            "too short",
            "ADM-04",
            [new SetGpaOperation(3.2m, "source-ref")]));
        Assert.Throws<ArgumentException>(() => new AcademicProfileCorrectionRequest(
            "term-2026-fall",
            "student-rv-1",
            "term-state-rv-1",
            "Reason is valid",
            "ADM-04",
            []));
    }

    [Fact]
    public void Academic_api_facade_exposes_term_scoped_reads_and_an_xsrf_versioned_patch()
    {
        var source = RepositoryFiles.Read(ApiClientPath);
        var type = typeof(App).Assembly.GetType(
            "StudentRegistration.Client.Features.Academics.AcademicApiClient");
        Assert.NotNull(type);

        AssertMethod(type!, "SearchAdminStudentsAsync");
        AssertMethod(type!, "GetAdminStudentAcademicContextAsync");
        AssertMethod(type!, "CorrectAdminStudentAcademicProfileAsync");
        RepositoryFiles.ContainsAll(
            source,
            "/api/admin/students",
            "/academic-context",
            "/academic-profile",
            "HttpMethod.Get",
            "HttpMethod.Patch",
            "AcademicProfileCorrectionRequest",
            "X-XSRF-TOKEN",
            "StudentRegistration.antiforgery.getRequestToken",
            "Uri.EscapeDataString");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("role=", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Adm04_route_binds_the_three_owner_calls_without_client_role_or_clock_authority()
    {
        var source = RequiredSource(PagePath, "SPEC-008/T082");

        RepositoryFiles.ContainsAll(
            source,
            "@page \"/admin/students\"",
            "Student administration",
            "SearchAdminStudentsAsync",
            "GetAdminStudentAcademicContextAsync",
            "CorrectAdminStudentAcademicProfileAsync",
            "AcademicProfileCorrectionRequest",
            "STALE_VERSION");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IsInRole", source, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", source, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertEndpoint(
        JsonElement api,
        string name,
        string method,
        string path)
    {
        var endpoint = api.GetProperty(name);
        Assert.Equal(method, endpoint.GetProperty("method").GetString());
        Assert.Equal(path, endpoint.GetProperty("path").GetString());
    }

    private static void AssertMethod(Type type, string methodName) =>
        Assert.Contains(
            type.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => string.Equals(method.Name, methodName, StringComparison.Ordinal));

    private static string[] PublicProperties<T>() => typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Select(property => property.Name)
        .Order(StringComparer.Ordinal)
        .ToArray();

    private static string[] Strings(JsonElement owner, string propertyName) => owner
        .GetProperty(propertyName)
        .EnumerateArray()
        .Select(value => value.GetString()!)
        .ToArray();

    private static int[] Integers(JsonElement owner, string propertyName) => owner
        .GetProperty(propertyName)
        .EnumerateArray()
        .Select(value => value.GetInt32())
        .ToArray();

    private static string RequiredSource(string repositoryPath, string deliveryTask)
    {
        var path = RepositoryFiles.PathTo(repositoryPath);
        Assert.True(
            File.Exists(path),
            $"{repositoryPath} has not been delivered; this is the expected red for {deliveryTask}.");
        return File.ReadAllText(path);
    }
}
